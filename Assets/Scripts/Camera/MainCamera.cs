using FMODUnity;
using Player;
using UnityEngine;
using VContainer;

public class MainCamera : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private StudioListener listener;
    
    private PlayerScript _player;
    #endregion

    [Inject]
    public void Construct(IObjectResolver container) {
        _player = container.Resolve<PlayerScript>();
        listener.attenuationObject = _player.gameObject;
    }
}
