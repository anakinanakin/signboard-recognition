#coding=utf-8
import cv2
import sys
import numpy as np

segmentator = cv2.ximgproc.segmentation.createGraphSegmentation(sigma=0.5, k=300, min_size=5000)
src = cv2.imread(sys.argv[1])
newHeight = 700
newWidth = int(src.shape[1]*700/src.shape[0])
src = cv2.resize(src, (newWidth, newHeight))
segment = segmentator.processImage(src)
seg_image = np.zeros(src.shape, np.uint8)

for i in range(np.max(segment)):
  y, x = np.where(segment == i)

  # 計算每分割區域的上下左右邊界
  bottom, top, left, right = min(y), max(y), min(x), max(x)

  # 繪製方框
  if (top-bottom)*(right-left)>10000:
      cv2.rectangle(src, (left, bottom), (right, top), (0, 255, 0), 1)

cv2.imshow("Result", src)
cv2.imwrite('graph-based'+sys.argv[1], src)
cv2.waitKey(0)
cv2.destroyAllWindows()