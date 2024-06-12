using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class StoryView_UI : MonoBehaviour
{
    [SerializeField] Button _AutoBtn;
    [SerializeField] TextMeshProUGUI _AutoText;

    [SerializeField] Button _SkipBtn;
    [SerializeField] Button _LogBtn;

    [SerializeField] ScriptBox _ScriptBox;
    [SerializeField] LogView _LogView;

    [SerializeField] Image _Cover;


    public Action AutoCallback;
    public Action SkipCallback;


    public void Initialize()
    {
        StoryManager.Instance.IsAuto.Subscribe(DrawAutoBtn);


        _AutoBtn.onClick.AddListener(OnClickAutoBtn);
        _SkipBtn.onClick.AddListener(OnClickSkipBtn);
        _LogBtn.onClick.AddListener(OnClickLogBtn);

        _LogView.Init();
        _LogView.Hide();

        _Cover.gameObject.SetActive(false);
    }

    private void OnClickAutoBtn()
    {
        AutoCallback?.Invoke();
    }

    private void DrawAutoBtn(bool value)
    {
        if (value)
        {
            _AutoText.text = "Auto в║";
        }
        else
        {
            _AutoText.text = "Auto бс";
        }
    }

    private void OnClickSkipBtn()
    {
        SkipCallback?.Invoke();
    }

    private void OnClickLogBtn()
    {
        _LogView.Show();
    }


    public async UniTask Start_Story_Data_Script(Story_Data_Script data)
    {
        _LogView.AddScript(data);

        await _ScriptBox.StartScript(data);
    }

    public void Start_Story_Data_ScriptBox_Visible(Story_Data_ScriptBox_Visible data)
    {
        _ScriptBox.gameObject.SetActive(data.Visible);
    }
}
