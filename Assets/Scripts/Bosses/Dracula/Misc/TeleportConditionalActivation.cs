using System;
using UnityEngine;

public class TeleportConditionalActivation
{
    private bool _teleportActivated;
    
    public void ActivateTeleport(Vector2 draculaPosition, Vector2 targetPosition, Action onTeleportEnd, Action<Vector2> teleportMethod)
    {
        if (Vector2.Distance(draculaPosition, targetPosition) < 0.1f)
        {
            onTeleportEnd.Invoke();
        }
            
        teleportMethod.Invoke(targetPosition);
    }
}
