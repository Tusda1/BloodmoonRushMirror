using Player;
using UI;
using UnityEngine;
using VContainer;

public class LevelUI_Installer : AutoInjectInstaller {
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private DialogueUI dialogueUI;
    
    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterInstance(playerUI);
        builder.RegisterInstance(pauseMenu);
        builder.RegisterInstance(dialogueUI);
    }

#if UNITY_EDITOR
    [ContextMenu(nameof(TrySetReferences))]
    public void TrySetReferences() {
        SetReferenceFromSceneIfNull(ref playerUI);
        SetReferenceFromSceneIfNull(ref pauseMenu);
        SetReferenceFromSceneIfNull(ref dialogueUI);
    }
#endif
}