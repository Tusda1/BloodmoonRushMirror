using UnityEngine;
using UnityEngine.Serialization;

public class SpawnEntity : DrucillaState
{
    [SerializeField] private GameObject entityPrefab;
    [SerializeField] private int entityCount = 2;
    
    private Pooler _pooler;
    private CornerFinder _cornerFinder;
    
    private void Start()
    {
        _pooler = TryGetCommunityPooler.TryGetPooler(entityPrefab);
        _cornerFinder = new CornerFinder(transform, LayerMask.GetMask("Ground"));
    }
    
    private void OnEnable()
    {
        var cornerOne = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomLeft);
        var cornerTwo = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomRight);
        
        for (int i = 0; i < entityCount; i++)
        {
            // Find random point between left and right corner
            var position = new Vector2(Random.Range(cornerOne.x, cornerTwo.x), cornerOne.y);
            var enemy = _pooler.GetPooledObject();
            enemy.transform.position = position;
            enemy.SetActive(true);
        }
        
        EndState();
    }
}
