using System.Linq;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

[UnitShortTitle("Set Action Map")] 
[UnitCategory("DialogueUI")] 
[TypeIcon(typeof(BoltAnimationEvent))]
public class SetActionMapEnabled : Unit {
    #region ---Fields---
    #region ***---Control_IO---***
    [DoNotSerialize] [PortLabelHidden] public ControlInput Enter;
    [DoNotSerialize] [PortLabelHidden] public ControlOutput Exit;
    #endregion
    
    #region ***---Input---***
    public ValueInput PlayerInput;
    public ValueInput ActionMap;
    public ValueInput State;
    #endregion
    #endregion
    
    protected override void Definition() {
        Enter = ControlInput(nameof(Enter), Action);
        Exit = ControlOutput(nameof(Exit));
        Succession(Enter, Exit);
        
        PlayerInput = ValueInput<PlayerInput>(nameof(PlayerInput), null);
        ActionMap = ValueInput(nameof(ActionMap), 0);
        State = ValueInput(nameof(State), false);
    }

    private ControlOutput Action(Flow flow) {
        var playerInput = flow.GetValue<PlayerInput>(PlayerInput);
        var actionMap = flow.GetValue<int>(ActionMap);
        var state = flow.GetValue<bool>(State);

        if (state) {
            playerInput.actions.actionMaps[actionMap].Enable();
        }
        else {
            playerInput.actions.actionMaps[actionMap].Disable();
        }
        
        return Exit;
    }
}