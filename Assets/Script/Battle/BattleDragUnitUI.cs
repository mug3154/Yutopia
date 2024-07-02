using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleDragUnitUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    [SerializeField] DragUnit _DragUnit;
    public DragUnit DragUnit => _DragUnit;
    RectTransform _DragUnitRectTr;

    [SerializeField] GameObject _GuideBox;

    bool _IsDirectionMode = false;
    bool _IsMouseDown = false;

    Vector2 _DirectionDownPos;
    Vector3Int _Direction;
    Vector3Int _UnitTilePos;
    Vector3Int[] _UnitAttackArea;

    public void Initialize()
    {
        gameObject.SetActive(false);

        _DragUnitRectTr = _DragUnit.transform.GetComponent<RectTransform>();
    }

    public void ShowDragUnit(ref UnitData data)
    {
        _IsDirectionMode = false;
        _IsMouseDown = false;

        _GuideBox.SetActive(false);

        _DragUnit.SetData(ref data);

        gameObject.SetActive(true);

        Time.timeScale = 0.2f;
    }

    public void HideDragUnit()
    {
        gameObject.SetActive(false);

        Time.timeScale = 1f;
    }

    public void ReadyUnitDirection()
    {
        _UnitTilePos = BattleManager.Instance.GetTilemapPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (_UnitTilePos == BattleConfig.TrashValueVector3Int)
        {
            BattleManager.Instance.HideDragUnit();

            return;
        }

        _IsDirectionMode = true;
        _Direction = Vector3Int.zero;

        //юс╫ц
        _UnitAttackArea = new Vector3Int[1] { new Vector3Int(1, 1, 0) };

        _GuideBox.SetActive(true);

        _DragUnitRectTr.anchoredPosition = Camera.main.WorldToScreenPoint(BattleConfig.GetTilePosition(_UnitTilePos));
        _DragUnit.ReadySelectDirection();
    }

    private void FixedUpdate()
    {
        if (_IsDirectionMode == false) 
        {
            _DragUnitRectTr.anchoredPosition = Input.mousePosition;
        }
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if(_IsDirectionMode)
        {
            _IsMouseDown = true;

            _DirectionDownPos = Input.mousePosition;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_IsDirectionMode)
        {
            _IsMouseDown = false;
            Debug.Log("up");

            BattleManager.Instance.HideDragUnit();

            BattleManager.Instance.CreateUnit(ref _DragUnit.Data, _UnitTilePos);
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (_IsDirectionMode == false)
            return;

        if (_IsMouseDown == false)
            return;

        if(Vector2.Distance(_DirectionDownPos, Input.mousePosition) > 2)
        {
            float divX = Input.mousePosition.x - _DirectionDownPos.x;
            float divY = Input.mousePosition.y - _DirectionDownPos.y;

            float xDistance = divX >= 0 ? divX : divX * -1f;
            float yDistance = divY >= 0 ? divY : divY * -1f;

            if (xDistance == yDistance)
            {

            }
            else if (xDistance > yDistance)
            {
                if(divX < 0)
                {
                    SetDirection(Vector3Int.left);
                }
                else
                {
                    SetDirection(Vector3Int.right);
                }
            }
            else
            {
                if(divY < 0)
                {
                    SetDirection(Vector3Int.down);
                }
                else
                {
                    SetDirection(Vector3Int.up);
                }
            }
        }
    }

    private void SetDirection(Vector3Int direction)
    {
        _Direction = direction;
        _DragUnit.SetDirection(direction);
        BattleManager.Instance.View.DrawSelectDirection(_UnitTilePos, _Direction, _UnitAttackArea);
    }
}
