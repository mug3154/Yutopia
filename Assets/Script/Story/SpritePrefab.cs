using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpritePrefab : MonoBehaviour
{
    [SerializeField] SpriteRenderer _SpriteRenderer;

    public void SetResource(Sprite sprite)
    {
        _SpriteRenderer.sprite = sprite;
    }
}
