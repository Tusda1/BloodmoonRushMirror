using UnityEngine;

public class Ruby : ItemPickupBase
{
    protected override void Pickup()
    {
        audioManager.PlayOneShot(pickupSoundID);
        player.gems++;
        enabled = false;
    }
}
