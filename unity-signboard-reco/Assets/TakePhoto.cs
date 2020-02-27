using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using OpenCVForUnity;
using OpenCvSharp;
using OpenCvSharp.XFeatures2D;

public class TakePhoto : MonoBehaviour
{
    Camera mainCam;
    Thread thread;
    int num1;
    int num2;

    //private static FeatureDetector siftDetect;
    //private static DescriptorExtractor siftExtract;
    private static SIFT sift;
    private static SURF surf;
    private static OpenCvSharp.ORB orb;
    private static OpenCvSharp.XFeatures2D.BriefDescriptorExtractor brief;
    private static OpenCvSharp.DescriptorMatcher descriptorMatcher;

    //private Texture2D inputTex;
    private Texture2D tex;
    private Texture2D textTex;

    //private OpenCvSharp.Mat inputImg;
    private OpenCvSharp.Mat mat;

    private OutputArray des1;
    private OutputArray des2;
    private OutputArray des3;
    private OutputArray desCam;
    //private OutputArray desFirstCatch;

    private OpenCvSharp.KeyPoint[] kp1;
    private OpenCvSharp.KeyPoint[] kp2;
    private OpenCvSharp.KeyPoint[] kp3;
    private OpenCvSharp.KeyPoint[] kpCam;
    //public OpenCvSharp.MatOfByte maskMoB;

    private List<OpenCvSharp.Point2f> srcPts;
    private List<OpenCvSharp.Point2f> dstPts;

    public GameObject textObj1;
    private UnityEngine.UI.Text text1;
    private RectTransform textRT1;
    public GameObject textObj2;
    private UnityEngine.UI.Text text2;
    private RectTransform textRT2;
    public GameObject textObj3;
    private UnityEngine.UI.Text text3;
    private RectTransform textRT3;

    public GameObject rawImageObj;
    private RectTransform rawImageRT;//拍攝和顯示的影像大小
    private RawImage rawImageRI;//顯示圖片

    private WebCamTexture myCam;//接收攝影機讀取到的圖片數據
    private Texture2D texture;

    private Vector3 rectTopLeft;
    private Vector3 rectTopRight;
    private Vector3 rectBotLeft;
    private Vector3 rectBotRight;

    private int width;
    private int height;

    private OpenCVForUnity.Mat transformMat;

    // Image rotation
    private Vector3 rotationVector = new Vector3(0f, 0f, 0f);
    // Image uvRect
    private UnityEngine.Rect defaultRect = new UnityEngine.Rect(0f, 0f, 1f, 1f);
    private UnityEngine.Rect fixedRect = new UnityEngine.Rect(0f, 1f, 1f, -1f);

    void Awake ()
    {
        mainCam = Camera.main;

        textObj1 = GameObject.Find("/ScreenCanvas/Text1");
        text1 = textObj1.GetComponent<UnityEngine.UI.Text>();
        textRT1 = textObj1.GetComponent<RectTransform>();
        textObj2 = GameObject.Find("/ScreenCanvas/Text2");
        text2 = textObj2.GetComponent<UnityEngine.UI.Text>();
        textRT2 = textObj2.GetComponent<RectTransform>();
        /*textObj3 = GameObject.Find("/ScreenCanvas/Text3");
        text3 = textObj3.GetComponent<UnityEngine.UI.Text>();
        textRT3 = textObj3.GetComponent<RectTransform>();*/

        rawImageObj = GameObject.Find("/ScreenCanvas/RawImage");
        rawImageRT = rawImageObj.GetComponent<RectTransform>();
        rawImageRI = rawImageObj.GetComponent<RawImage>();

        text1.enabled = false;
        text2.enabled = false;
        //text3.enabled = false;

        num1 = 0;
        num2 = 0;

        /*Vector3[] v = new Vector3[4];
        rawImageRT.GetWorldCorners(v);
        //rectBotLeft = v[0];
        //rectTopLeft = v[1];
        //rectTopRight = v[2];
        //rectBotRight = v[3];

        rectTopLeft = mainCam.WorldToScreenPoint(v[1]);
        rectTopRight = mainCam.WorldToScreenPoint(v[2]);
        rectBotLeft = mainCam.WorldToScreenPoint(v[0]);
        rectBotRight = mainCam.WorldToScreenPoint(v[3]);

        rectTopLeft = mainCam.WorldToScreenPoint(new Vector3(rawImageRT.rect.xMin, rawImageRT.rect.yMax, 0));
        rectTopRight = mainCam.WorldToScreenPoint(new Vector3(rawImageRT.rect.xMax, rawImageRT.rect.yMax, 0));
        rectBotLeft = mainCam.WorldToScreenPoint(new Vector3(rawImageRT.rect.xMin, rawImageRT.rect.yMin, 0));
        rectBotRight = mainCam.WorldToScreenPoint(new Vector3(rawImageRT.rect.xMax, rawImageRT.rect.yMin, 0));

        width = (int)(rectTopRight.x-rectBotLeft.x);
        height = (int)(rectTopRight.y-rectBotLeft.y);*/

        StartCoroutine(OpenCamera());
    }

    void Start ()
    {
        //thread = new Thread(new ThreadStart(ThreadMainFunc));
        //thread = new Thread(ThreadMainFunc);
        //thread.IsBackground = true;
        //thread = Loom.RunAsync(ThreadMainFunc);
        //thread.Start();
        //textTex = Resources.Load<Texture2D>("test-IMG_0204-text");
        //rawImageRI.texture = textTex;

        //TextAsset binary = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/img1.bytes", typeof(TextAsset));
        
        //inputTex = Resources.Load<Texture2D>("test-IMG_0204");
        //rawImageRI.texture = inputTex;
        //Debug.Log("inputTex.width: "+inputTex.width);
        //Debug.Log("inputTex.height: "+inputTex.height);

        //tex.LoadImage(binary.bytes);
        //Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/img.PNG", typeof(Texture2D));

        //StartCoroutine(GetTextImg());

        //Size texSize = new Size(tex.width, tex.height);
        //Mat mat = new Mat(texSize, CvType.CV_8UC4);
        //Utils.texture2DToMat(tex, mat);
        
        //inputImg = Cv2.ImRead(imgPath);
        //Cv2.ImShow("inputImg", inputImg);
        
        //tex = Unity.MatToTexture(inputImg);
        //rawImageRI.texture = tex;

        /*inputTex = new Texture2D(2,2);
        string imgPath = "../signboard-rectangle/test-IMG_0204.PNG";
        byte [] binaryImageData = File.ReadAllBytes(imgPath);
        inputTex.LoadImage(binaryImageData);*/

        //inputTex = Resources.Load<Texture2D>("forAddText");

        //必要 防止記憶體爆炸
        Texture2D inputTex1 = TextureGray.ToGray("1");
        Texture2D inputTex2 = TextureGray.ToGray("2");
        //Texture2D inputTex3 = TextureGray.ToGray("3");
        //Debug.Log("inputTex.width: "+inputTex.width);
        //Debug.Log("inputTex.height: "+inputTex.height);

        //rawImageRI.texture = inputTex;

        OpenCvSharp.Mat inputImg1 = Unity.TextureToMat(inputTex1);
        OpenCvSharp.Mat inputImg2 = Unity.TextureToMat(inputTex2);
        //OpenCvSharp.Mat inputImg3 = Unity.TextureToMat(inputTex3);
        //OpenCvSharp.Mat inputImg2 = Unity.TextureToMat(inputTex);
        //Cv2.ImShow("img", inputImg);

        InputArray img1 = InputArray.Create(inputImg1);
        InputArray img2 = InputArray.Create(inputImg2);
        //InputArray img3 = InputArray.Create(inputImg3);
        //Debug.Log("inputImg: "+inputImg.ToString());
        //InputArray mask = null;
        //OpenCvSharp.KeyPoint[] kp1 = null;
        des1 = OutputArray.Create(inputImg1);
        des2 = OutputArray.Create(inputImg2);
        //des3 = OutputArray.Create(inputImg3);
        //Debug.Log("des1: "+des1);

        //Initiate SIFT detector and extractor
        //siftDetect = FeatureDetector.create(3);
        //siftExtract = DescriptorExtractor.create(1);

        sift = SIFT.Create();
        //surf = SURF.Create((double)100);
        //orb = OpenCvSharp.ORB.Create();
        //brief = OpenCvSharp.XFeatures2D.BriefDescriptorExtractor.Create();

        //if image too large will cause app Terminated due to memory error
        kp1 = sift.Detect(inputImg1);
        kp2 = sift.Detect(inputImg2);
        //kp3 = sift.Detect(inputImg3);
        //kp1 = surf.Detect(inputImg);
        //kp1 = orb.Detect(inputImg);
        //kp1 = brief.Detect(inputImg);

        //Cv2.ImShow("img", inputImg); ok

        sift.Compute(img1, ref kp1, des1);
        sift.Compute(img2, ref kp2, des2);
        //sift.Compute(img3, ref kp3, des3);
        //surf.Compute(img1, ref kp1, des1);
        //orb.Compute(img1, ref kp1, des1);
        //brief.Compute(img1, ref kp1, des1);

        //Cv2.ImShow("img", inputImg); 亂碼圖
        //Cv2.ImShow("img", inputImg2); ok

        //foreach (OpenCvSharp.KeyPoint kp in kp1)
           // Debug.Log("kp: "+kp.ToString());

        //用flannbased的話unity會掛掉
        descriptorMatcher = OpenCvSharp.DescriptorMatcher.Create("BruteForce");

        //sift.DetectAndCompute(img1, mask, out kp1, des1);

        //MatOfKeyPoint kp1 = new MatOfKeyPoint();
        //Mat des1 = new Mat();
        //siftDetect.detect(inputImg, kp1);
        //siftExtract.compute(inputImg, kp1, des1);

        //StartCoroutine(OpenCamera());//開啟攝影機鏡頭
        //StartCoroutine(CalculateHomography());

        /*Texture2D sourceTex = ScreenCapture.CaptureScreenshotAsTexture();
        Color[] pix = sourceTex.GetPixels((int)rectBotLeft.x, (int)rectBotLeft.y, width, height);     
        tex = new Texture2D(width, height);
        tex.SetPixels(pix);
        tex.Apply();

        tex = TextureGray.ToGray(tex);

        mat = Unity.TextureToMat(tex);

        InputArray img2 = InputArray.Create(mat);
        desCam = OutputArray.Create(mat);

        kpCam = sift.Detect(mat);*/
    }
    
    void Update ()
    {
        //若有開啟鏡頭則將拍到的畫面顯示出來
        if (myCam.isPlaying)
        {
            // Rotate image to show correct orientation 
            rotationVector.z = -myCam.videoRotationAngle;
            rawImageRI.rectTransform.localEulerAngles = rotationVector;
 
            // Unflip if vertically flipped
            rawImageRI.uvRect = myCam.videoVerticallyMirrored ? fixedRect : defaultRect; 

            rawImageRI.texture = myCam;

            if (Time.frameCount % 10 == 0){
                StartCoroutine(CalculateHomography());
            }
        }

        if (Time.frameCount % 50 == 0)
        {
            System.GC.Collect();
        }

        if (Time.frameCount % 70 == 0){
            text1.enabled = false;
            text2.enabled = false;
            //text3.enabled = false;
        }

        //Thread.Sleep(1);
    }
    
    //take photo
    public void OnClick1()
    {
        myCam.Pause();
    }

    //resume camera
    public void OnClick2()
    {
        myCam.Play();
    }

    //select region
    public void OnClick3()
    {
        StartCoroutine(GetPicture());
    }

    //show UI text
    public void OnClick6()
    {
        //StartCoroutine(ShowText());
    }

    /*IEnumerator GetTextImg()
    {
        //read sign image
        //inputTex = new Texture2D(2,2);
        //string imgPath = "../signboard-rectangle/test-IMG_0204.PNG";
        //byte [] binaryImageData = File.ReadAllBytes(imgPath);
        //inputTex.LoadImage(binaryImageData);
        Texture2D forText = Resources.Load<Texture2D>("forAddText");
        rawImageRI.texture = forText;

        yield return new WaitForEndOfFrame();

        int x = Mathf.FloorToInt(rectBotLeft.x);
        int y = Mathf.FloorToInt(rectBotLeft.y);
        int w = Mathf.FloorToInt(width);
        int h = Mathf.FloorToInt(height);

        //get sign image with text
        Texture2D sourceTex = ScreenCapture.CaptureScreenshotAsTexture();
        //rawImageRI.texture = sourceTex;
        //Color[] pix = sourceTex.GetPixels((int)rectBotLeft.x, (int)rectBotLeft.y, width, height); 
        Color[] pix = sourceTex.GetPixels(x,y,w,h);     
        textTex = new Texture2D(w, h);
        textTex.SetPixels(pix);
        textTex.Apply();

        Debug.Log("x: "+x);
        Debug.Log("y: "+y);
        Debug.Log("w: "+w);
        Debug.Log("h: "+h);

        text.enabled = false;
        //rawImageRI.texture = textTex;
    }*/

    void ThreadMainFunc()
    {
        //這裏我們使用無窮迴圈讓執行緒永久存在
        while(true)
        {
            Debug.Log("thread running2");
            /*if (myCam.isPlaying)
            {
                // Rotate image to show correct orientation 
                rotationVector.z = -myCam.videoRotationAngle;
                rawImageRI.rectTransform.localEulerAngles = rotationVector;
 
                // Unflip if vertically flipped
                rawImageRI.uvRect = myCam.videoVerticallyMirrored ? fixedRect : defaultRect; */

            //rawImageRI.texture = myCam;

            //if (Time.frameCount % 10 == 0){
            //await Task.Delay(1000);
            CalculateHomography();
            //}

            Debug.Log("thread running3");

            //這種永久存在的執行緒, 要適當讓他進入沉睡, 否則Abort關不掉這個執行緒
            Thread.Sleep(300);
        }
    }

    IEnumerator CalculateHomography()
    //void CalculateHomography()
    {
        //Debug.Log("CalculateHomography1");
        //myCam.Pause();
        yield return new WaitForEndOfFrame();
        //yield return new WaitForSeconds((float)0.5); 
        //程式開始後至少要等0.3秒才會出現影像畫面，不然算sift一開始就會記憶體爆掉

        //input camera image
        /*Texture2D sourceTex = ScreenCapture.CaptureScreenshotAsTexture();
        Color[] pix = sourceTex.GetPixels((int)rectBotLeft.x, (int)rectBotLeft.y, width, height);     
        Texture2D tex = new Texture2D(width, height);
        tex.SetPixels(pix);
        tex.Apply();*/

        //Debug.Log("CalculateHomography2");

        //rawimage position at (0,0)，start from bottom left
        int xStart = (int)(Screen.width - rawImageRT.rect.width)/2;
        int yStart = (int)(Screen.height - rawImageRT.rect.height)/2;

        /*Debug.Log("xStart: "+xStart);
        Debug.Log("yStart: "+yStart);
        Debug.Log("Screen.width: "+Screen.width);
        Debug.Log("Screen.height: "+Screen.height);
        Debug.Log("rawImageRT.rect.width: "+rawImageRT.rect.width);
        Debug.Log("rawImageRT.rect.height: "+rawImageRT.rect.height);*/

        //get sign image with text
        Texture2D sourceTex = ScreenCapture.CaptureScreenshotAsTexture();
        //rawImageRI.texture = sourceTex;
        //Color[] pix = sourceTex.GetPixels((int)rectBotLeft.x, (int)rectBotLeft.y, width, height); 
        Color[] pix = sourceTex.GetPixels(xStart, yStart, (int)rawImageRT.rect.width, (int)rawImageRT.rect.height);     
        tex = new Texture2D((int)rawImageRT.rect.width, (int)rawImageRT.rect.height);
        tex.SetPixels(pix);
        tex.Apply();

        //Debug.Log("tex.width: "+tex.width);
        //Debug.Log("tex.height: "+tex.height);

        //input fixed image
        /*Texture2D tex = new Texture2D(2,2);
        string imgPath = "../signboard-rectangle/test-199-fast-628.jpg"; 
        byte [] binaryImageData = File.ReadAllBytes(imgPath);
        tex.LoadImage(binaryImageData);*/

        //scale texture to make it smaller
        TextureScale.Bilinear(tex, tex.width/2, tex.height/2);

        //必要 防止記憶體爆炸
        tex = TextureGray.ToGray(tex);

        //rawImageRI.texture = tex;

        mat = Unity.TextureToMat(tex);

        Destroy(sourceTex);
        Destroy(tex);

        //Cv2.ImShow("img", mat); ok
        //OpenCvSharp.Mat mat = Cv2.ImRead(imgPath, ImreadModes.Unchanged);

        //Debug.Log("mat: "+mat.ToString());
        //string imgPath = "../signboard-rectangle/test-199-fast-628.jpg";
        //OpenCvSharp.Mat mat = Cv2.ImRead(imgPath);
        InputArray imgCam = InputArray.Create(mat);
        desCam = OutputArray.Create(mat);

        //Cv2.ImShow("img", mat); ok
        //OpenCvSharp.Mat mat2 = mat;

        //sift = SIFT.Create();

        //System.Diagnostics.Stopwatch time = new System.Diagnostics.Stopwatch();
        //time.Start ();

        //卡卡
        OpenCvSharp.KeyPoint[] kpCam = sift.Detect(mat);
        //OpenCvSharp.KeyPoint[] kpCam = surf.Detect(mat);
        //OpenCvSharp.KeyPoint[] kpCam = orb.Detect(mat);
        //OpenCvSharp.KeyPoint[] kpCam = brief.Detect(mat);

        //time.Stop();
        //Debug.Log("執行 " + time.Elapsed.TotalSeconds + " 秒");

        //myCam.Pause();
        //rawImageRI.texture = tex;

        //Cv2.ImShow("img", mat); ok
        //Cv2.ImShow("img", mat2); ok

        sift.Compute(imgCam, ref kpCam, desCam);
        //surf.Compute(img2, ref kpCam, desCam);
        //orb.Compute(img2, ref kpCam, desCam);
        //brief.Compute(img2, ref kpCam, desCam);

        //Cv2.ImShow("img", mat); 
        //Cv2.ImShow("img", mat2); 爆炸

        OpenCvSharp.Mat desCammat = desCam.GetMat();
        //Debug.Log("desCammat: "+desCammat);

        //if (!M) 如果還沒計算出homography M {
        //desFirstCatch = desCam;
        //OutputArray descriptors_object = des1;

        OpenCvSharp.Mat des1mat = des1.GetMat();
        OpenCvSharp.Mat des2mat = des2.GetMat();
        //OpenCvSharp.Mat des3mat = des3.GetMat();
        //Debug.Log("des1mat: "+des1mat);

        OpenCvSharp.DMatch[] dmatch1 = descriptorMatcher.Match(des1mat, desCammat); 
        OpenCvSharp.DMatch[] dmatch2 = descriptorMatcher.Match(des2mat, desCammat); 
        //OpenCvSharp.DMatch[] dmatch3 = descriptorMatcher.Match(des3mat, desCammat); 

        //Debug.Log("damtch1[0]: "+dmatch1[0].ToString());
        //}
        //else {
            //OpenCvSharp.Mat desFirstCatchmat = desFirstCatch.GetMat();

           // OpenCvSharp.DMatch[] dmatch = descriptorMatcher.Match(desFirstCatchmat, desCammat);
           // OutputArray descriptors_object = desFirstCatch;
        //}

        double max_dist1 = 0; 
        double min_dist1 = 100;
        double max_dist2 = 0; 
        double min_dist2 = 100;
        //double max_dist3 = 0; 
        //double min_dist3 = 100;

        //Cv2.ImShow("img", mat); 爆炸

        //Quick calculation of max and min distances between keypoints
        foreach (OpenCvSharp.DMatch d in dmatch1){   
            double dist = d.Distance;
            if( dist < min_dist1 ) min_dist1 = dist;
            if( dist > max_dist1 ) max_dist1 = dist;
        }

        foreach (OpenCvSharp.DMatch d in dmatch2){   
            double dist = d.Distance;
            if( dist < min_dist2 ) min_dist2 = dist;
            if( dist > max_dist2 ) max_dist2 = dist;
        }

        /*foreach (OpenCvSharp.DMatch d in dmatch3){   
            double dist = d.Distance;
            if( dist < min_dist3 ) min_dist3 = dist;
            if( dist > max_dist3 ) max_dist3 = dist;
        }*/

        //Draw only "good" matches (i.e. whose distance is less than 3*min_dist )
        List<OpenCvSharp.DMatch> goodMatch1 = new List<OpenCvSharp.DMatch>();
        foreach (OpenCvSharp.DMatch d in dmatch1){   
            if( d.Distance < 3*min_dist1 )
                goodMatch1.Add(d);    
        }

        List<OpenCvSharp.DMatch> goodMatch2 = new List<OpenCvSharp.DMatch>();
        foreach (OpenCvSharp.DMatch d in dmatch2){   
            if( d.Distance < 3*min_dist2 )
                goodMatch2.Add(d);    
        }

        /*List<OpenCvSharp.DMatch> goodMatch3 = new List<OpenCvSharp.DMatch>();
        foreach (OpenCvSharp.DMatch d in dmatch3){   
            if( d.Distance < 3*min_dist3 )
                goodMatch3.Add(d);    
        }*/

        List<OpenCvSharp.Point2f> srcPts1 = new List<OpenCvSharp.Point2f>();
        List<OpenCvSharp.Point2f> dstPts1 = new List<OpenCvSharp.Point2f>();
        foreach (OpenCvSharp.DMatch d in goodMatch1){
            //-- Get the keypoints from the good matches
            srcPts1.Add(kp1[d.QueryIdx].Pt);
            dstPts1.Add(kpCam[d.TrainIdx].Pt);
            //Debug.Log("kp1[d.QueryIdx].Pt: "+kp1[d.QueryIdx].Pt);
        }

        List<OpenCvSharp.Point2f> srcPts2 = new List<OpenCvSharp.Point2f>();
        List<OpenCvSharp.Point2f> dstPts2 = new List<OpenCvSharp.Point2f>();
        foreach (OpenCvSharp.DMatch d in goodMatch2){
            //-- Get the keypoints from the good matches
            srcPts2.Add(kp2[d.QueryIdx].Pt);
            dstPts2.Add(kpCam[d.TrainIdx].Pt);
            //Debug.Log("kp1[d.QueryIdx].Pt: "+kp1[d.QueryIdx].Pt);
        }

        /*List<OpenCvSharp.Point2f> srcPts3 = new List<OpenCvSharp.Point2f>();
        List<OpenCvSharp.Point2f> dstPts3 = new List<OpenCvSharp.Point2f>();
        foreach (OpenCvSharp.DMatch d in goodMatch3){
            //-- Get the keypoints from the good matches
            srcPts3.Add(kp3[d.QueryIdx].Pt);
            dstPts3.Add(kpCam[d.TrainIdx].Pt);
            //Debug.Log("kp1[d.QueryIdx].Pt: "+kp1[d.QueryIdx].Pt);
        }*/

        //jump to next iteration if less than certain number of keypoints matched
        if (srcPts1.Count < 200 && srcPts2.Count < 200)
            yield break;

        if (srcPts1.Count >= srcPts2.Count){
            srcPts = new List<OpenCvSharp.Point2f>(srcPts1);
            dstPts = new List<OpenCvSharp.Point2f>(dstPts1);
            text1.enabled = true;
            text2.enabled = false;
            num1++;
            //text3.enabled = false;
        }
        /*else if(srcPts2.Count >= srcPts1.Count && srcPts2.Count >= srcPts3.Count){
            srcPts = new List<OpenCvSharp.Point2f>(srcPts2);
            dstPts = new List<OpenCvSharp.Point2f>(dstPts2);
            text2.enabled = true;
            text1.enabled = false;
            text3.enabled = false;
        }*/
        else{
            srcPts = new List<OpenCvSharp.Point2f>(srcPts2);
            dstPts = new List<OpenCvSharp.Point2f>(dstPts2);
            text2.enabled = true;
            text1.enabled = false;
            num2++;
            //text2.enabled = false;
        }

        if (num1 > num2+10){
            text1.enabled = true;
            text2.enabled = false;
        }

        if (num2 > num1+10){
            text2.enabled = true;
            text1.enabled = false;
        }

        if (num1>60 || num2>60){
            num1 = 0;
            num2 = 0;
        }
        //OpenCvSharp.Mat mat2 = mat;

        //Cv2.DrawKeypoints(mat, kpCam, mat2); 

        //Cv2.ImShow("img", mat); 亂碼圖

        //Texture2D tex2 = new Texture2D(8, 8);
        //tex2 = Unity.MatToTexture(mat);
        //rawImageRI.texture = tex2;
        //myCam.Pause();

        //Cv2.ImShow("img", mat2); 亂碼圖

        Texture2D emptyTex = new Texture2D(8, 8);
        OpenCvSharp.Mat outputImg = Unity.TextureToMat(emptyTex);
        //Debug.Log("outputImg: "+outputImg.ToString());

        InputArray srcArr =  InputArray.Create<OpenCvSharp.Point2f>(srcPts);
        InputArray dstArr =  InputArray.Create<OpenCvSharp.Point2f>(dstPts);
        OutputArray mask = OutputArray.Create(outputImg);

        OpenCvSharp.Mat M = Cv2.FindHomography(srcArr, dstArr, HomographyMethods.Ransac, 5, mask);

        OpenCVForUnity.Mat transMat = new OpenCVForUnity.Mat (3, 3, CvType.CV_32FC1);
        transMat.put(0,0,   M.Get<double>(0,0), M.Get<double>(0,1), M.Get<double>(0,2),
                            M.Get<double>(1,0), M.Get<double>(1,1), M.Get<double>(1,2),
                            M.Get<double>(2,0), M.Get<double>(2,1), M.Get<double>(2,2));
        //Debug.Log("transMat: "+transMat.dump());
        
        //Debug.Log("mask: "+mask);
        //OpenCvSharp.Mat maskMat = mask.GetMat();
        //Debug.Log("maskMat: "+maskMat.ToString());
        //maskMoB = new OpenCvSharp.MatOfByte(maskMat);

        //-- Get the corners from the image_1 ( the object to be "detected" )
        /*OpenCvSharp.Point2f[] obj_corners = new OpenCvSharp.Point2f[4];
        obj_corners[0] = new OpenCvSharp.Point2f(0, 0); 
        obj_corners[1] = new OpenCvSharp.Point2f(inputTex.width, 0);
        obj_corners[2] = new OpenCvSharp.Point2f(inputTex.width, inputTex.height); 
        obj_corners[3] = new OpenCvSharp.Point2f(0, inputTex.height);

        //OpenCvSharp.Point2f[] scene_corners = new OpenCvSharp.Point2f[4];
        //scene_corners = Cv2.PerspectiveTransform(obj_corners, M);

        //if (!M) 如果還沒計算出homography M {
        //Cv2.DrawMatches(inputImg, kp1, mat, kpCam, goodMatch, outputImg, OpenCvSharp.Scalar.All(-1), 
        //OpenCvSharp.Scalar.All(-1), maskMoB.ToArray(), DrawMatchesFlags.NotDrawSinglePoints);
        //else {

        //Texture2D outputTex = Unity.MatToTexture(outputImg);
        //rawImageRI.texture = outputTex;

        //-- Draw lines between the corners (the mapped object in the scene - image_2 )
        //Cv2.Line(outputImg, scene_corners[0] + obj_corners[1], scene_corners[1] + obj_corners[1], OpenCvSharp.Scalar.LightBlue, 4);
        //Cv2.Line(outputImg, scene_corners[1] + obj_corners[1], scene_corners[2] + obj_corners[1], OpenCvSharp.Scalar.LightBlue, 4);
        //Cv2.Line(outputImg, scene_corners[2] + obj_corners[1], scene_corners[3] + obj_corners[1], OpenCvSharp.Scalar.LightBlue, 4);
        //Cv2.Line(outputImg, scene_corners[3] + obj_corners[1], scene_corners[0] + obj_corners[1], OpenCvSharp.Scalar.LightBlue, 4);

        //OpenCvSharp.Mat outimg = Unity.TextureToMat(emptyTex);
        //inputImg = Unity.TextureToMat(emptyTex);
        //Cv2.DrawKeypoints(mat, kpCam, outimg, OpenCvSharp.Scalar.LightBlue);

        //show image with text after homography
        /*string imgPath2 = "../signboard-rectangle/test-IMG_0204-text.PNG";
        textTex = new Texture2D(2,2);
        byte [] binaryImageData2 = File.ReadAllBytes(imgPath2);
        textTex.LoadImage(binaryImageData2);
        rawImageRI.texture = textTex;*/

        /*OpenCVForUnity.Mat inputTextImg = new OpenCVForUnity.Mat(new OpenCVForUnity.Size(textTex.width, textTex.height), CvType.CV_8UC4);
        Utils.texture2DToMat(textTex, inputTextImg);
        OpenCVForUnity.Mat outputTextImg = new OpenCVForUnity.Mat(new OpenCVForUnity.Size(textTex.width, textTex.height), CvType.CV_8UC4);

        Imgproc.warpPerspective(inputTextImg, outputTextImg, transMat, new OpenCVForUnity.Size(textTex.width, textTex.height));

        Texture2D outputTex = new Texture2D((int)textTex.width, (int)textTex.height, TextureFormat.RGB24, false);
        Utils.matToTexture2D(outputTextImg, outputTex);*/

        //TextureScale.Bilinear(outputTex, outputTex.width/5, outputTex.height/5);
        //rawImageRI.texture = outputTex;

        //text.enabled = true;

        /*Vector3 scale;
        scale.x = new Vector4((float)M.Get<double>(0,0), (float)M.Get<double>(1,0), (float)M.Get<double>(2,0), 0).magnitude;
        scale.y = new Vector4((float)M.Get<double>(0,1), (float)M.Get<double>(1,1), (float)M.Get<double>(2,1), 0).magnitude;
        scale.z = new Vector4((float)M.Get<double>(0,2), (float)M.Get<double>(1,2), (float)M.Get<double>(2,2), 0).magnitude;
        
        Vector3 forward;
        forward.x = (float)M.Get<double>(0,2);
        forward.y = (float)M.Get<double>(1,2);
        forward.z = (float)M.Get<double>(2,2);
 
        Vector3 upwards;
        upwards.x = (float)M.Get<double>(0,1);
        upwards.y = (float)M.Get<double>(1,1);
        upwards.z = (float)M.Get<double>(2,1);

        //textRT.localScale = scale;
        //textRT.rotation = Quaternion.LookRotation(forward, upwards);*/

        Matrix4x4 matrix = new Matrix4x4();

        /*matrix.SetRow(0, new Vector4((float)M.Get<double>(0,0), (float)M.Get<double>(0,1), (float)M.Get<double>(0,2),0));
        matrix.SetRow(1, new Vector4((float)M.Get<double>(1,0), (float)M.Get<double>(1,1), (float)M.Get<double>(1,2),0));
        matrix.SetRow(2, new Vector4(0,0,1,0));
        matrix.SetRow(3, new Vector4(0,0,0,1));*/

        //inverse效果還行
        matrix.SetRow(0, new Vector4((float)M.Get<double>(0,0), (float)M.Get<double>(0,1), 0, (float)M.Get<double>(0,2)));
        matrix.SetRow(1, new Vector4((float)M.Get<double>(1,0), (float)M.Get<double>(1,1), 0, (float)M.Get<double>(1,2)));
        matrix.SetRow(2, new Vector4(0,0,1,0));
        matrix.SetRow(3, new Vector4(0,0,0,1));

        Matrix4x4 inverse = matrix.inverse;

        //textRT.localScale = matrix.lossyScale;
        //textRT.rotation = matrix.rotation; //rotation跟eulerangles效果一樣
        textRT1.rotation = inverse.rotation;
        textRT2.rotation = inverse.rotation;
        //textRT3.rotation = inverse.rotation;

        Destroy(emptyTex);

        //calculate euler angle
        /*double angleX = Math.Asin(-M.Get<double>(2,1));
        double angleY = Math.Atan2(M.Get<double>(2,0), M.Get<double>(2,2));
        double angleZ = Math.Atan2(M.Get<double>(0,1), M.Get<double>(1,1));
        //textRT.eulerAngles = new Vector3((float)angleX, (float)angleY, (float)angleZ);
        //Debug.Log("textRT.eulerAngles: "+textRT.eulerAngles.ToString());

        //calculate quaternion
        double w = Math.Sqrt(1 + M.Get<double>(0,0) + M.Get<double>(1,1) + M.Get<double>(2,2))/2;
        double w4 = w*4;
        double qx = (M.Get<double>(2,1) - M.Get<double>(1,2))/w4 ;
        double qy = (M.Get<double>(0,2) - M.Get<double>(2,0))/w4 ;
        double qz = (M.Get<double>(1,0) - M.Get<double>(0,1))/w4 ;
        //textRT.rotation = new Quaternion((float)qx, (float)qy, (float)qz, 1);

        double tr = M.Get<double>(0,0) + M.Get<double>(1,1) + M.Get<double>(2,2);
        Debug.Log("tr: "+tr);*/

        //Cv2.ImShow("img", mat); 
        //myCam.Pause();
    }

    IEnumerator OpenCamera()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);//授權開啟鏡頭

        if (Application.HasUserAuthorization(UserAuthorization.WebCam))//若同意開啟攝影機
        {
            // (攝影機名稱, resolution width, resolution height, FPS)
            myCam = new WebCamTexture(WebCamTexture.devices[0].name, 1280, 720, 30);
            myCam.Play();//開啟攝影機
        }
    }

    IEnumerator GetPicture()
    {
        yield return new WaitForEndOfFrame(); //攝影機讀取到的Frame繪製完畢後才進行拍照

        //calculate mat of selected region and whole rawimage
        OpenCVForUnity.Mat srcMat = new OpenCVForUnity.Mat (4, 1, CvType.CV_32FC2);
        OpenCVForUnity.Mat dstMat = new OpenCVForUnity.Mat (4, 1, CvType.CV_32FC2);

        dstMat.put(0,0,rectTopLeft.x-rectBotLeft.x, rectTopLeft.y-rectBotLeft.y,
                       rectTopRight.x-rectBotLeft.x, rectTopRight.y-rectBotLeft.y,
                       rectBotLeft.x-rectBotLeft.x, rectBotLeft.y-rectBotLeft.y,
                       rectBotRight.x-rectBotLeft.x, rectBotRight.y-rectBotLeft.y);
        
        //calculate transform matrix
        transformMat = new OpenCVForUnity.Mat(3,3,CvType.CV_32FC1);
        transformMat = Imgproc.getPerspectiveTransform(srcMat, dstMat);

        Texture2D sourceTex = ScreenCapture.CaptureScreenshotAsTexture();

        Color[] pix = sourceTex.GetPixels((int)rectBotLeft.x, (int)rectBotLeft.y, width, height);
        Texture2D destTex = new Texture2D(width, height);
        destTex.SetPixels(pix);
        destTex.Apply();

        OpenCVForUnity.Size textureSize = new OpenCVForUnity.Size(width, height);

        OpenCVForUnity.Mat rawImageSrcMat = new OpenCVForUnity.Mat(textureSize, CvType.CV_8UC4);
        OpenCVForUnity.Mat rawImageSrcMatFlip = new OpenCVForUnity.Mat(textureSize, CvType.CV_8UC4);
        Utils.texture2DToMat(destTex, rawImageSrcMat);

        Core.flip(rawImageSrcMat, rawImageSrcMatFlip, 0);

        OpenCVForUnity.Mat rawImageDstMat = new OpenCVForUnity.Mat(textureSize, CvType.CV_8UC4);
        //Mat rawImageDstMatFlip = new Mat(textureSize, CvType.CV_8UC4);

        Imgproc.warpPerspective(rawImageSrcMatFlip, rawImageDstMat, transformMat, textureSize);

        texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        Utils.matToTexture2D(rawImageDstMat, texture);
        rawImageRI.texture = texture;
    }

    /*IEnumerator ShowText()
    {
        text.enabled = true;
        yield return new WaitForEndOfFrame();

        // Remember currently active render texture
        //RenderTexture currentActiveRT = RenderTexture.active;

        // Set the supplied RenderTexture as the active one
        //RenderTexture.active = renderCam.targetTexture;
        //renderCam.Render();

        //Debug.Log("rt.width: "+ rt.width);
        //Debug.Log("rt.height: "+ rt.height);

        // Create a new Texture2D and read the RenderTexture image into it
        //Texture2D tex = new Texture2D(renderCam.pixelWidth, renderCam.pixelHeight);
        //tex.ReadPixels(renderCam.pixelRect, 0, 0);

        //Color[] pix = tex.GetPixels();
        //texture.SetPixels(0, 0, renderCam.pixelWidth, renderCam.pixelHeight, pix);

        //texture.ReadPixels(renderCam.pixelRect, 0, 0);

        Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();

        Color[] pix = tex.GetPixels((int)rectBotLeft.x, (int)rectBotLeft.y, width, height);
        texture.SetPixels(pix);
        //texture.Apply();

        OpenCVForUnity.Size textureSize = new OpenCVForUnity.Size(width, height);

        OpenCVForUnity.Mat rawImageSrcMat = new OpenCVForUnity.Mat(textureSize, CvType.CV_8UC4);
        OpenCVForUnity.Mat rawImageSrcMatFlip = new OpenCVForUnity.Mat(textureSize, CvType.CV_8UC4);

        Utils.texture2DToMat(texture, rawImageSrcMat);

        Core.flip(rawImageSrcMat, rawImageSrcMatFlip, 0);

        OpenCVForUnity.Mat rawImageDstMat = new OpenCVForUnity.Mat(textureSize, CvType.CV_8UC4);

        Imgproc.warpPerspective(rawImageSrcMatFlip, rawImageDstMat, transformMat, textureSize, Imgproc.WARP_INVERSE_MAP);

        Texture2D newTex = new Texture2D(width, height, TextureFormat.RGB24, false);

        Utils.matToTexture2D(rawImageDstMat, newTex);

        // Restorie previously active render texture
        //RenderTexture.active = currentActiveRT;

        rawImageRI.texture = newTex;

        text.enabled = false;
    }*/

    void OnDisable()
    {
        myCam.Stop();//離開當前Scene後關閉攝影機
        //thread.Abort();
    }
}
