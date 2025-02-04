using System;
using UnityEngine;

[CreateAssetMenu(
    menuName = "Scriptable Objects/InputLocalizationData/Create " + nameof(InputLocalizationData), 
    fileName = "ID_" + nameof(InputLocalizationData))] 
public class InputLocalizationData : ScriptableObject {
    [Header("ID")]
    public string id;
    public string controlScheme;
    
    [Header("Assets")]
    public Sprite jumpButton;
    public SpriteAnimation jumpButtonAnimation;
    
    public Sprite attackButton;
    public SpriteAnimation attackButtonAnimation;
    
    public Sprite dashButton;
    public SpriteAnimation dashButtonAnimation;

    [Serializable] public class SpriteAnimation {
        public float secondsPerFrame = 0.2f;
        public Sprite[] frames;
    }
}