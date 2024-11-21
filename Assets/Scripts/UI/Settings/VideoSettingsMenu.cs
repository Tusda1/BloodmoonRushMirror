using Attributes;
using Consensus;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities.UI.DisplaySettings;

namespace UI.Settings {
    public class VideoSettingsMenu : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField] private UIDocument _settingsUiDocument;

        [Header("Display Settings Data")]
        [SerializeField, Required] private FullScreenModeSO fullScreenModeSo;
        [SerializeField, Required] private AspectRatioResolutionsSO aspectRatioResolutionsSo;

        [Header("Scrub Global Data")] 
        [SerializeField, Required] private AtLeastOneInFavourConsensusSO hasMadeChangeThatRequiresRestart;

        [Header("Debug Logger")] 
        [SerializeField, Required] private DebugLoggerSO debugLogger;

        private DropdownField _screenModeDropdownMenu;
        private DropdownField _displayDropdownMenu;
        private DropdownField _aspectRatioDropdownMenu;
        private DropdownField _resolutionDropdownMenu;
        private Toggle _vsyncToggle;
        private SliderInt _fpsLimitSlider;

        private readonly ScriptVotePair _displaySettingChanged = new();

        private int _currentDisplay;
        #endregion

        #region ---EventFunctions---
        private void Awake() {
            debugLogger.messageOrigin = this;

            var settingsRoot = _settingsUiDocument.rootVisualElement;

            _screenModeDropdownMenu = settingsRoot.Q<DropdownField>("ScreenModeSelectDropdown");
            _displayDropdownMenu = settingsRoot.Q<DropdownField>("DisplaySelectDropdown");
            _aspectRatioDropdownMenu = settingsRoot.Q<DropdownField>("AspectRatioSelectDropdown");
            _resolutionDropdownMenu = settingsRoot.Q<DropdownField>("ResolutionSelectDropdown");
            _vsyncToggle = settingsRoot.Q<Toggle>("VsyncToggle");
            _fpsLimitSlider = settingsRoot.Q<SliderInt>("FPSLimiterSlider");

            if (_screenModeDropdownMenu != null) {
                _screenModeDropdownMenu.choices.Clear();
                foreach (var screenMode in fullScreenModeSo.screenModes) {
                    _screenModeDropdownMenu.choices.Add(screenMode);
                }

                _screenModeDropdownMenu.RegisterValueChangedCallback(FullScreenModeChange);

                var fullScreenMode = PlayerPrefs.GetInt("FullScreenMode");
                _screenModeDropdownMenu.value = fullScreenModeSo.screenModes[fullScreenMode];
            }
            else debugLogger.LogWarning("'_screenModeDropdownMenu' is not set");

            if (_displayDropdownMenu != null) {
                _displayDropdownMenu.choices.Clear();
                for (var i = 0; i < Display.displays.Length; i++) {
                    _displayDropdownMenu.choices.Add("Display " + i);
                }

                var display = PlayerPrefs.GetInt("UnitySelectMonitor");
                _displayDropdownMenu.value = "Display " + display;
                _currentDisplay = display;

                _displayDropdownMenu.RegisterValueChangedCallback(DisplayChange);
            }
            else debugLogger.LogWarning("'_displayDropdownMenu' is not set");

            if (_aspectRatioDropdownMenu != null && _resolutionDropdownMenu != null) {
                _aspectRatioDropdownMenu.choices.Clear();
                _resolutionDropdownMenu.choices.Clear();
                foreach (var key in aspectRatioResolutionsSo.aspectRatioResolutions.Keys) {
                    _aspectRatioDropdownMenu.choices.Add(key);
                }

                _aspectRatioDropdownMenu.RegisterValueChangedCallback(AspectRatioChanged);

                var resolution = PlayerPrefs.GetString("ScreenResolution");
                var screenResolution = resolution != ""
                    ? AspectRatioResolutionsSO.StringToVector2Int(resolution)
                    : new Vector2Int(Screen.width, Screen.height);

                var screenAspectRatio = aspectRatioResolutionsSo.FindKeyByValue(screenResolution);

                _aspectRatioDropdownMenu.value = screenAspectRatio;
                _resolutionDropdownMenu.value = AspectRatioResolutionsSO.ResolutionToString(screenResolution);

                _resolutionDropdownMenu.RegisterValueChangedCallback(ResolutionChanged);
            }
            else debugLogger.LogWarning("'_aspectRatioDropdownMenu' and '_resolutionDropdownMenu' is not set");

            if (_vsyncToggle != null) {
                _vsyncToggle.RegisterValueChangedCallback(VsyncToggleChanged);

                _vsyncToggle.value = PlayerPrefs.GetInt("VsyncLevel", 0) == 1;
            }
            else debugLogger.LogWarning("'_vsyncToggle' is not set");

            if (_fpsLimitSlider != null) {
                _fpsLimitSlider.RegisterValueChangedCallback(FPSLimitChanged);

                var refreshRate = (int)Screen.currentResolution.refreshRateRatio.value;
                var targetFPS = PlayerPrefs.GetInt("TargetFrameRate", refreshRate);
                _fpsLimitSlider.value = targetFPS;
            }
            else debugLogger.LogWarning("'_fpsLimitSlider' is not set");

            if (hasMadeChangeThatRequiresRestart != null) {
                hasMadeChangeThatRequiresRestart.OnVoteChange += OnHasMadeChangeThatRequiresRestartValueChanged;
            }
            else debugLogger.LogWarning("'hasMadeChangeThatRequiresRestartConsensus' is not set");
        }

        private void Start() {
            hasMadeChangeThatRequiresRestart.consensus = false;

            debugLogger.Log("Settings menu initialized");
        }

        private void OnDisable() {
            if (hasMadeChangeThatRequiresRestart != null) {
                hasMadeChangeThatRequiresRestart.OnVoteChange -=
                    OnHasMadeChangeThatRequiresRestartValueChanged;
            }

            hasMadeChangeThatRequiresRestart.DeleteVote(_displaySettingChanged);

            debugLogger.Log("Disposing and clearing delegates and scrubs");
        }
        #endregion

        #region ---EventHandlers---
        private void OnHasMadeChangeThatRequiresRestartValueChanged(bool value) {
            debugLogger.Log("Has made change that requires restart: " + value);
        }

        private void FullScreenModeChange(ChangeEvent<string> changeEvent) {
            var value = _screenModeDropdownMenu.choices.IndexOf(changeEvent.newValue);
            Screen.fullScreenMode = FullScreenModeSO.GetFullScreenMode(value);

            PlayerPrefs.SetInt("FullScreenMode", value);
            PlayerPrefs.Save();

            debugLogger.Log("ScreenMode changed to: " + value);
        }

        private void DisplayChange(ChangeEvent<string> changeEvent) {
            var value = _displayDropdownMenu.choices.IndexOf(changeEvent.newValue);
            PlayerPrefs.SetInt("UnitySelectMonitor", value);
            PlayerPrefs.Save();

            hasMadeChangeThatRequiresRestart.ChangeVote(_displaySettingChanged, value != _currentDisplay);

            debugLogger.Log("Display changed to: " + value);
        }

        private void AspectRatioChanged(ChangeEvent<string> changeEvent) {
            _resolutionDropdownMenu.choices.Clear();
            foreach (var resolutions in aspectRatioResolutionsSo.GetResolutionForAspectRatio(changeEvent.newValue)) {
                _resolutionDropdownMenu.choices.Add(AspectRatioResolutionsSO.ResolutionToString(resolutions));
            }

            debugLogger.Log("Aspect Ratio changed");
        }

        private void ResolutionChanged(ChangeEvent<string> changeEvent) {
            var resolution = AspectRatioResolutionsSO.StringToVector2Int(changeEvent.newValue);
            Screen.SetResolution(resolution.x, resolution.y, Screen.fullScreenMode);
            PlayerPrefs.SetString("ScreenResolution", changeEvent.newValue);

            debugLogger.Log("Resolution changed to: " + changeEvent.newValue);
        }

        private void VsyncToggleChanged(ChangeEvent<bool> evt) {
            var vsyncLevel = evt.newValue ? 1 : 0;
            PlayerPrefs.SetInt("VsyncLevel", vsyncLevel);
            QualitySettings.vSyncCount = vsyncLevel;

            debugLogger.Log("Vsync Level changed to " + vsyncLevel);
        }

        private void FPSLimitChanged(ChangeEvent<int> changeEvent) {
            Application.targetFrameRate = changeEvent.newValue;
            PlayerPrefs.SetInt("TargetFrameRate", changeEvent.newValue);

            debugLogger.Log("***FPS Limit changed to " + changeEvent.newValue);
        }
        #endregion
    }
}