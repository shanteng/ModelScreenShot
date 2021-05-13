#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System.Collections.Generic;

public class ModelCapture : MonoBehaviour
{
    public Camera _camera;
    public Transform _modelRoot;
    public static ModelCapture mInstance;
    void Awake()
    {
        mInstance = this;
    }

    public int offset = 500;
    public int pngWidth = 800;
    public void CaptureCamera(string SaveName,Image img)
    {
        Camera camera = _camera;

        Vector3 ScreenPos = camera.WorldToScreenPoint(this._modelRoot.localPosition);
        //var x = ScreenPos.x - Screen.width / 2;
        //       var y = ScreenPos.y - Screen.height / 2;

        int w = 1920; //Screen.width;
        int h = 1080;// Screen.height;

        RenderTexture renderTexture = RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.ARGB32);
        camera.targetTexture = renderTexture;

   
        camera.RenderDontRestore();
        RenderTexture.active = renderTexture;
        Texture2D shot = new Texture2D(pngWidth, h, TextureFormat.ARGB32, false);
        shot.ReadPixels(new Rect(offset, 0, pngWidth, h), 0, 0);
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