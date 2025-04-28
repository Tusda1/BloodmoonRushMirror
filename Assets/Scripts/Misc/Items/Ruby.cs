using UnityEngine;

public class Ruby : ItemPickupBase
{
    protected override void Pickup()
    {
        player.gems++;
        base.Pickup();
    }
}
