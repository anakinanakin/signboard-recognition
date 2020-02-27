#!/usr/local/bin/python2.7
#python testSiftBow.py -i dataset/test

import argparse as ap
import cv2
import imutils
import yaml
import numpy as np
import os
import matplotlib
matplotlib.use('TkAgg')
from sklearn.externals import joblib
from scipy.cluster.vq import *
from sklearn import preprocessing
import numpy as np
from pylab import *
from PIL import Image

# check if parser argument exists
def is_valid_file(parser, arg):
    if not os.path.exists(arg):
        parser.error("The file %s does not exist!" % arg)
    else:
        return arg

# Get the path of the test set
parser = ap.ArgumentParser()
parser.add_argument("-i", "--image", help="Path to query image set", required="True", type=lambda x: is_valid_file(parser, x))
args = vars(parser.parse_args())

# Get query image set path
image_file_path = args["image"]

image_names = os.listdir(image_file_path)
# Get all the path to the test images and save them in a list
# test_image_paths and the corresponding label in test_image_paths
test_image_paths = []
images = []
for image_name in image_names:
    # exclude non image file
    if os.path.splitext(image_name)[1] in ['.jpg', '.PNG', '.png']:
        test_image_path = os.path.join(image_file_path, image_name)
        test_image_paths += [test_image_path]
        images += [image_name]
    else:
        print image_name+" is not an image file"

# Load the classifier, class names, scaler, number of clusters and vocabulary
im_features, image_paths, idf, numWords, voc, train_path = joblib.load("store1-augment.pkl")

# read yaml and databaseImage
with open("newTest/numrect=1000/store1/result-augment/night.jpg-roi.yaml", 'r') as f:
    doc = yaml.load(f)
databaseIm = cv2.imread(doc["databaseImage"])
#height = 700
#width = int(databaseIm.shape[1]*700/databaseIm.shape[0])
#databaseIm = cv2.resize(databaseIm, (width, height))
databaseIm = cv2.resize(databaseIm, (0,0), fx=0.3, fy=0.3) 

# Create feature extraction and keypoint detector objects
sift = cv2.xfeatures2d.SIFT_create()

largestCtr = 0;

maxScore = 0;

# test each image and output result
for image_path in test_image_paths:
    #ignore small images(or error occurs)
    if os.path.getsize(image_path) < 11000:
        continue
    # List where all the descriptors are stored
    des_list = []

    im = cv2.imread(image_path)
    kpts = sift.detect(im)
    kpts, des = sift.compute(im, kpts)

    des_list.append((image_path, des))

    # Stack all the descriptors vertically in a numpy array
    descriptors = des_list[0][1]

    #if valueError, delete the last printed image
    print image_path

    test_features = np.zeros((1, numWords), "float32")
    words, distance = vq(descriptors, voc)
    for w in words:
        test_features[0][w] += 1

    # Perform Tf-Idf vectorization and L2 normalization
    test_features = test_features*idf
    test_features = preprocessing.normalize(test_features, norm='l2')

    # Dot product of two arrays
    score = np.dot(test_features, im_features.T) # .T = self.transpose
    # indices from highest score to lowest score with argsort(-score)
    rank_ID = np.argsort(-score)

    ctr = 0;

    totalScore = 0;
    
    # Visualize the results
    figure(figsize = (13, 8))
    gray()
    subplot(5,4,1)
    imshow(im[:,:,::-1])
    axis('off')
    for i, ID in enumerate(rank_ID[0][0:16]):
	    # set similarity threshold
        totalScore += score[0][ID]
        if score[0][ID] > 0.2:
            #print image_path
            #if roi detects its original store, draw this roi rectangle on database image
            #if image_paths[ID] in ['database-store/1.jpg', 'database-store/1.1.jpg', 'database-store/1.2.jpg', 'database-store/1n.1.jpg', 'database-store/1n.jpg']:

            #if more than 5 images that similarities > 0.3, draw this roi rectangle on database image
            if score[0][ID] > 0.26:
                ctr+=1
            if ctr == 1:
                x = int(doc[image_path][0])
                y = int(doc[image_path][1])
                w = int(doc[image_path][2])
                h = int(doc[image_path][3])
                #cv2.rectangle(databaseIm, (x, y), (x+w, y+h), (0, 255, 0), 1, cv2.LINE_AA)
 
            #show candidates and similarities
            img = Image.open(image_paths[ID])
            #print image_paths[ID]
            gray()
            subplot(5,4,i+5)
            imshow(img)
            text(1, 1, score[0][ID])
            axis('off')
    #savefig(image_path+"-testBoW.jpg")
    #show()

    '''if totalScore > maxScore:
        maxScore = totalScore
        selected_image_path = image_path'''

    if ctr > largestCtr:
    	largestCtr = ctr
    	selected_image_path = image_path
        print "selected!!! "+selected_image_path
        print "largestCtr: "
        print largestCtr


print "selected!!! "+selected_image_path

x = int(doc[selected_image_path][0])
y = int(doc[selected_image_path][1])
w = int(doc[selected_image_path][2])
h = int(doc[selected_image_path][3])
cv2.rectangle(databaseIm, (x, y), (x+w, y+h), (0, 255, 0), 1, cv2.LINE_AA)
    
cv2.imwrite(doc["databaseImage"]+"-quality-fast.jpg", databaseIm)
