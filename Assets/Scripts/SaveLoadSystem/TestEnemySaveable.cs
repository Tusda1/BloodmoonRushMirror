using Pathfinding.Util;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using UnityEngine;
using Utilities.DataBinding;

namespace Enemies {
    public class TestEnemySaveable : MonoBehaviour, IBind<EnemyData> {
        #region ---Fields---
        [SerializeField] private int health = 3;
        [SerializeField] private bool isAlive = true;
        [SerializeField] private int gemsContaining = 5;

        #region ***---GameManagers---***
        private static SaveLoadSystemManager SaveLoadSystemManager => SaveLoadSystemManager.Instance;
        private static LevelDataManager LevelDataManager => LevelDataManager.Instance;
        #endregion

        #region ***---DataBindingVariables---***
        [field: Header("Data Binding")]
        [field: SerializeField] public string ID { get; set; }

        [field: SerializeField] public EnemyData SaveSystemData { get; set; }

        public EnemyData DataInGameData => LevelDataManager.SaveSystemData.enemyDatas.GetOrAddByIdInList(ID,
            new EnemyData {
                ID = ID,
                health = health,
                gemsContaining = gemsContaining,
                isAlive = isAlive
            });
        #endregion
        #endregion

        #region ---EventFunctions---
        private void Start() {
            Bind();
        }
        #endregion

        #region ---DataBindingMethods---
        public void Bind() {
            SaveLoadSystemManager.Bind(this, DataInGameData);
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (gameObject.activeInHierarchy && string.IsNullOrEmpty(ID)) {
                ID = Guid.NewGuid().ToString();
                Debug.Log($"Setting new guid for {nameof(TestEnemySaveable)}");
            }
        }
#endif

        [ContextMenu(nameof(LoadBindData))]
        public void LoadBindData() {
            name = SaveSystemData.name;
            isAlive = SaveSystemData.isAlive;
            health = SaveSystemData.health;
            gemsContaining = SaveSystemData.gemsContaining;
            transform.position = SaveSystemData.position;
        }

        [ContextMenu(nameof(SaveBindData))]
        public void SaveBindData() {
            SaveSystemData.name = name;
            SaveSystemData.isAlive = isAlive;
            SaveSystemData.health = health;
            SaveSystemData.gemsContaining = gemsContaining;
            SaveSystemData.position = transform.position;
        }
        #endregion
    }
}