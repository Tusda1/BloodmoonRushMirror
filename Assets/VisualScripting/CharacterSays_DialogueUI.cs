using System.Collections;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

// [UnitTitle("CharacterSays_DialogueUI")]
[UnitShortTitle("Character Says")]
// [UnitSubtitle("It's a subtitle!")]
[UnitCategory("DialogueUI")]
[TypeIcon(typeof(BoltAnimationEvent))]
public class CharacterSays_DialogueUI : Unit {
    [DoNotSerialize] [PortLabelHidden] public ControlInput Enter;
    [DoNotSerialize] [PortLabelHidden] public ControlOutput Exit;

    public ValueInput DialogueCharacter;
    public ValueInput TextID;
    public ValueInput AutoContinue;
    
    [Inspectable] public Sprite PortraitOverwrite;
    [Inspectable] public FontAsset TextFontAssetOverwrite;
    [Inspectable] public string CharacterNameOverwrite;
    [Inspectable] public FontAsset CharacterNameFontAssetOverwrite;
    [Inspectable] public float TextSizeOverwrite;
    [Inspectable] public float TextSpeedOverwrite;
    [Inspectable] public Color TextColorOverwrite;
    
    private static DialogueUI DialogueUI => DialogueUI.Instance;

    protected override void Definition() {
        Enter = ControlInputCoroutine(nameof(Enter), Action);
        Exit = ControlOutput(nameof(Exit));
        Succession(Enter, Exit);

        DialogueCharacter = ValueInput<DialogueCharacterSO>(nameof(DialogueCharacter), null);
        TextID = ValueInput(nameof(TextID), string.Empty);
        AutoContinue = ValueInput(nameof(AutoContinue), false);
    }
    
    private IEnumerator Action(Flow flow) {
        DialogueUI.Initialize();
        
        yield return new WaitUntil(() => DialogueUI.Typewriter().IsCompleted);
        
        yield return Exit;
    }
}