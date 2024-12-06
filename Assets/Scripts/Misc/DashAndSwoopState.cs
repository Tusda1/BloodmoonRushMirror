using UnityEngine;

public class DashAndSwoopState : DraculaState
{
    
    [SerializeField] private Swoop swoop = new();
    private Teleport _teleport;
}
