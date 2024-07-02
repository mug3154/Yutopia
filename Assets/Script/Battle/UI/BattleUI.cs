using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUI : MonoBehaviour
{
    [SerializeField] BattleUITop _Top;
    [SerializeField] BattleUIBottom _Bottom;

    public void LoadingResources(Action<float> progressCallback)
    {
        _Bottom.LoadingResources(progressCallback);
    }


    public void Initialize(MapData data)
    {
        _Top.Initialize(data);
        _Bottom.Initialize(data);
    }


}
