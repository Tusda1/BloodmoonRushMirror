using Attributes;
using FMODScripts;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Settings {
    public class AudioSettingsMenu : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector]
        private UIDocument uiDocument;

        private Slider _masterVolumeSlider;
        private Slider _sfxVolumeSlider;
        private Slider _musicVolumeSlider;
        #endregion

        private void Awake() {
            _masterVolumeSlider = uiDocument.rootVisualElement.Q<Slider>("MasterVolumeSlider");
            _sfxVolumeSlider = uiDocument.rootVisualElement.Q<Slider>("SFXVolumeSlider");
            _musicVolumeSlider = uiDocument.rootVisualElement.Q<Slider>("MusicVolumeSlider");
        }

        private void Start() {
            _masterVolumeSlider.value = FMODManager.MasterVolume;
            _sfxVolumeSlider.value = FMODManager.SfxVolume;
            _musicVolumeSlider.value = FMODManager.MusicVolume;

            _masterVolumeSlider.RegisterValueChangedCallback(evt =>
                FMODManager.RaiseSetVolumeEvent(VolumeChanel.Master, evt.newValue));
            
            _sfxVolumeSlider.RegisterValueChangedCallback(evt =>
                FMODManager.RaiseSetVolumeEvent(VolumeChanel.SFX, evt.newValue));
            
            _musicVolumeSlider.RegisterValueChangedCallback(evt =>
                FMODManager.RaiseSetVolumeEvent(VolumeChanel.Music, evt.newValue));
        }
    }
}