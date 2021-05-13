using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Events;

public class ModelClickItem : MonoBehaviour
{
    public GameObject _Select;
    public Button _clickButton;
    public Text _NameTxt;
    private UnityAction<string> _fun;
    private string _file;
    void Awake()
    {
        this._clickButton.onClick.AddListener(this.OnClick);
    }

    private void OnClick()
    {
        this._fun.Invoke(this._file);
    }

    public void SetSelected(string file)
    {
        this._Select.SetActive(file.Equals(this._file));
    }

    public void AddCallBack(UnityAction<string> fun)
    {
        this._fun = fun;   
    }

    public void SetData(string file)
    {
        this._file = file;
        int lastIndex = file.LastIndexOf('/');
        int lastPoint = file.LastIndexOf('.');
        string lastStr = file.Substring(lastIndex+1, file.Length - lastIndex-1);
        string finalStr = lastStr.Split('.')[0];
        this._NameTxt.text = finalStr;
    }

  
}