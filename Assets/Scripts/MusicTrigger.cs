using System;
using UnityEngine;
using UnityEngine.LowLevelPhysics;

public class MusicTrigger : MonoBehaviour
{
    [SerializeField] private int musicStage;
    [SerializeField] private string musicName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!FmodManager.Instance.IsMusic())
            {
                FmodManager.Instance.PlayMusic(musicName);
            }
            FmodManager.Instance.SetParameterValue("levelMusicProgression", musicStage);
        }
    }
}
