using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.UI;

public class LogView : MonoBehaviour
{
    [SerializeField] RectTransform _Viewport;
    [SerializeField] RectTransform _Content;
    [SerializeField] Button _CloseBtn;

    ObjectPool<LogScriptCell> _CellPool;
    Queue<LogScriptCell> _Cells = new Queue<LogScriptCell>();

    Queue<ScriptData> _Datas = new Queue<ScriptData>();

    private void Start()
    {
        _CloseBtn.onClick.AddListener(OnClickCloseBtn);
    }

    private void OnClickCloseBtn()
    {
        Hide();
    }

    public void Init()
    {
        if(_CellPool == null)
        {
            var handler = Addressables.LoadAssetAsync<GameObject>("LogScriptCell").WaitForCompletion();
            _CellPool = new ObjectPool<LogScriptCell>(()=>Instantiate(handler.GetComponent<LogScriptCell>(), _Content));
            Addressables.Release(handler);
        }
    }

    public void AddScript(Story_Data_Script data)
    {
        if(_Datas.Count > 10)
        {
            _Datas.Dequeue();
        }

        StringBuilder strBuilder = new StringBuilder();

        bool isOption = false;
        char curr;

        int max = data.Script.Length;
        for (int i = 0; i < max; ++i)
        {
            curr = data.Script[i];

            if (curr == '<')
            {
                isOption = true;
            }
            else if (curr == '>')
            {
                isOption = false;
            }
            else
            {
                if (isOption == false)
                {
                    strBuilder.Append(curr.ToString());
                }
            }
        }

        ScriptData script = new ScriptData();
        script.Name = data.SpeecherName;
        script.Description = strBuilder.ToString();

        _Datas.Enqueue(script);
    }

    public void Show()
    {
        Time.timeScale = 0;

        gameObject.SetActive(true);

        _Content.sizeDelta = new Vector2(_Content.sizeDelta.x, _Datas.Count * 110);

        int cnt = 0;

        foreach(var data in _Datas)
        {
            var cell = _CellPool.Get();
            cell.SetText(data.Name, data.Description);
            cell.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -cnt * 110);
            cell.gameObject.SetActive(true);
            
            _Cells.Enqueue(cell);

            ++cnt;
        }

        _Content.anchoredPosition = new Vector2(0, _Content.sizeDelta.y - _Viewport.sizeDelta.y);
    }

    public void Hide()
    {
        Time.timeScale = 1;

        gameObject.SetActive(false);

        foreach (var cell in _Cells)
        {
            cell.gameObject.SetActive(false);
            _CellPool.Release(cell);
        }
    }



    struct ScriptData
    {
        public string Name;
        public string Description;
    }
}
