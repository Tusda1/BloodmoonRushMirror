using Fmod;
using UnityEngine;

namespace Triggers {
    public class MusicTrigger : TriggerBox {
        [SerializeField] private int musicStage;
        [SerializeField] private string musicName;

        private FmodManager fmodManager => FmodManager.Instance;

        public override void TriggeredAction() {
            if (!fmodManager.IsMusicPlaying()) {
                fmodManager.PlayMusic(musicName);
            }

            fmodManager.SetParameterValue("levelMusicProgression", musicStage);
        }
    }
}