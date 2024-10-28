using Pathfinding.Util;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using UnityEngine;
using Utilities.DataBinding;

public class TestEnemySaveable : MonoBehaviour, IBind<EnemyData> {
    #region ---Fields---
    [SerializeField] private int health = 3;
    [SerializeField] private bool isAlive = true;
    [SerializeField] private int gemsContaining = 5;

    #region ***---GameManagers---***
    private static SaveLoadSystemManager SaveLoadSystemManager => SaveLoadSystemManager.Instance;
    #endregion

    #region ***---DataBindingVariables---***
    [field: Header("Data Binding")]
    [field: SerializeField]
    public string id { get; set; }

    [field: SerializeField] public EnemyData saveSystemData { get; set; }

    public EnemyData dataInGameData => SaveLoadSystemManager.gameData.enemies.GetOrAddByIdInList(id, new EnemyData {
        id = id,
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
        SaveLoadSystemManager.Bind(this, dataInGameData);
    }

#if UNITY_EDITOR
    private void OnValidate() {
        if (gameObject.activeInHierarchy && string.IsNullOrEmpty(id)) {
            id = Guid.NewGuid().ToString();
            Debug.Log($"Setting new guid for {nameof(TestEnemySaveable)}");
        }
    }
#endif

    [ContextMenu(nameof(LoadBindData))]
    public void LoadBindData() {
        name = saveSystemData.name;
        isAlive = saveSystemData.isAlive;
        health = saveSystemData.health;
        gemsContaining = saveSystemData.gemsContaining;
        transform.position = saveSystemData.position;
    }

    [ContextMenu(nameof(SaveBindData))]
    public void SaveBindData() {
        saveSystemData.name = name;
        saveSystemData.isAlive = isAlive;
        saveSystemData.health = health;
        saveSystemData.gemsContaining = gemsContaining;
        saveSystemData.position = transform.position;
    }
    #endregion
}