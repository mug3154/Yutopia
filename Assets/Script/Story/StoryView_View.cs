using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

public class StoryView_View : MonoBehaviour
{
    ObjectPool<SpritePrefab> _ObjPool;
    Dictionary<string, SpritePrefab> _ObjDic;

    public void Initialize()
    {
        var handler = Addressables.LoadAssetAsync<GameObject>("SpritePrefab").WaitForCompletion();
        _ObjPool = new ObjectPool<SpritePrefab>( () => Instantiate(handler.GetComponent<SpritePrefab>()));
        Addressables.Release(handler);

        _ObjDic = new Dictionary<string, SpritePrefab>();
    }

    public SpritePrefab GetObj(string name, bool isNotNull = true)
    {
        SpritePrefab obj;

        if (_ObjDic.TryGetValue(name, out obj) == false)
        {
            if(isNotNull)
            {
                obj = _ObjPool.Get();
                obj.transform.SetParent(transform);
                _ObjDic.Add(name, obj);
            }
        }

        return obj;
    }

    public void Start_Story_Data_Object(Story_Data_Object data)
    {
        SpritePrefab obj = GetObj(data.Name);
            
        obj.SetResource(StoryManager.Instance.GetSprite(data.AtlasName, data.ResourceName));
        obj.transform.position = new Vector3(data.X, data.Y, 0);

        var sr = obj.GetComponent<SpriteRenderer>();
        sr.sortingOrder = data.OrderInLayer;
        sr.color = new Color(data.Color[0], data.Color[1], data.Color[2], data.Color[3]);

        obj.transform.localScale = new Vector3(data.ScaleX, data.ScaleY, 0);
    }


    public void Start_Story_Data_FadeInOut(Story_Data_FadeInOut data)
    {
        SpritePrefab obj = GetObj(data.TargetName, false);
        if (obj == null)
            return;

        var sr = obj.GetComponent<SpriteRenderer>();
        sr.color = new Color(data.StartColor[0], data.StartColor[1], data.StartColor[2], data.StartColor[3]);
        sr.DOColor(new Color(data.EndColor[0], data.EndColor[1], data.EndColor[2], data.EndColor[3]), data.Duration);//.SetEase((Ease)Enum.Parse(typeof(Ease), data.Ease)).SetUpdate(true);
    }


    public void Start_Story_Data_Object_Visible(Story_Data_Object_Visible data)
    {
        SpritePrefab obj = GetObj(data.TargetName, false);
        if (obj == null)
            return;

        obj.gameObject.SetActive(data.Visible);
    }
}
