#!/usr/bin/env python
'''
Wrong Usage!!!
Usage:
    python selective-search.py input_image (f|q)
    f=fast, q=quality
Use "l" to display less rects, 'm' to display more rects, "q" to quit.
'''
import sys
import cv2
import yaml
import numpy as np
 
if __name__ == '__main__':
    # If image path and f/q is not passed as command
    # line arguments, quit and display help message
    #if len(sys.argv) < 3:
        #print(__doc__)
        #sys.exit(1)
 
    # speed-up using multithreads
    cv2.setUseOptimized(True);
    cv2.setNumThreads(4);
 
    # read image
    im = cv2.imread(sys.argv[1])

    # resize image
    #newHeight = 700
    #newWidth = int(im.shape[1]*700/im.shape[0])
    #im = cv2.resize(im, (newWidth, newHeight)) 
    im = cv2.resize(im, (0,0), fx=0.3, fy=0.3) 

    # create Selective Search Segmentation Object using default parameters
    ss = cv2.ximgproc.segmentation.createSelectiveSearchSegmentation()
 
    # set input image on which we will run segmentation
    ss.setBaseImage(im)
 
    # Switch to fast but low recall Selective Search method
    #if (sys.argv[2] == 'f'):
    #quality first(do not change priority)
    ss.switchToSelectiveSearchQuality()
    
    #output = "fast"
 
    # Switch to high recall but slow Selective Search method
    #elif (sys.argv[2] == 'q'):
    #ss.switchToSelectiveSearchQuality()
    #output = "quality"
    # if argument is neither f nor q print help message
    #else:
        #print(__doc__)
        #sys.exit(1)
 
    # run selective search segmentation on input image
    Qrects = ss.process()
    print('Total Number of Region Proposals: {}'.format(len(Qrects)))

    #fast second(do not change priority)
    ss.switchToSelectiveSearchFast()
    rects = ss.process()
    print('Total Number of Region Proposals: {}'.format(len(rects)))

    # number of region proposals to show
    numShowRects = 1000

    # increment to increase/decrease total number
    # of reason proposals to be shown
    #increment = 50
    
    #while True:
        # create a copy of original image
    imOutQuality = im.copy()
    imOutFast = im.copy()
 
    # itereate over all the region proposals
    for i, rect in enumerate(rects):
        # draw rectangle for region proposal till numShowRects
        if (i < numShowRects):
            x, y, w, h = rect
            if w*h > 20000:
                cv2.rectangle(imOutFast, (x, y), (x+w, y+h), (0, 255, 0), 1, cv2.LINE_AA)
        else:
            break

    '''for i, rect in enumerate(Qrects):
        # draw rectangle for region proposal till numShowRects
        if (i < numShowRects):
            x, y, w, h = rect
            if w*h > 20000:
                cv2.rectangle(imOutQuality, (x, y), (x+w, y+h), (0, 255, 0), 1, cv2.LINE_AA)
        else:
            break
 
    # show output
    #cv2.imshow("img", imOut)

    # record key press
    #k = cv2.waitKey(0) & 0xFF
    
    # m is pressed
    #if k == 109:
        # increase total number of rectangles to show by increment
        #numShowRects += increment
    # l is pressed
    #elif k == 108 and numShowRects > increment:
        # decrease total number of rectangles to show by increment
        #numShowRects -= increment
    # q is pressed
    #if k == 113:
    '''
    '''roiInfo = {}
    # output rectangle ROI image
    for i, rect in enumerate(rects):
        if (i < numShowRects):
            x, y, w, h = rect
            if w*h > 20000:
                roi = im[y:y+h, x:x+w]
                #roi = cv2.resize(roi, (0,0), fx=10, fy=10) 
                roiInfo["newTest/numrect=1000/store1-augmented/night-fast/night.jpg-roi-"+str(i)+"-fast"+".jpg"] = [str(x),str(y),str(w),str(h)]
                #cv2.imwrite(sys.argv[1]+"-roi-"+str(i)+"-fast"+".jpg", roi)
        else:
            break

    for i, rect in enumerate(Qrects):
        if (i < numShowRects):
            x, y, w, h = rect
            if w*h > 20000:
                roi = im[y:y+h, x:x+w]
                #roi = cv2.resize(roi, (0,0), fx=10, fy=10) 
                roiInfo["newTest/numrect=1000/store1-augmented/night-quality/night.jpg-roi-"+str(i)+"-quality"+".jpg"] = [str(x),str(y),str(w),str(h)]
                #cv2.imwrite(sys.argv[1]+"-roi-"+str(i)+"-quality"+".jpg", roi)
        else:
            break
    
    roiInfo["databaseImage"] = sys.argv[1]
    '''
    #output yaml of roi info
    #with open(sys.argv[1]+"-roi"+".yaml", 'w') as f:
        #yaml.dump(roiInfo, f, default_flow_style = False)

    # close image show window
    #cv2.destroyAllWindows()
    #cv2.imwrite('selective-search-quality-'+sys.argv[1], imOutQuality)
    cv2.imwrite('selective-search-fast-'+sys.argv[1], imOutFast)