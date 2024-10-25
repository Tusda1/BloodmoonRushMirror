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
    [field: SerializeField] public string id { get; set; } = Guid.NewGuid().ToString();

    [field: SerializeField] public EnemyData saveSystemData { get; set; }
    public EnemyData dataInGameData => saveLoadSystemManager.gameData.enemies.GetOrAddByIdInList(id, new EnemyData() {
        id = id,
        health = health,
        gemsContaining = gemsContaining,
        isAlive = isAlive,
    });
    #endregion
    #endregion

    private void Start() {
        saveLoadSystemManager.Bind(this, dataInGameData);
    }

    public void LoadBindData() {
        isAlive = dataInGameData.isAlive;
        health = dataInGameData.health;
        gemsContaining = dataInGameData.gemsContaining;
    }

    public void SaveBindData() {
        dataInGameData.isAlive = isAlive;
        dataInGameData.health = health;
        dataInGameData.gemsContaining = gemsContaining;
    }
}