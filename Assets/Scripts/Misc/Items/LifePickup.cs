using UnityEngine;

public class LifePickup : ItemPickupBase
{
    protected override void Pickup()
    {
        player.playerLives++;
        base.Pickup();
    }
}
