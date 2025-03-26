using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.XInput;
using VContainer;

#pragma warning disable UDR0005
public class ControllerRecognizer : IDisposable {
    #region ---Fields---
    public string CurrentDeviceID { get; private set; }
    public InputDevice CurrentDevice { get; private set; }

    #region ***---Events---***
    public event Action<string, InputDevice> OnDeviceChanged = delegate { };
    public event Action<InputDevice> ControllerInputDetected = delegate { };
    public event Action<InputDevice> KeyboardInputDetected = delegate { };
    #endregion

    private readonly Dictionary<string, InputDevice> _currentDevices = new();

    private readonly IDisposable _onAnyButtonPressDisposable; 

    #region ***---Consts---***
    public const string XboxDeviceID = "XBOX";
    public const string PlaystationDeviceID = "PLAYSTATION";
    public const string NintendoDeviceID = "NINTENDO";

    public const string DefaultGamepadControlsID = XboxDeviceID;
    #endregion
    #endregion

    #region ---Construction---
    [Inject]
    public ControllerRecognizer() {
        InputSystem.onDeviceChange += OnDeviceChangeAutomatic;
        _onAnyButtonPressDisposable = InputSystem.onAnyButtonPress.Call(OnAnyButtonPress);
        
        GetAllControllersManually();
    }

    public void Dispose() {
        InputSystem.onDeviceChange -= OnDeviceChangeAutomatic;
        _onAnyButtonPressDisposable.Dispose();
    }
    #endregion

    #region ---SetCurrentDevice---
    private void SetCurrentDevice(string id, InputDevice device) {
        CurrentDeviceID = id;
        CurrentDevice = device;

        OnDeviceChanged.Invoke(CurrentDeviceID, CurrentDevice);
    }

    private void SetCurrentDevice(InputDevice inputDevice) {
        SetCurrentDevice(InputDeviceToID(inputDevice), inputDevice);
    }
    #endregion

    #region ---PrivateMethods---
    private void OnAnyButtonPress(InputControl inputControl) {
        var inputDevice = inputControl.device;
        
        if (inputDevice == CurrentDevice) return;

        switch (inputDevice) {
            case Keyboard:
                CurrentDevice = inputDevice;
                KeyboardInputDetected.Invoke(inputDevice);
                break;
            case Gamepad or Joystick:
                SetCurrentDevice(inputDevice);
                ControllerInputDetected.Invoke(inputDevice);
                break;
        }
    }

    private void OnDeviceChangeAutomatic(InputDevice inputDevice, InputDeviceChange inputDeviceChange) {
        if (inputDevice is not (Gamepad or Joystick)) return;

        switch (inputDeviceChange) {
            case InputDeviceChange.Added: {
                var connectedControllers = GetAllConnectedControllers();
                if (PlaystationDualInputDeviceFix(inputDevice, connectedControllers)) break;

                AddDevice(inputDevice);
                break;
            }
            case InputDeviceChange.Removed: {
                RemoveDevice(inputDevice);
                break;
            }
        }
    }

    private bool PlaystationDualInputDeviceFix(InputDevice inputDevice, InputDevice[] connectedControllers) {
        return inputDevice is DualShockGamepad &&
               (_currentDevices.ContainsKey(PlaystationDeviceID) ||
                connectedControllers.Count(x => x is DualShockGamepad) > 1);
    }
    #endregion

    #region ---PublicMethods---
    public static InputDevice[] GetAllConnectedControllers() {
        return InputSystem.devices.Where(x => x is Gamepad or Joystick).ToArray();
    }

    public void GetAllControllersManually() {
        var connectedControllers = GetAllConnectedControllers();

        foreach (var inputDevice in connectedControllers) {
            if (PlaystationDualInputDeviceFix(inputDevice, connectedControllers)) continue;

            _currentDevices.Add(InputDeviceToID(inputDevice), inputDevice);
        }

        if (_currentDevices.Count <= 0) return;

        var device = _currentDevices.First();
        SetCurrentDevice(device.Key, device.Value);
    }

    public void AddDevice(InputDevice inputDevice) {
        if (inputDevice is not (Gamepad or Joystick)) return;

        var id = InputDeviceToID(inputDevice);

        if (!_currentDevices.TryAdd(id, inputDevice)) return;

        SetCurrentDevice(id, inputDevice);
    }

    public void RemoveDevice(InputDevice inputDevice) {
        if (inputDevice is not (Gamepad or Joystick)) return;

        var id = InputDeviceToID(inputDevice);

        if (!_currentDevices.ContainsKey(id)) return;

        _currentDevices.Remove(id);

        if (CurrentDeviceID != id) return;
        var newID = _currentDevices.FirstOrDefault();

        SetCurrentDevice(newID.Key, newID.Value);
    }

    public static string InputDeviceToID(InputDevice inputDevice) {
        if (inputDevice is not (Gamepad or Joystick)) {
            throw new ArgumentException("Input Device is not a Gamepad or Joystick");
        }

        return inputDevice switch {
            XInputController => XboxDeviceID,
            DualShockGamepad => PlaystationDeviceID,
            SwitchProControllerHID or Joystick => NintendoDeviceID,
            _ => DefaultGamepadControlsID,
        };
    }
    #endregion
}
#pragma warning enable UDR0005