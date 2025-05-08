using UnityEngine;

public class Ruby : ItemPickupBase
{
    protected override void Pickup() {
        player.AddGems(1);
        base.Pickup();
    }
}
