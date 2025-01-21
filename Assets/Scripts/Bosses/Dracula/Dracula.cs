using UnityEngine;

public class Dracula : BossBase
{
    [SerializeField] private AnimationClip teleportEnter;
    public AnimationClip TeleportEnter => teleportEnter;
    [SerializeField] private AnimationClip teleportExit;
    public AnimationClip TeleportExit => teleportExit;
}
