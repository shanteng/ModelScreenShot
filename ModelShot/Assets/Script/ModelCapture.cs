#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System.Collections.Generic;

public class ModelCapture : MonoBehaviour
{
    public Camera _camera;
    public static ModelCapture mInstance;
    void Awake()
    {
        mInstance = this;
    }

    public void CaptureCamera(string SaveName,Image img)
    {
        Camera camera = _camera;
        int w = 1920; //Screen.width;
        int h = 1080;// Screen.height;

        RenderTexture renderTexture = RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.ARGB32);
        camera.targetTexture = renderTexture;

   
        camera.RenderDontRestore();
        RenderTexture.active = renderTexture;
        Texture2D shot = new Texture2D(w, h, TextureFormat.ARGB32, false);
        shot.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        shot.Apply();

        Sprite _sprite = Sprite.Create(shot, new Rect(0, 0, shot.width, shot.height), new Vector2(0.5f, 0.5f));
        img.sprite = _sprite;
        img.gameObject.SetActive(true);

        string path = System.Environment.CurrentDirectory + "/ModelCapture/" + SaveName + ".png";
        byte[] bytes = shot.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);

        RenderTexture.active = null;
        camera.targetTexture = null;
    }
}
#endif