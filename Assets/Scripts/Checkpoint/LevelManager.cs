using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private CheckpointData checkpointData;
    [SerializeField] private EnemyActivation enemyActivation;
    [SerializeField] private GameObject[] checkpointCameraArea;
    [SerializeField] private CombatCheckpoint[] combatCheckpoints;
    [SerializeField] private GameObject enemyLocation;
    
    public void Start()
    {
        // Turns checkpointcameraareas into a list of rooms. Rooms are children of checkcameraareaobjects
        List<GameObject> rooms = new List<GameObject>();
        
        foreach (var area in checkpointCameraArea)
        {
            foreach (Transform child in area.transform)
            {
                rooms.Add(child.gameObject);
            }
        }
        
        checkpointData.combatCheckpoints = combatCheckpoints;
        enemyActivation.rooms = rooms.ToArray();
        enemyActivation.enemiesLocation = enemyLocation.name;
        
        checkpointData.Initialize();
        enemyActivation.Initialize();
    }
}
