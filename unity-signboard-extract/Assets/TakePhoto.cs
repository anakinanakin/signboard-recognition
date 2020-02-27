using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using OpenCVForUnity;

public class TakePhoto : MonoBehaviour
{
	Camera mainCam;

    public GameObject textObj;
    private UnityEngine.UI.Text text;

	public GameObject circle1;        
    public GameObject circle2;
    public GameObject circle3;
    public GameObject circle4; 

    public GameObject rawImageObj;
    private RectTransform rawImageRT;//拍攝和顯示的影像大小
    private RawImage rawImageRI;//顯示圖片

    private WebCamTexture myCam;//接收攝影機讀取到的圖片數據
    private Texture2D texture;
    private JsonObject jsonObj;

    private Vector3 rectTopLeft;
    private Vector3 rectTopRight;
    private Vector3 rectBotLeft;
    private Vector3 rectBotRight;

    private int width;
    private int height;

    private Mat transformMat;

    // Image rotation
    private Vector3 rotationVector = new Vector3(0f, 0f, 0f);
    // Image uvRect
    private UnityEngine.Rect defaultRect = new UnityEngine.Rect(0f, 0f, 1f, 1f);
    private UnityEngine.Rect fixedRect = new UnityEngine.Rect(0f, 1f, 1f, -1f);

	[Serializable]
	public class Gps
	{
    	public string latitude;
   		public string longitude;

   		public Gps(string lati, string longi){
    		latitude = lati;
    		longitude = longi;
    	}
	}

	[Serializable]
	public class TimePeriod
	{
    	//public string _id;
    	public string begin_hr;
    	public string begin_min;
    	public string end_hr;
    	public string end_min;

    	public TimePeriod(string bhr, string bmin, string ehr, string emin){
    		begin_hr = bhr;
    		begin_min = bmin;
    		end_hr = ehr;
    		end_min = emin;
    	}
	}

	[Serializable]
	public class Item
	{
    	public List<TimePeriod> timePeriod;
    	//public string _id;
    	public bool open;

    	public Item(bool op, List<TimePeriod> t){
    		open = op;
    		timePeriod = t;
    	}
	}

	[Serializable]
	public class OpenTime
	{
    	public List<Item> Items;

    	public OpenTime(List<Item> i){
    		Items = i;
    	}
	}		

	[Serializable]
	public class Item2
	{
    	//public string _id;
    	public string title;
    	public string content;

    	public Item2(string t, string cont){
    		title = t;
    		content = cont;
    	}
	}

	[Serializable]
	public class InfoList
	{
    	public List<Item2> Items;

    	public InfoList(List<Item2> i){
    		Items = i;
    	}
	}

	//data上傳的時候要直接包byte[],下載的時候要分data.data
	[Serializable]
	public class Data
	{
   	 	//public string type = "Buffer";
    	public byte[] data;

    	public Data(byte[] d){
    		data = d;
    	}
	}

	//for download
	[Serializable]
	public class Item3
	{
		//public string _id;
   	 	public Data data;
    	public string contentType;

    	public Item3(Data d, string ct){
    		data = d;
    		contentType = ct;
    	}
	}

	//for upload
	[Serializable]
	public class Item4
	{
		//public string _id;
   	 	public byte[] data;
    	public string contentType;

    	public Item4(byte[] d, string ct){
    		data = d;
    		contentType = ct;
    	}
	}

	//for download
	[Serializable]
	public class Picture
	{
		public List<Item3> Items;

		public Picture(List<Item3> i){
    		Items = i;
    	}
	}

	//for upload
	[Serializable]
	public class Picture1
	{
		public List<Item4> Items;

		public Picture1(List<Item4> i){
    		Items = i;
    	}
	}

	[Serializable]
	public class JsonObject
	{
   		public Gps gps;
    	public OpenTime openTime;
    	public InfoList infoList;
    	public Picture picture;
    	//public string _id;
    	public string shopName;
    	public string shopAddress;
    	public string telephone;
	}

	JsonObject JsonConvert(string json) {
    	JsonObject jsonObj = JsonUtility.FromJson<JsonObject>(json);
    	return jsonObj;
    }

    void Start ()
    {
    	mainCam = Camera.main;

    	circle1 = GameObject.Find("Circle1");
    	circle2 = GameObject.Find("Circle2");
    	circle3 = GameObject.Find("Circle3");
    	circle4 = GameObject.Find("Circle4");

        textObj = GameObject.Find("/WorldCanvas/Text");
        text = textObj.GetComponent<UnityEngine.UI.Text>();
        text.enabled = false;

    	rawImageObj = GameObject.Find("/WorldCanvas/RawImage");
        rawImageRT = rawImageObj.GetComponent<RectTransform>();
        rawImageRI = rawImageObj.GetComponent<RawImage>();

        StartCoroutine(OpenCamera());//開啟攝影機鏡頭
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

        	//rawImageRI.texture = myCam.videoVerticallyMirrored ? myCam : (Texture)Texture2D.whiteTexture;
        	rawImageRI.texture = myCam;
        }
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

    //save picture and upload
    public void OnClick4()
    {
    	byte[] bytes = texture.EncodeToPNG();

    	//save to local directory
    	//File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);

    	string shopID = "02";
    	string shopName = "麥當勞";
    	string shopAddress = "公園北路3號";
    	string telephone = "091234";
    	Gps gps = new Gps("12.3", "2.4");

    	List<TimePeriod> timePeriod = new List<TimePeriod>();
    	timePeriod.Add(new TimePeriod("10","30","15","00"));
    	List<Item> item = new List<Item>();
    	item.Add(new Item(true, timePeriod));
    	OpenTime openTime = new OpenTime(item);

    	List<Item2> item2 = new List<Item2>();
    	item2.Add(new Item2("哈哈", "笑你"));
    	InfoList infoList = new InfoList(item2);

    	List<Item4> item4 = new List<Item4>();
    	item4.Add(new Item4(bytes, "png"));
    	Picture1 picture = new Picture1(item4);

    	WWWForm formData = new WWWForm();
        formData.AddField("shopID", shopID);
        formData.AddField("shopName", shopName);
        formData.AddField("shopAddress", shopAddress);
        formData.AddField("telephone", telephone);
        formData.AddField("gps", JsonUtility.ToJson(gps));
        formData.AddField("openTime", JsonUtility.ToJson(openTime));
        formData.AddField("infoList", JsonUtility.ToJson(infoList));
        formData.AddField("picture", JsonUtility.ToJson(picture));

    	StartCoroutine(Upload(formData));
    }

    //download picture and display
    public void OnClick5()
    {
    	myCam.Pause();

    	string shopID = "01";
    	StartCoroutine(DownLoad(shopID));
    }

    //show UI text
    public void OnClick6()
    {
        StartCoroutine(ShowText());
    }

    //change scene to input text
    public void OnClick7()
    {
        StartCoroutine(ShowText());
    }

    IEnumerator Upload(WWWForm formData) 
    {  
        UnityWebRequest www = UnityWebRequest.Post("http://localhost:8888/set_shopInfo", formData);
        yield return www.Send();
 
        if(www.error != null) {
        	Debug.Log("error that occurs: "+www.error);
        	Debug.Log("HTTP respond: "+www.responseCode);
        }
        else {
            Debug.Log("Form upload complete!");
        }
    }

    IEnumerator DownLoad(string shopID) 
    {
        UnityWebRequest www = UnityWebRequest.Get("http://localhost:8888/get_shopInfo?shopID=" + shopID);
        yield return www.Send();
 
        if(www.error != null) {
            Debug.Log("error that occurs: "+www.error);
            Debug.Log("HTTP respond: "+www.responseCode);
        }
        else {
            // Show results as text
            Debug.Log("Form download complete!");
            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;
            string resultStr = System.Text.Encoding.UTF8.GetString(results);
            jsonObj = JsonConvert(resultStr);
        }
        byte[] img = jsonObj.picture.Items[0].data.data;
        Texture2D tex = new Texture2D(128, 128);
        tex.LoadImage(img);
        rawImageRI.texture = tex;
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
        Mat srcMat = new Mat (4, 1, CvType.CV_32FC2);
        Mat dstMat = new Mat (4, 1, CvType.CV_32FC2);

        Vector3 circle1screenPos = mainCam.WorldToScreenPoint(circle1.transform.position);
        Vector3 circle2screenPos = mainCam.WorldToScreenPoint(circle2.transform.position);
        Vector3 circle3screenPos = mainCam.WorldToScreenPoint(circle3.transform.position);
        Vector3 circle4screenPos = mainCam.WorldToScreenPoint(circle4.transform.position);

        rectTopLeft = mainCam.WorldToScreenPoint(new Vector3(rawImageRT.rect.xMin, rawImageRT.rect.yMax, 0));
        rectTopRight = mainCam.WorldToScreenPoint(new Vector3(rawImageRT.rect.xMax, rawImageRT.rect.yMax, 0));
        rectBotLeft = mainCam.WorldToScreenPoint(new Vector3(rawImageRT.rect.xMin, rawImageRT.rect.yMin, 0));
        rectBotRight = mainCam.WorldToScreenPoint(new Vector3(rawImageRT.rect.xMax, rawImageRT.rect.yMin, 0));

        //getPerspectiveTransform要以目標rect的左下角為(0,0)
        srcMat.put(0,0,circle1screenPos.x-rectBotLeft.x, circle1screenPos.y-rectBotLeft.y, 
        		       circle2screenPos.x-rectBotLeft.x, circle2screenPos.y-rectBotLeft.y,
        		       circle3screenPos.x-rectBotLeft.x, circle3screenPos.y-rectBotLeft.y,
        		       circle4screenPos.x-rectBotLeft.x, circle4screenPos.y-rectBotLeft.y);

        dstMat.put(0,0,rectTopLeft.x-rectBotLeft.x, rectTopLeft.y-rectBotLeft.y,
        			   rectTopRight.x-rectBotLeft.x, rectTopRight.y-rectBotLeft.y,
        			   rectBotLeft.x-rectBotLeft.x, rectBotLeft.y-rectBotLeft.y,
        			   rectBotRight.x-rectBotLeft.x, rectBotRight.y-rectBotLeft.y);
        
        //calculate transform matrix
        transformMat = new Mat(3,3,CvType.CV_32FC1);
        transformMat = Imgproc.getPerspectiveTransform(srcMat, dstMat);

        width = (int)(rectTopRight.x-rectBotLeft.x);
        height = (int)(rectTopRight.y-rectBotLeft.y);

        Texture2D sourceTex = ScreenCapture.CaptureScreenshotAsTexture();

        Color[] pix = sourceTex.GetPixels((int)rectBotLeft.x, (int)rectBotLeft.y, width, height);
        Texture2D destTex = new Texture2D(width, height);
        destTex.SetPixels(pix);
        destTex.Apply();

        Size textureSize = new Size(width, height);

        Mat rawImageSrcMat = new Mat(textureSize, CvType.CV_8UC4);
        Mat rawImageSrcMatFlip = new Mat(textureSize, CvType.CV_8UC4);
        Utils.texture2DToMat(destTex, rawImageSrcMat);

        Core.flip(rawImageSrcMat, rawImageSrcMatFlip, 0);

        Mat rawImageDstMat = new Mat(textureSize, CvType.CV_8UC4);
        //Mat rawImageDstMatFlip = new Mat(textureSize, CvType.CV_8UC4);

        Imgproc.warpPerspective(rawImageSrcMatFlip, rawImageDstMat, transformMat, textureSize);

    	texture = new Texture2D(width, height, TextureFormat.RGB24, false);
    	Utils.matToTexture2D(rawImageDstMat, texture);
    	rawImageRI.texture = texture;
    }

    IEnumerator ShowText()
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

        Size textureSize = new Size(width, height);

        Mat rawImageSrcMat = new Mat(textureSize, CvType.CV_8UC4);
        Mat rawImageSrcMatFlip = new Mat(textureSize, CvType.CV_8UC4);

        Utils.texture2DToMat(texture, rawImageSrcMat);

        Core.flip(rawImageSrcMat, rawImageSrcMatFlip, 0);

        Mat rawImageDstMat = new Mat(textureSize, CvType.CV_8UC4);

        Imgproc.warpPerspective(rawImageSrcMatFlip, rawImageDstMat, transformMat, textureSize, Imgproc.WARP_INVERSE_MAP);

        Texture2D newTex = new Texture2D(width, height, TextureFormat.RGB24, false);

        Utils.matToTexture2D(rawImageDstMat, newTex);

        // Restorie previously active render texture
        //RenderTexture.active = currentActiveRT;

        rawImageRI.texture = newTex;

        text.enabled = false;
    }

    void OnDisable()
    {
        myCam.Stop();//離開當前Scene後關閉攝影機
    }
}