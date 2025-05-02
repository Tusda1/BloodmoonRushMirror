using System;
using UnityEngine;

public class PreFightStateDracula : DraculaState
{
    private void OnEnable()
    {
        Boss.AnimancerComponent.Play(idleAnimation);
    }
}
