using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private CheckpointData checkpointData;
    [SerializeField] private EnemyActivation enemyActivation;
    [SerializeField] private GameObject cameraStacks;
    [SerializeField] private CombatCheckpoint[] combatCheckpoints;
    [SerializeField] private GameObject enemyLocation;
    
    private void Start()
    {
        // Turns checkpointcameraareas into a list of rooms. Rooms are children of checkcameraareaobjects
        List<GameObject> areas = new List<GameObject>();
        List<GameObject> rooms = new List<GameObject>();
        
        // Iterate through all children of cameraStacks toi find rooms
        foreach (Transform child in cameraStacks.transform)
        {
            areas.Add(child.gameObject);
        }
        
        // Iterate through all children of areas to find rooms
        foreach (var area in areas)
        {
            foreach (Transform child in area.transform)
            {
                // Check if child is on room layer
                if (child.gameObject.layer == LayerMask.NameToLayer("Room"))
                {
                    rooms.Add(child.gameObject);
                }
            }
        }
        
        checkpointData.combatCheckpoints = combatCheckpoints;
        
        checkpointData.Initialize();
        enemyActivation.Initialize(rooms.ToArray(), enemyLocation.name);
    }
}
