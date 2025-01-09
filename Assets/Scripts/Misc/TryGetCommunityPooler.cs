using UnityEngine;
using System.Linq;

public class TryGetCommunityPooler
{
    public static Pooler TryGetPooler(GameObject prefabToFind)
    {
        try
        {
            var communityPooler = ServiceLocator.Get<CommunityPooler>();
            foreach (var pooler in communityPooler.poolers.Where(pooler => pooler.ObjectToPool == prefabToFind))
            {
                return pooler;
            }

            Debug.LogWarning("No pooler found for projectile, creating new pooler");
            var newPooler = new Pooler(prefabToFind, 5);
            communityPooler.poolers.Add(newPooler); // Register the new pooler
            return newPooler;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Couldnt find communitypooler: {ex.Message}");
            return new Pooler(prefabToFind, 5);
        }
    }
}
