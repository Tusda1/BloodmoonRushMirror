using UnityEngine;

public class HeartPickup : ItemPickupBase
{
    protected override void Pickup()
    {
        HealPlayer();
        base.Pickup();
    }
    
    private void HealPlayer()
    {
        if (player.healthComponent.Health == 7)
        {
            player.healthComponent.TempHealth++;
        }
        else
        {
            player.healthComponent.Heal(1);
        }
    }
}
