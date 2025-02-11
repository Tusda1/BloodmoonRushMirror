using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using static ControllerRecognizer;
using static IControlSchemeChanger;

public class ControlSchemeChanger : IControlSchemeChanger {
    #region ---Fields---
    public bool ControlSchemeIsController { get; private set; }

    public ControlSchemeData CurrentControlSchemeInfo { get; private set; }

    public event Action<ControlSchemeData> OnControlSchemeChanged = delegate { };

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
    #endregion

    #region ***---SchemeID---***
    private string _currentGamepadSchemeID;
    private string _currentKeyboardSchemeID;
    
    private const string KeyboardSchemeID = "KEYBOARD";
    private const string KeyboardMouseSchemeID = "KEYBOARDMOUSE";
    
    private const string DefaultKeyboardSchemeID = KeyboardMouseSchemeID;
    #endregion

    private const string PlayerPrefsKeyboardSchemeIDKey = "KeyboardSchemeID";
    private const string PlayerPrefsGamepadSchemeIDKey = "GamepadSchemeID";
    #endregion

    #region ---Construction---
    [Inject]
    public ControlSchemeChanger(ControllerRecognizer controllerRecognizer) {
        _controllerRecognizer = controllerRecognizer;
        
        _currentKeyboardSchemeID = PlayerPrefs.GetString(PlayerPrefsKeyboardSchemeIDKey, DefaultKeyboardSchemeID);
        _currentGamepadSchemeID = PlayerPrefs.GetString(PlayerPrefsGamepadSchemeIDKey, DefaultGamepadControlsID);

        InitializeInputLocalizationDatas();

        _controllerRecognizer.OnDeviceChanged += OnControllerDeviceChanged;
        _controllerRecognizer.KeyboardInputDetected += KeyboardInputDetected;
        _controllerRecognizer.ControllerInputDetected += ControllerInputDetected;

        _currentControllerDevice = _controllerRecognizer.CurrentDevice;

        if (_currentControllerDevice is Gamepad or Joystick) {
            _currentGamepadSchemeID = _controllerRecognizer.CurrentDeviceID;
            ChangeSchemeByID(_currentGamepadSchemeID);
        }
        else {
            ChangeSchemeByID(_currentKeyboardSchemeID);
        }
    }

    ~ControlSchemeChanger() {
        _controllerRecognizer.OnDeviceChanged -= OnControllerDeviceChanged;
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
                PlayerPrefs.SetString(PlayerPrefsGamepadSchemeIDKey, _currentGamepadSchemeID);
                currentDevices.Add(_currentControllerDevice);
                ControlSchemeIsController = true;
            }
            else if (_currentControlSchemeName is KeyboardScheme or KeyboardMouseScheme)  {
                _currentKeyboardSchemeID = _currentControlSchemeID;
                PlayerPrefs.SetString(PlayerPrefsKeyboardSchemeIDKey, _currentKeyboardSchemeID);
                currentDevices.AddRange(InputSystem.devices.Where(x => x is Keyboard or Mouse));
                ControlSchemeIsController = false;
            }
            else {
                throw new ArgumentOutOfRangeException(_currentControlSchemeName, "Invalid control scheme name");
            }

            CurrentControlSchemeInfo = new ControlSchemeData(_currentControlSchemeDataSO, currentDevices.ToArray());

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

    private void OnControllerDeviceChanged(string controllerID, InputDevice inputDevice) {
        _currentGamepadSchemeID = controllerID;
        _currentControllerDevice = inputDevice;
    }

    private void KeyboardInputDetected(InputDevice inputDevice) {
        ChangeSchemeByID(_currentKeyboardSchemeID);
    }

    private void ControllerInputDetected(InputDevice inputDevice) {
        ChangeSchemeByID(_currentGamepadSchemeID);
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

    private static string IDToName(string id) {
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