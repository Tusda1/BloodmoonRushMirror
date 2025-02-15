using Player;
using UnityEngine;
using VContainer;

public class FollowPlayerExact : MonoBehaviour {
    [Inject] private PlayerScript _player;

    private void FixedUpdate() {
        if (_player != null) transform.position = _player.transform.position;
    }
}
