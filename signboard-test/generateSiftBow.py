#!/usr/local/bin/python2.7
#python generateSiftBow.py -t dataset/train/

import argparse as ap
import cv2
import numpy as np
import os
from sklearn.externals import joblib
from scipy.cluster.vq import *
from sklearn import preprocessing
import math

# check if parser argument exists
def is_valid_file(parser, arg):
    if not os.path.exists(arg):
        parser.error("The path %s does not exist!" % arg)
    else:
        return arg

# Get the path of the training set
parser = ap.ArgumentParser()
parser.add_argument("-t", "--trainingSet", help="Path to Training Set", required="True", type=lambda x: is_valid_file(parser, x))
args = vars(parser.parse_args())

# Get the training classes names and store them in a list
train_path = args["trainingSet"]
#train_path = "dataset/train/"

training_names = os.listdir(train_path)

# Get all the path to the images and save them in a list
# image_paths and the corresponding label in image_paths
image_paths = []
for training_name in training_names:
    # exclude non image file
    if os.path.splitext(training_name)[1] in ['.jpg', '.PNG', '.png']:
        image_path = os.path.join(train_path, training_name)
        # exclude small images(or it will crush)
        if os.path.getsize(image_path) > 85000:
            image_paths += [image_path]
        else:
            print image_path+" is too small"
    else:
        print training_name+" is not an image file"

# Create feature extraction and keypoint detector objects
sift = cv2.xfeatures2d.SIFT_create()

# List where all the descriptors are stored
des_list = []

for i, image_path in enumerate(image_paths):
    im = cv2.imread(image_path, 0)
    print "Extract SIFT of %s image, %d of %d images" %(image_paths[i], i+1, len(image_paths))
    kpts = sift.detect(im)
    kpts, des = sift.compute(im, kpts)
    des_list.append((image_path, des))

# Stack all the descriptors vertically in a numpy array
#downsampling = 1
#descriptors = des_list[0][1][::downsampling,:]
#for image_path, descriptor in des_list[1:]:
# descriptors = np.vstack((descriptors, descriptor[::downsampling,:]))

# Stack all the descriptors vertically in a numpy array
descriptors = des_list[0][1]
for image_path, descriptor in des_list[1:]:
    descriptors = np.vstack((descriptors, descriptor))

numWords = 1000
# Perform k-means clustering
print "Start k-means: %d words, %d key points" %(numWords, descriptors.shape[0])
voc, variance = kmeans(descriptors, numWords, 1)

# Calculate the histogram of features
im_features = np.zeros((len(image_paths), numWords), "float32")
for i in xrange(len(image_paths)):
    words, distance = vq(des_list[i][1],voc)
    for w in words:
        im_features[i][w] += 1

# Perform Tf-Idf vectorization
nbr_occurences = np.sum( (im_features > 0) * 1, axis = 0)
idf = np.array(np.log((1.0*len(image_paths)+1) / (1.0*nbr_occurences + 1)), 'float32')

# Perform L2 normalization
im_features = im_features*idf
im_features = preprocessing.normalize(im_features, norm='l2')

joblib.dump((im_features, image_paths, idf, numWords, voc, train_path), "store1-augment.pkl", compress=3)