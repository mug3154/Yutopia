using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class SummonUnitCell : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] SummonUnitCellInfo _Info;

    public void SetData(UnitData unitData)
    {
        _Info.SetData(unitData);
    }

    public void Use()
    {
        _Info.Use();

        if(_Info.Usable())
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
        if(_Info.Usable())
        {
            BattleManager.Instance.CreateDragUnit(ref _Info.Data);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        BattleManager.Instance.CreateUnit(ref _Info.Data);
    }
}
