using System;
using SaveLoadSystem.Data;
using UnityEngine;

public partial class EnemyBase {
    #region ---Fields---
    [field: Header("Data Binding")]
    [field: SerializeField] public string ID { get; set; }

    [field: SerializeField] public EnemyData SaveSystemData { get; set; }

    public EnemyData DataInGameData => _levelDataManager.SaveSystemData.enemyDatas
        .GetOrAddByIDInList(ID, NewEnemyDataGenerator);
    
    private Func<EnemyData> NewEnemyDataGenerator => () => new EnemyData {
        name = name,
        ID = ID,
        health = currentHealth,
        gemsContaining = rubyDropAmount,
        isAlive = !isDead,
        position = transform.position
    };
    #endregion

    #region ---DataBindingMethods---
    public virtual void Bind() {
#if SAVESYSTEM_ACTIVE
        _saveLoadSystemManager.DataBinder.Bind(this, DataInGameData);
#endif
    }

    public virtual void LoadBindData() {
        name = SaveSystemData.name;
        currentHealth = SaveSystemData.health;
        rubyDropAmount = SaveSystemData.gemsContaining;
        isDead = !SaveSystemData.isAlive;
        transform.position = SaveSystemData.position;
    }

    public virtual void SaveBindData() {
        SaveSystemData.name = name;
        SaveSystemData.health = currentHealth;
        SaveSystemData.gemsContaining = rubyDropAmount;
        SaveSystemData.isAlive = !isDead;
        SaveSystemData.position = transform.position;
    }
    #endregion
}