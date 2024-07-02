using System;
using UnityEngine;
using UnityEngine.UI;

public class DragUnit : MonoBehaviour
{
    [SerializeField] Image _Icon;
    [SerializeField] Button _CancelBtn;
    [SerializeField] RectTransform _Arrow;

    RectTransform _Rect;


    public UnitData Data;

    Quaternion Down = Quaternion.Euler(0f, 0f, 0f);
    Quaternion Up = Quaternion.Euler(0f, 0f, 180f);
    Quaternion Left = Quaternion.Euler(0f, 0f, -90f);
    Quaternion Right = Quaternion.Euler(0f, 0f, 90f);


    private void Awake()
    {
        _Rect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        _CancelBtn.onClick.AddListener(OnClickCancelBtn);
    }

    private void OnClickCancelBtn()
    {
        BattleManager.Instance.HideDragUnit();
    }

    //// Update is called once per frame
    //void FixedUpdate()
    //{
    //    _Rect.anchoredPosition = Input.mousePosition;
    //}


    public void SetData(ref UnitData data)
    {
        Data = data;
 
        _Icon.sprite = BattleManager.Instance.BattleAtlas.GetSprite(data.ResourceName);
        _Icon.SetNativeSize();

        gameObject.SetActive(true);
      
        _CancelBtn.gameObject.SetActive(false);
        _Arrow.gameObject.SetActive(false);
    }

    public void ReadySelectDirection()
    {
        _CancelBtn.gameObject.SetActive(true);
        _Arrow.gameObject.SetActive(true);
    }

    public void SetDirection(Vector3Int direction)
    {
        _Arrow.gameObject.SetActive(true);

        if(direction == Vector3Int.down)
        {
            _Arrow.rotation = Down;
        }
        else if (direction == Vector3Int.up)
        {
            _Arrow.rotation = Up;
        }
        else if (direction == Vector3Int.left)
        {
            _Arrow.rotation = Left;
        }
        else
        {
            _Arrow.rotation = Right;
        }
    }
}
