using System;
using UnityEngine;

public class DraculaDeath : BossState
{
    private void OnEnable()
    {
        Boss.EndBattle();
    }
}
