using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragUnit : MonoBehaviour
{
    [SerializeField] Image _Icon;
    RectTransform _Rect;

    public UnitData Data;

    private void Awake()
    {
        _Rect = GetComponent<RectTransform>();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        _Rect.anchoredPosition = Input.mousePosition;
    }


    public void SetData(ref UnitData data)
    {
        Data = data;
 
        _Icon.sprite = BattleManager.Instance.BattleAtlas.GetSprite(data.ResourceName);
        _Icon.SetNativeSize();

        gameObject.SetActive(true);
    }

}
