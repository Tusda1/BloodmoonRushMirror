using UnityEngine;

public class MusicTrigger : MonoBehaviour {
    [SerializeField] private int musicStage;
    [SerializeField] private string musicName;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!collision.CompareTag("Player")) return;
        
        if (!FmodManager.Instance.IsMusicPlaying()) {
            FmodManager.Instance.PlayMusic(musicName);
        }

        FmodManager.Instance.SetParameterValue("levelMusicProgression", musicStage);
    }
}