using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

[CreateAssetMenu(
    fileName = nameof(DialogueUI_DataContainer), 
    menuName = "Scriptable Objects/Create " + nameof(DialogueUI_DataContainer))]
public class DialogueUI_DataContainer : ScriptableObject {
    [Header("Data")] 
    public DisplayStyle uiDocumentVisible;
    public DisplayStyle playerUIDocumentVisible;

    #region ---CharacterData---
    [Header("Character Data")]
    public Sprite characterPortrait;
    public string characterName;
    public FontAsset characterNameFontAsset;
    public float characterNameSize;
    public Color characterNameColor;
    public SpriteAnimation spriteAnimation;
    public float staticSpriteOpacity;
    #endregion
    
    #region ---TextData---
    [Header("Text Data")]
    public string text;
    public FontAsset textFontAsset;
    public float textSize;
    public float textSpeed;
    public Color textColor;
    #endregion

    [Preserve]
    public void Clear() {
        text = "";
        characterPortrait = null;
        characterName = "";
    }
}