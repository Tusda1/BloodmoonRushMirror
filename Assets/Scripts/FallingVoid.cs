using Attributes;
using Unity.VisualScripting;
using UnityEngine;
#if STEAM_BUILD
using Steamworks;
#endif

public class FallingVoid : MonoBehaviour {
    #region ---Fields---
    [GetOrAddComponent, SerializeField] private ScriptMachine cutsceneGraph;
    #endregion

    private void Start() {
        cutsceneGraph.TriggerUnityEvent("Start");
#if STEAM_BUILD
        SteamFriends.SetRichPresence("status", "#status_fallingvoid");
#endif
    }
}