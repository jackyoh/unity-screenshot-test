using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine;
using UnityEditor;

public class TestCamera : MonoBehaviour {
    [SerializeField] private TilemapRenderer tilemapRenderer;
    [SerializeField] private RectTransform rectTransform;

    void Start() {
        Color originalBackground = Camera.main.backgroundColor;
        /*
        string persistentDataPath = Application.persistentDataPath;
        string temporaryCachePath = Application.temporaryCachePath;
        string filePath = "/home/user1/aaa/result2.png";
        */
        //string filePath = "/home/user1/aaa/result2.png";
        
        string filePath = Application.persistentDataPath + 
            "/" + FileUtil.GetUniqueTempPathInProject().Replace("Temp/", "") + ".png";
        RenderTexture rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
        Camera screenshotCamera = GetComponent<Camera>();
        screenshotCamera.CopyFrom(Camera.main);
        screenshotCamera.targetTexture = rt;
        screenshotCamera.backgroundColor = Color.white;
        tilemapRenderer.enabled = false;
        screenshotCamera.Render();
        screenshotCamera.backgroundColor = originalBackground;
        tilemapRenderer.enabled = true;

        /*Debug.Log("Width:" + Screen.width / 2 + ", Height:" + Screen.height / 2);
        Debug.Log("Left:" + rectTransform.offsetMin.x);
        Debug.Log("Right:" + rectTransform.offsetMax.x);
        Debug.Log("Top:" + rectTransform.offsetMax.y);
        Debug.Log("Bottom:" + rectTransform.offsetMin.y);*/

        /*var width = Screen.width - (Mathf.Abs(rectTransform.offsetMin.x) + Mathf.Abs(rectTransform.offsetMax.x));
        var height = Screen.height - (Mathf.Abs(rectTransform.offsetMin.y) + Mathf.Abs(rectTransform.offsetMax.y));
        var x = rectTransform.offsetMin.x;
        var y = rectTransform.offsetMin.y;*/
        
        int width = 450;
        int height = 300;
        int x = 0;
        int y = 0;

        Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);        
        RenderTexture.active = rt;
        texture.ReadPixels(new Rect((int)x, (int)y, width, height), 0, 0);
        texture.Apply();
        byte[] bytes = texture.EncodeToPNG();

        Texture2D sourceTexture = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);
        sourceTexture.LoadImage(bytes);

        Color[] pixels = sourceTexture.GetPixels();
        for (int i = 0 ; i < pixels.Length ; i++) {
            if (pixels[i] == Color.white) {
                pixels[i].a = 0f;
            }
        }
        sourceTexture.SetPixels(pixels);
        sourceTexture.Apply();
        bytes = sourceTexture.EncodeToPNG();
        System.IO.File.WriteAllBytes(filePath, bytes);

        RenderTexture.ReleaseTemporary(rt);
        RenderTexture.active = null;
        screenshotCamera.targetTexture = null;
    }
}
