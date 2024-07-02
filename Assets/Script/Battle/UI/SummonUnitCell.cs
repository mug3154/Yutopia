using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class SummonUnitCell : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public SummonUnitCellInfo Info;

    public void SetData(UnitData unitData, SummonUnitCellInfo info)
    {
        Info = info;
        Info.transform.SetParent(transform);
        Info.transform.localScale = Vector3.one;
        Info.transform.localPosition = Vector3.zero;
        Info.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        Info.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        Info.gameObject.SetActive(true);   
        Info.SetData(unitData);
    }

    public void Use()
    {
        Info.Use();

        if(Info.Usable())
        {
            GetComponent<CanvasGroup>().alpha = 1.0f;
        }
        else
        {
            GetComponent<CanvasGroup>().alpha = 0.5f;
        }
    }



    public void OnPointerDown(PointerEventData eventData)
    {
        if(Info.Usable())
        {
            BattleManager.Instance.CreateDragUnit(ref Info.Data);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //처음 여기서 PointerDown이 일어났기 때문에 다른 position에서 drag up해도 여기서 감지한다.
        BattleManager.Instance.DragUnitUI.ReadyUnitDirection();
    }
}
