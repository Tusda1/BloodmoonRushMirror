using FMODScripts;
using UnityEngine;

namespace Triggers {
    public class MusicTrigger : TriggerBox {
        #region ---Fields---
        [SerializeField] private string musicName;
        [SerializeField] private int musicStage;
        #endregion

        public override void TriggeredAction() {
            FMODManager.RaiseMusicEvent(musicName, "levelMusicProgression", musicStage);
        }
    }
}