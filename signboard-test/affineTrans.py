import cv2
import sys
import numpy as np

img = cv2.imread(sys.argv[1])

rows,cols,ch = img.shape

pts1 = np.float32([[50,50],[200,50],[50,200]])
pts2 = np.float32([[10,100],[200,50],[100,250]])

M = cv2.getAffineTransform(pts1,pts2)

dst = cv2.warpAffine(img,M,(cols,rows))
cv2.imshow('image', dst)
cv2.waitKey(0)
cv2.destroyAllWindows()
cv2.imwrite("affine-"+sys.argv[1], dst)