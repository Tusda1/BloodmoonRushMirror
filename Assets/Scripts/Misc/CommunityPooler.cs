using UnityEngine;

public class CommunityPooler : MonoBehaviour 
{
    public Pooler pooler {get ; private set;}
    
    [SerializeField] private GameObject prefab;
    
    private void Awake()
    {
        pooler = new Pooler(prefab, 15, gameObject);
    }
}
