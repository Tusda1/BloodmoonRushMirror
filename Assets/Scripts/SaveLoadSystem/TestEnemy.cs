using Pathfinding.Util;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using UnityEngine;
using Utilities.DataBinding;

public class TestEnemy : MonoBehaviour, IBind<EnemyData> {
    #region ---Fields---
    [SerializeField] private int health = 100;
    [SerializeField] private bool isAlive = true;
    [SerializeField] private int gemsContaining = 5;

    #region ***---GameManagers---***
    private static SaveLoadSystemManager saveLoadSystemManager => SaveLoadSystemManager.Instance;
    #endregion

    #region ***---DataBindingVariables---***
    [field: Header("Data Binding")]
    [field: SerializeField]
    public string id { get; set; }

    [field: SerializeField] public EnemyData saveSystemData { get; set; }

    public EnemyData dataInGameData => saveLoadSystemManager.gameData.enemies.GetOrAddByIdInList(id, new EnemyData {
        id = id,
        health = health,
        gemsContaining = gemsContaining,
        isAlive = isAlive,
    });
    #endregion
    #endregion

    private void Start() {
        Bind();
    }

    public void Bind() {
        saveLoadSystemManager.Bind(this, dataInGameData);
    }

#if UNITY_EDITOR
    private void OnValidate() {
        if (!gameObject.activeInHierarchy) {
            return;
        }
        
        if (string.IsNullOrEmpty(saveSystemData.id)) {
            id = Guid.NewGuid().ToString();
        }
    }
#endif

    public void LoadBindData() {
        isAlive = saveSystemData.isAlive;
        health = saveSystemData.health;
        gemsContaining = saveSystemData.gemsContaining;
        transform.position = saveSystemData.position;
    }

    public void SaveBindData() {
        saveSystemData.isAlive = isAlive;
        saveSystemData.health = health;
        saveSystemData.gemsContaining = gemsContaining;
        saveSystemData.position = transform.position;
    }
}