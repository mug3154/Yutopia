using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScriptBox : MonoBehaviour
{

    [SerializeField] Button _Screen;
    [SerializeField] TextMeshProUGUI _NameText;
    [SerializeField] TextMeshProUGUI _ScriptText;
    [SerializeField] GameObject _Pointer;


    Queue<string> _ScriptQueue = new Queue<string>();
    
    bool _IsTouch;


    // Start is called before the first frame update
    void Start()
    {
        _Screen.onClick.AddListener(OnClickScreen);
    }

    
    private void OnClickScreen()
    {
        _IsTouch = true;
    }

    public async UniTask StartScript(Story_Data_Script data)
    {
        _IsTouch = false;

        gameObject.SetActive(true);

        _Pointer.SetActive(false);

        _NameText.text = data.SpeecherName;
        _ScriptText.text = "";

        SetScriptToQueue(data.Script);

        _Screen.interactable = data.Skippable;

        await ScriptPrint(data.Script);

        _Screen.interactable = true;
        _Pointer.SetActive(true);

        if(StoryManager.Instance.IsAuto.Value == false)
        {
            while (_IsTouch == false)
            {
                await UniTask.Yield();
            }
        }
        else
        {
            await UniTask.Delay(100);
        }
    }

    private void SetScriptToQueue(string script)
    {
        _ScriptQueue.Clear();

        //얍얍 테스트 <@size@30><@speed@0.5><@shaking@1@0.5><@speed@0>등 <@shaking@1@0.5><@speed@0>장 <@size@30><@speed@0.1>와하하하하하하하하하하!

        //((<@speed@0.5> 글자 타이핑 속도)
        //((<@shaking@1@0.5 > 화면 흔들기 강도 1, 0.5초 지속)

        char curr;
        string completeText = "";

        bool isOption = false;

        int max = script.Length;
        for (int i = 0; i < max; ++i)
        {
            curr = script[i];

            if (curr == '<')
            {
                isOption = true;
                completeText = curr.ToString();
            }
            else if (curr == '>')
            {
                isOption = false;
                completeText += curr;

                _ScriptQueue.Enqueue(completeText);
            }
            else
            {
                if (isOption)
                {
                    completeText += curr;
                }
                else
                {
                    _ScriptQueue.Enqueue(curr.ToString());
                }
            }
        }

    }

    private async UniTask ScriptPrint(string script)
    {
        int speedMiliseconds = 100;
        string[] split;

        string completeText = "";

        while (_ScriptQueue.Count > 0)
        {
            completeText = _ScriptQueue.Dequeue();

            if (_IsTouch)
            {
                if (completeText.Length == 1) //option
                {
                    _ScriptText.text += completeText;
                }

                await UniTask.Delay(10);
            }
            else
            {
                if (completeText.Length > 1) //option
                {
                    split = completeText.Split('@');

                    if (split[1] == "speed")//0="<", 1="speed", 2="0.1", 3=">"
                    {
                        if (float.TryParse(split[2], out float speed))
                        {
                            speedMiliseconds = (int)(1000 * speed);
                        }
                    }
                }
                else
                {
                    _ScriptText.text += completeText;
                }

                await UniTask.Delay(speedMiliseconds);
            }
        }

        _IsTouch = false;
    }

}
