using Cysharp.Threading.Tasks;
using DG.Tweening;
using Newtonsoft.Json;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

public class StoryManager : Singleton<StoryManager>
{
    [SerializeField] Camera _Camera;
    [SerializeField] StoryView_UI _UI;
    [SerializeField] StoryView_View _View;

    Dictionary<string, SpriteAtlas> _AtlasDic;

    Story_Data _StoryData;
    List<Story_Data_Script> _PrevScripts;

    public ReactiveProperty<bool> IsAuto = new ReactiveProperty<bool>(false);


    /**
    Story_Data_Delay가 실행되지 않는 이상 모든 스크립트는 연속적으로 실행된다. 단, Story_Data_Script가 실행 될 때에는 Story_Data_Script가 끝나기 전에는 다음 데이터를 실행하지 않는다.
     */
    private void Start()
    {
        DOTween.Init(true, true, LogBehaviour.ErrorsOnly);

        _View.Initialize();
        _UI.Initialize();
        _UI.AutoCallback = Auto;
        _UI.SkipCallback = Skip;

        LoadStory();

        LoadAtlases();

        Show();
    }

    private void Show()
    {
        IsAuto.Value = false;

        StartStory().Forget();
    }

    private void Auto()
    {
        IsAuto.Value = !IsAuto.Value;
    }

    private void Skip()
    {

    }


    private void LoadStory()
    { 
        Story_Data data = new Story_Data();

        data.List.Enqueue(new Story_Data_ScriptBox_Visible()
        {
            Visible = false
        });

        data.List.Enqueue(new Story_Data_Object()
        {
            Name = "CharacterA",
            AtlasName = "CharacterA",
            ResourceName = "CharacterA_Normal",
            Color = new ushort[4] { 0, 0, 0, 255 },
            X = 0, Y= 0, ScaleX = 1, ScaleY = 1,
        });

        //data.List.Enqueue(new Story_Data_Delay()
        //{
        //    MilliSeconds = 1000
        //});

        data.List.Enqueue(new Story_Data_FadeInOut()
        {
            StartColor = new ushort[4] { 0, 0, 0, 255 },
            EndColor = new ushort[4] { 255, 255, 255, 255 },
            Duration = 10,
            Ease = "Linear",
            TargetName = "CharacterA"
        });

        data.List.Enqueue(new Story_Data_Camera_Shaking()
        {
            Duration = 1,
            Strength = 1
        });

        data.List.Enqueue(new Story_Data_Delay()
        {
            MilliSeconds = 1000
        });

        data.List.Enqueue(new Story_Data_Script()
        {
            Skippable = false,
            SpeecherName = "캐릭터A",
            Script = "스킵 안되는 텍스트지롱~~"
            //Script = "얍얍 테스트 <@size@30><@speed@0.5><@shaking@1@0.5><@speed@0>등 <@shaking@1@0.5><@speed@0>장 <@size@30><@speed@0.1>와하하하하하하하하하하!"
        });

        data.List.Enqueue(new Story_Data_Object()
        {
            Name = "CharacterA",
            AtlasName = "CharacterA",
            ResourceName = "CharacterA_Happy",
            Color = new ushort[4] { 255, 255, 255, 255 },
            X = 0,
            Y = 0,
            ScaleX = 1,
            ScaleY = 1,
        });


        data.List.Enqueue(new Story_Data_Script()
        {
            Skippable = true,
            SpeecherName = "캐릭터A",
            Script = "스킵 되는 텍스트지롱~~!!!!!!!!!"
            //Script = "얍얍 테스트 <@size@30><@speed@0.5><@shaking@1@0.5><@speed@0>등 <@shaking@1@0.5><@speed@0>장 <@size@30><@speed@0.1>와하하하하하하하하하하!"
        });

        string str = JsonConvert.SerializeObject(data, new Story_Data_Converter());

        _StoryData = JsonConvert.DeserializeObject<Story_Data>(str, new Story_Data_Converter());
    }

    private void LoadAtlases()
    {
        List<string> atlasNames = new List<string>();

        foreach(var data in _StoryData.List)
        {
            Story_Data_Object obj = data as Story_Data_Object;
            if (obj != null)
            {
                if(atlasNames.Contains(obj.AtlasName) == false) 
                    atlasNames.Add(obj.AtlasName);
            }
        }

        _AtlasDic = new Dictionary<string, SpriteAtlas>();

        foreach (var data in atlasNames)
        {
            var handler = Addressables.LoadAssetAsync<SpriteAtlas>(data).WaitForCompletion();
            _AtlasDic.Add(data, handler);
            Addressables.Release(handler);
        }
    }

    public Sprite GetSprite(string atlasName, string resourceName)
    {
        if(_AtlasDic.TryGetValue(atlasName, out var atlas))
        {
            return atlas.GetSprite(resourceName);
        }
        return null;
    }

    private async UniTaskVoid StartStory()
    {
        _PrevScripts = new List<Story_Data_Script>();

        int curr = 0;
        int max = _StoryData.List.Count;

        while (curr != max)
        {
            var data = _StoryData.List.Dequeue();

            if (data as Story_Data_Object != null)
            {
                Start_Story_Data_Object(data as Story_Data_Object);
            }
            else if (data as Story_Data_Delay != null)
            {
                await UniTask.Delay((data as Story_Data_Delay).MilliSeconds);
            }
            else if (data as Story_Data_Script != null)
            {
                await Start_Story_Data_Script(data as Story_Data_Script);
            }
            else if (data as Story_Data_ScriptBox_Visible != null)
            {
                Start_Story_Data_ScriptBox_Visible(data as Story_Data_ScriptBox_Visible);
            }
            else if (data as Story_Data_FadeInOut != null)
            {
                Start_Story_Data_FadeInOut(data as Story_Data_FadeInOut);
            }
            else if (data as Story_Data_Object_Visible != null)
            {
                Start_Story_Data_Object_Visible(data as Story_Data_Object_Visible);
            }
            else if (data as Story_Data_Camera_Shaking != null)
            {
                Start_Story_Data_Camera_Shaking(data as Story_Data_Camera_Shaking);
            }

            ++curr;
        }
    }

    private void Start_Story_Data_Object(Story_Data_Object data)
    {
        _View.Start_Story_Data_Object(data);
    }

    private async UniTask Start_Story_Data_Script(Story_Data_Script data)
    {
        _PrevScripts.Add(data);

        await _UI.Start_Story_Data_Script(data);
    }

    private void Start_Story_Data_ScriptBox_Visible(Story_Data_ScriptBox_Visible data)
    {
        _UI.Start_Story_Data_ScriptBox_Visible(data);
    }

    private void Start_Story_Data_FadeInOut(Story_Data_FadeInOut data)
    {
        _View.Start_Story_Data_FadeInOut(data);
    }

    private void Start_Story_Data_Object_Visible(Story_Data_Object_Visible data)
    {
        _View.Start_Story_Data_Object_Visible(data);
    }

    private void Start_Story_Data_Camera_Shaking(Story_Data_Camera_Shaking data)
    {
        _Camera.transform.DOShakePosition(data.Duration, data.Strength, data.Vibrato, fadeOut: data.FadeOut);
    }
}
