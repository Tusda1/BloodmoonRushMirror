using SaveLoadSystem.Data;
using UnityEngine;

namespace Player {
    public partial class PlayerScript {
        #region ---Fields---
        [field: Header("Data Binding")]
        [field: SerializeField] public string ID { get; set; } = "Player";

        [field: SerializeField] public PlayerData SaveSystemData { get; set; }

        public PlayerData DataInGameData {
            get {
                if (string.IsNullOrEmpty(PlayerData.ID)) {
                    PlayerData.ID = ID;
                    PlayerData.health = healthComponent.Health;
                    PlayerData.lives = playerLives;
                    PlayerData.gems = gems;
                }

                return PlayerData;
            }
        }

        private PlayerData PlayerData => _saveLoadSystemManager.CurrentGameData.playerData;
        #endregion

        #region ---DataBindingMethods---
        public void Bind() {
#if SAVESYSTEM_ACTIVE
            _saveLoadSystemManager.DataBinder.Bind(this, DataInGameData);
#endif
        }

        public void Unbind() {
#if SAVESYSTEM_ACTIVE
            _saveLoadSystemManager.DataBinder.UnBind(this, DataInGameData);
#endif
        }

        public void LoadBindData() {
            healthComponent.Health = SaveSystemData.health;
            healthComponent.TempHealth = SaveSystemData.tempHealth;
            playerLives = SaveSystemData.lives;
            gems = SaveSystemData.gems;
        }

        public void SaveBindData() {
            SaveSystemData.ID = ID;
            SaveSystemData.health = healthComponent.Health;
            SaveSystemData.lives = playerLives;
            SaveSystemData.gems = gems;
        }
        #endregion
    }
}