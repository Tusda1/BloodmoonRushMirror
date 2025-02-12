using System;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using System.Linq;
using ExtensionMethods;
using UnityEditor;
#endif

[CreateAssetMenu(
    menuName = "Scriptable Objects/Create " + nameof(ControlSchemeDataSO),
    fileName = "ID_" + nameof(ControlSchemeDataSO))]
public class ControlSchemeDataSO : ScriptableObject {
    #region ---ID---
    [Header("ID")] 
    public string id;
    public string controlScheme;
    #endregion

    #region ---WizardText---
    [Header("WizardText")]
    public AnimationClip move_InputAnimation;
    public AnimationClip jump_InputAnimation;
    public AnimationClip attack_InputAnimation;
    public AnimationClip dash_InputAnimation;
    #endregion

    #region ---UI---
    [FormerlySerializedAs("controllerSettingPanel")] [Header("UI")] 
    public Sprite controlsLayout;
    public SpriteAnimation skipDialogueButtonAnimation;
    #endregion

    public const string FolderName = "ControlSchemeData";

#if UNITY_EDITOR
    [ContextMenu(nameof(AutoSetAllSpriteAnimation))]
    public void AutoSetAllSpriteAnimation() {
        var spriteAnimations = this.GetFieldsOfType<SpriteAnimation>();

        foreach (var spriteAnimation in spriteAnimations) {
            AutoSetSpriteAnimation(spriteAnimation);
        }
    }

    private static void AutoSetSpriteAnimation(SpriteAnimation spriteAnimation) {
        if (spriteAnimation.autoSetTexture == null) return;

        var path = AssetDatabase.GetAssetPath(spriteAnimation.autoSetTexture);
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .ToArray();

        spriteAnimation.frames = sprites;
    }
#endif

    [Serializable] public class SpriteAnimation {
        public Texture2D autoSetTexture;
        public float secondsPerFrame = 0.2f;
        public Sprite[] frames;
    }
}