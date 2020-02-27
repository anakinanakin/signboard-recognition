using UnityEngine;

public class TextureGray {
   public static Texture2D ToGray(Texture2D tex2D){ 
      Texture2D grayTex = new Texture2D(tex2D.width, tex2D.height);
      float grayScale;
      float alpha;
      for (int y  = 0; y < tex2D.height; ++y) {
         for (int x  = 0; x < tex2D.width; ++x) {
            grayScale = tex2D.GetPixel(x, y).r * 0.21f + tex2D.GetPixel(x, y).g * 0.71f + tex2D.GetPixel(x, y).b * 0.07f;
            alpha =  tex2D.GetPixel(x, y).a;
            grayTex.SetPixel (x, y, new Color(grayScale, grayScale, grayScale, alpha));
         }
      }
      grayTex.Apply();
      return grayTex;
   } 
   
   public static Texture2D ToGray(string resource_name){
      Texture2D text2D = (Texture2D)Resources.Load(resource_name);
      return(ToGray(text2D));
   }
}