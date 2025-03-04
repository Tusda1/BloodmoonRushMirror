using System.Collections.Generic;
using Unity.VisualScripting;

[UnitTitle("Multi-Split")] 
[UnitCategory("Control")] 
[TypeIcon(typeof(Sequence))]
public class SplitOutputNode : Unit {
    [DoNotSerialize] public List<KeyValuePair<string, ControlOutput>> branches { get; private set; }

    [DoNotSerialize] [PortLabelHidden] public ControlInput inputTrigger { get; private set; }

    [Inspectable, Serialize] public List<string> options { get; set; } = new List<string>();

    protected override void Definition() {
        inputTrigger = ControlInput(nameof(inputTrigger), OnEnter);

        branches = new List<KeyValuePair<string, ControlOutput>>();

        foreach (var option in options) {
            if (string.IsNullOrEmpty(option)) continue;

            var key = $"%{option}";

            if (controlOutputs.Contains(key)) continue;

            var branch = ControlOutput(key);
            branches.Add(new KeyValuePair<string, ControlOutput>(option, branch));
            Succession(inputTrigger, branch);
        }
    }

    private ControlOutput OnEnter(Flow arg) {
        var reference = arg.stack.ToReference();

        foreach (var branch in branches) {
            var temp = Flow.New(reference);
            temp.StartCoroutine(branch.Value);
        }

        return null;
    }
}