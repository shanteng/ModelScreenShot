
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using System.Collections;
using System;

public class ModelCaptureUI : MonoBehaviour
{
    public UIDragHandler _drager;
    public Transform _modelRoot;

    //UI
    public Button _btnFind;
    public Button _btnClear;
    public InputField _Input;


    public Button _btnCapture;
    public Button _btnFile;
    public Transform _Root;
    public ModelClickItem _templete;
    private List<ModelClickItem> _ItemList = new List<ModelClickItem>();
    private string _curFile;
    private GameObject _mode;

    public List<Toggle> mToggleList;
    public Button _btnResetState;
    public Slider _timeScleSlider;
    public Button _btnFrame;

    public Image _PreViewImg;
    void Awake()
    {
        this._PreViewImg.gameObject.SetActive(false);
        this._templete.gameObject.SetActive(false);
        this._drager._beginCall = this.OnBeginDrag;
        this._drager._dragCall = this.OnDrag;
        this._drager._endCall = this.OnEndDrag;
        this._btnCapture.onClick.AddListener(OnClickCapture);
        this._btnFile.onClick.AddListener(OnFileClick);

        this._btnFind.onClick.AddListener(OnFind);
        this._btnClear.onClick.AddListener(OnClear);

        this._btnResetState.onClick.AddListener(OnClickResetState);
        this._btnFrame.onClick.AddListener(OnClickFrame);

        _timeScleSlider.onValueChanged.AddListener(this.OnSlider);

        foreach (Toggle to in this.mToggleList)
        {
            to.GetComponentInChildren<Text>().text = to.name;
            to.isOn = false;
        }

        foreach (Toggle to in this.mToggleList)
        {
            to.onValueChanged.AddListener(this.OnToggle);
        }

        this.InitModelFileList();
    }

    void OnSlider(float value)
    {
        float val = this._timeScleSlider.value;
        Time.timeScale = val;
        float speed = Mathf.RoundToInt(val * 100);
        _timeScleSlider.GetComponentInChildren<Text>().text = "播放速度:"+ speed;
    }

    private string _stateName = "";
    void OnToggle(bool toggle)
    {
        if (this._mode == null)
            return;
        Animator animator = this._mode.GetComponent<Animator>();
        if (animator == null)
            return;
        this._timeScleSlider.value = 1f;
        _stateName = "";
        foreach (Toggle to in this.mToggleList)
        {
            animator.SetBool(to.name, to.isOn);
            if (to.isOn)
                _stateName = to.gameObject.name;
        }
    }

    private void OnFind()
    {
        if (this._Input.text.Equals(""))
            return;
        this._keyName = this._Input.text;
        ShowList();
    }


    private void OnClear()
    {
        this._Input.text = "";
        this._keyName = "";
        ShowList();
    }


    private void OnFileClick()
    {
        string path = System.Environment.CurrentDirectory + "/ModelCapture/" + "init" + ".txt";
        EditorUtility.RevealInFinder(path);
    }

    private Coroutine _cor;
    private void OnClickFrame()
    {
        this._timeScleSlider.value = 0;
        if (this._cor != null)
            StopCoroutine(this._cor);
        this._cor = StartCoroutine(PlayOneFrame());
    }

    IEnumerator PlayOneFrame()
    {
        Time.timeScale = 1f;
        yield return new WaitForSeconds(Time.deltaTime);
        Time.timeScale = 0f;
    }

    private void OnClickResetState()
    {
        foreach (Toggle to in this.mToggleList)
        {
            to.isOn = false;
        }
        _stateName = "";
        this._mode.transform.localEulerAngles = Vector3.zero;
        Time.timeScale = 1f;
    }

    private void OnClickCapture()
    {
        int lastIndex = this._curFile.LastIndexOf('/');
        string lastStr = _curFile.Substring(lastIndex + 1, _curFile.Length - lastIndex - 1);
        string finalStr = lastStr.Split('.')[0];
        if (this._stateName.Equals("") == false)
            finalStr = finalStr+"_"+ _stateName;

        string str = finalStr + Time.time;
        ModelCapture.mInstance.CaptureCamera(str, this._PreViewImg);
    }

    private List<string> _prefabs_names;
    private List<string> _show_names;
    private string _keyName = "";
    private void InitModelFileList()
    {
        string fullPath = "Assets/BundleAssets/Characters";
        _prefabs_names = new List<string>();
        this._show_names = new List<string>();

        string[] subFolders = Directory.GetDirectories(fullPath);
        string[] guids = null;
        int i = 0, iMax = 0;
        foreach (var folder in subFolders)
        {
            guids = AssetDatabase.FindAssets("t:Prefab", new string[] { folder });
            for (i = 0, iMax = guids.Length; i < iMax; ++i)
            {
                string nameStr = AssetDatabase.GUIDToAssetPath(guids[i]);
                int lastIndex = nameStr.LastIndexOf('/');
                string lastStr = nameStr.Substring(lastIndex + 1, nameStr.Length - lastIndex - 1);
                if (lastStr.StartsWith("H_"))
                    _prefabs_names.Add(nameStr);
            }
        }
        ShowList();
    }//end func

    private void ShowList()
    {
     //   this._btnClear.gameObject.SetActive(!this._keyName.Equals(""));
     //   this._btnFind.gameObject.SetActive(this._keyName.Equals(""));

        for (int i = 0; i < this._ItemList.Count; ++i)
        {
            GameObject.Destroy(this._ItemList[i].gameObject);    
        }

        this._show_names.Clear();
        this._ItemList.Clear();
        foreach (string file in _prefabs_names)
        {
            if (this._keyName.Equals("") || file.Contains(this._keyName))
            {
                _show_names.Add(file);
                ModelClickItem item = GameObject.Instantiate<ModelClickItem>(this._templete, this._Root);
                item.AddCallBack(this.ClickListCalllBack);
                item.gameObject.SetActive(true);
                item.SetData(file);
                this._ItemList.Add(item);
            }
        }

        if (_show_names.Count > 0)
            this.ClickListCalllBack(_show_names[0]);
    }

    public void ClickListCalllBack(string file)
    {
        foreach (ModelClickItem item in this._ItemList)
        {
            item.SetSelected(file);
        }
        this._curFile = file;

        if (this._mode != null)
        {
            GameObject.Destroy(this._mode);
            this._mode = null;
        }

        //加载新的模型
        string assetPath = this._curFile.Replace("\\", "/");
        GameObject go = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
        if (go)
        {
            this._mode = GameObject.Instantiate(go,this._modelRoot) as GameObject;
            this._mode.transform.localPosition = Vector3.zero;
            this._mode.transform.localScale = Vector3.one;
            this._mode.transform.localEulerAngles = Vector3.zero;
        }

        this.OnClickResetState();
        this._timeScleSlider.value = 1f;
    }

    public void OnBeginDrag(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        if (this._mode == null)
            return;
        Vector3 rotate = this._mode.transform.localEulerAngles;
        rotate.y -= eventData.delta.x;
        this._mode.transform.localEulerAngles = rotate;
    }

    public void OnEndDrag(PointerEventData eventData) { }
}
#endif