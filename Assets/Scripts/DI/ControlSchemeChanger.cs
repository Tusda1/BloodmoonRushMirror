using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using static ControllerRecognizer;

public class ControlSchemeChanger : IControlSchemeChanger {
    #region ---Fields---
    public IControlSchemeChanger.ControlSchemeData CurrentControlSchemeInfo { get; private set; }

    public event Action<IControlSchemeChanger.ControlSchemeData> OnControlSchemeChanged = delegate { };

    private readonly ControllerRecognizer _controllerRecognizer;
    private readonly Dictionary<string, ControlSchemeDataSO> _inputLocalizationDatas = new();

    private InputDevice _currentControllerDevice;

    #region ***---PropertyBackingFields---***
    private string _currentControlSchemeName;
    private string _currentControlSchemeID;
    private ControlSchemeDataSO _currentControlSchemeDataSO;
    #endregion

    #region ***---SchemeNames---***
    private const string GamepadScheme = "Gamepad";
    private const string JoystickScheme = "Joystick";
    private const string KeyboardScheme = "Keyboard";
    private const string KeyboardMouseScheme = "KeyboardMouse";
    
    private const string DefaultScheme = KeyboardScheme;
    #endregion

    #region ***---SchemeID---***
    private string _currentGamepadSchemeID;
    private const string KeyboardSchemeID = "KEYBOARD";
    private const string KeyboardMouseSchemeID = "KEYBOARDMOUSE";
    #endregion
    #endregion

    #region ---Construction---
    public ControlSchemeChanger(ControllerRecognizer controllerRecognizer) {
        _controllerRecognizer = controllerRecognizer;

        InitializeInputLocalizationDatas();

        _controllerRecognizer.OnDeviceChanged += OnNewController;

        _currentControllerDevice = _controllerRecognizer.CurrentDevice;

        if (_currentControllerDevice != null) {
            ChangeSchemeByID(_controllerRecognizer.CurrentDeviceID);
        }
        else {
            ChangeSchemeByName(DefaultScheme);
        }
    }

    ~ControlSchemeChanger() {
        _controllerRecognizer.OnDeviceChanged -= OnNewController;
    }
    #endregion

    #region ---PublicMethods---
    public void ChangeSchemeByName(string scheme) => CurrentControlSchemeName = scheme;
    
    public void ChangeSchemeByID(string id) => CurrentControlSchemeID = id;
    #endregion
    
    #region ---Properties---
    public string CurrentControlSchemeName {
        get => _currentControlSchemeName;
        set {
            _currentControlSchemeName = value;
            _currentControlSchemeID = NameToID(CurrentControlSchemeName);

            if (_currentControlSchemeID == null) return;
            
            CurrentControlSchemeDataSO = _inputLocalizationDatas[CurrentControlSchemeID];
        }
    }

    public string CurrentControlSchemeID {
        get => _currentControlSchemeID;
        set {
            _currentControlSchemeID = value;
            _currentControlSchemeName = IDToName(CurrentControlSchemeID);
            
            CurrentControlSchemeDataSO = _inputLocalizationDatas[CurrentControlSchemeID];
        }
    }

    public ControlSchemeDataSO CurrentControlSchemeDataSO {
        get => _currentControlSchemeDataSO;
        private set {
            _currentControlSchemeDataSO = value;

            var currentDevices = new List<InputDevice>();

            if (_currentControlSchemeName is GamepadScheme or JoystickScheme) {
                currentDevices.Add(_currentControllerDevice);
            }
            else {
                currentDevices.AddRange(InputSystem.devices.Where(x => x is Keyboard or Mouse));
            }

            CurrentControlSchemeInfo = new IControlSchemeChanger.ControlSchemeData(_currentControlSchemeDataSO, currentDevices.ToArray());

            OnControlSchemeChanged.Invoke(CurrentControlSchemeInfo);

            PlayerPrefs.SetString("ControlScheme", _currentControlSchemeName);
        }
    }
    #endregion

    #region ---PrivateMethods---
    private void InitializeInputLocalizationDatas() {
        var data = ScrubUtils.GetAllScrubsInResourceFolder<ControlSchemeDataSO>(ControlSchemeDataSO.FolderName);

        foreach (var localizationData in data) {
            _inputLocalizationDatas.Add(localizationData.id, localizationData);
        }
    }

    private void OnNewController(string controllerID, InputDevice inputDevice) {
        _currentGamepadSchemeID = controllerID;
        _currentControllerDevice = inputDevice;

        if (CurrentControlSchemeName is GamepadScheme or JoystickScheme) {
            ChangeSchemeByID(_currentGamepadSchemeID);
        }
    }

    private string NameToID(string name) {
        return name switch {
            KeyboardScheme => KeyboardSchemeID,
            KeyboardMouseScheme => KeyboardMouseSchemeID,
            GamepadScheme => _currentGamepadSchemeID,
            JoystickScheme => NintendoDeviceID,
            _ => null
        };
    }

    private string IDToName(string id) {
        return id switch {
            KeyboardSchemeID => KeyboardScheme,
            KeyboardMouseSchemeID => KeyboardMouseScheme,
            XboxDeviceID => GamepadScheme,
            PlaystationDeviceID => GamepadScheme,
            NintendoDeviceID => JoystickScheme,
            _ => throw new ArgumentOutOfRangeException(nameof(id))
        };
    }
    #endregion
}