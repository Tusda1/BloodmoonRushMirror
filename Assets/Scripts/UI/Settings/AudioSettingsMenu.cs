using Attributes;
using FMODScripts;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Settings {
    public class AudioSettingsMenu : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;

        private Tab _audioTab;
        private Slider _masterVolumeSlider;
        private Slider _sfxVolumeSlider;
        private Slider _musicVolumeSlider;

        private static FMODManager FMODManager => FMODManager.Instance;
        #endregion

        private void Awake() {
            _audioTab = uiDocument.rootVisualElement.Q<Tab>("AudioTab");

            _masterVolumeSlider = uiDocument.rootVisualElement.Q<Slider>("MasterVolumeSlider");
            _sfxVolumeSlider = uiDocument.rootVisualElement.Q<Slider>("SFXVolumeSlider");
            _musicVolumeSlider = uiDocument.rootVisualElement.Q<Slider>("MusicVolumeSlider");
        }

        private void Start() {
            _audioTab.dataSource = FMODManager;

            _masterVolumeSlider.value = FMODManager.MasterVolume;
            _sfxVolumeSlider.value = FMODManager.SfxVolume;
            _musicVolumeSlider.value = FMODManager.MusicVolume;

            _masterVolumeSlider.RegisterValueChangedCallback(evt => FMODManager.SetMasterVolume(evt.newValue));
            _sfxVolumeSlider.RegisterValueChangedCallback(evt => FMODManager.SetSfxVolume(evt.newValue));
            _musicVolumeSlider.RegisterValueChangedCallback(evt => FMODManager.SetMusicVolume(evt.newValue));
        }
    }
}