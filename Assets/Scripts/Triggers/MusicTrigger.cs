using FMODScripts;
using UnityEngine;

namespace Triggers {
    public class MusicTrigger : TriggerBox {
        #region ---Fields---
        [SerializeField] private int musicStage;
        [SerializeField] private string musicName;
        #endregion

        private static FMODManager FMODManager => FMODManager.Instance;

        public override void TriggeredAction() {
            if (!FMODManager.IsMusicPlaying()) {
                FMODManager.PlayMusic(musicName);
            }

            FMODManager.SetParameterValue("levelMusicProgression", musicStage);
        }
    }
}