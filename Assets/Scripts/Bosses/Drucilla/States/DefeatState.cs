using System;
using UnityEngine;

public class DefeatState : BossState
{
    private void OnEnable()
    {
        Boss.EndBattle();
    }
}
