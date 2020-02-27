import cv2
import sys
import numpy as np

#rectangle in Python is a tuple of (x,y,w,h)
#for rectangle
def union(a, b):
    x = min(a[0], b[0])
    y = min(a[1], b[1])
    w = max(a[0]+a[2], b[0]+b[2]) - x
    h = max(a[1]+a[3], b[1]+b[3]) - y
    return (x, y, w, h)

#for rectangle
def intersection(a, b):
    x = max(a[0], b[0])
    y = max(a[1], b[1])
    w = min(a[0]+a[2], b[0]+b[2]) - x
    h = min(a[1]+a[3], b[1]+b[3]) - y
    if w<0 or h<0: return () # or (0,0,0,0) ?
    return (x, y, w, h)

#combine all rectangles with overlappings to outermost
def combine_boxes(boxes):
    noIntersectLoop = False
    noIntersectMain = False
    posIndex = 0
    # keep looping until we have completed a full pass over each rectangle
    # and checked it does not overlap with any other rectangle
    while noIntersectMain == False:
        noIntersectMain = True
        posIndex = 0
        # start with the first rectangle in the list, once the first 
        # rectangle has been unioned with every other rectangle,
        # repeat for the second until done
        while posIndex < len(boxes):
            noIntersectLoop = False
            while noIntersectLoop == False and len(boxes) > 1 and posIndex < len(boxes):
                a = boxes[posIndex]
                listBoxes = np.delete(boxes, posIndex, 0)
                index = 0
                for b in listBoxes:
                    #if there is an intersection, the boxes overlap
                    if intersection(a, b): 
                        newBox = union(a,b)
                        listBoxes[index] = newBox
                        boxes = listBoxes
                        noIntersectLoop = False
                        noIntersectMain = False
                        index = index + 1
                        break
                    noIntersectLoop = True
                    index = index + 1
            posIndex = posIndex + 1

    return boxes.astype("int")

#detect text in a image using mser
mser = cv2.MSER_create(_delta = 10, _min_area=1000)
img = cv2.imread(sys.argv[1])
gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
vis = img.copy()
regions, _ = mser.detectRegions(gray)
rectList = []
for region in regions:
    #fit a bounding box to the contour
    (x, y, w, h) = cv2.boundingRect(region.reshape(-1,1,2))
    #increase rect width and height, union overlapped rect
    rectList.append((x, y, w+40, h+40))

newRectList = combine_boxes(rectList)
for rect in newRectList:
	x, y, w, h = rect[0], rect[1], rect[2], rect[3]
	if w*h > 50000:
    		cv2.rectangle(vis, (x, y), (x + w, y + h), (0, 255, 0), 5)

#hulls = [cv2.convexHull(p.reshape(-1, 1, 2)) for p in regions]
#cv2.polylines(vis, hulls, 1, (0, 255, 0))

vis = cv2.resize(vis, (720, 480))  
cv2.imshow('image', vis)
cv2.imwrite('output2'+sys.argv[1], vis)
cv2.waitKey(0)
cv2.destroyAllWindows()

'''#draw the filled in contours to an empty img in white
mask = np.zeros((img.shape[0], img.shape[1], 1), dtype=np.uint8)
for contour in hulls:
    cv2.drawContours(mask, [contour], -1, (255, 255, 255), -1)
#extract only the text
text_only = cv2.bitwise_and(img, img, mask=mask)
pic2 = cv2.resize(text_only, (720, 480))  
cv2.imshow('image2', pic2)
cv2.waitKey(0)
cv2.destroyAllWindows()'''