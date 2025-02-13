using Player;
using UnityEngine;
using VContainer;

public class FollowPlayerExact : MonoBehaviour {
    [Inject] private PlayerScript _player;

    private void FixedUpdate() {
        transform.position = _player.transform.position;
    }
}
