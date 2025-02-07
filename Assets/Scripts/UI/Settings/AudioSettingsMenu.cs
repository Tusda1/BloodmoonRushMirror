using System;
using Attributes;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace UI.Settings {
    public class AudioSettingsMenu : MonoBehaviour, IMenu {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector]
        private UIDocument uiDocument;

        #region ---PrivateFields---
        private VisualElement _root;
        private VisualElement _audioSettingsMenu;
        private Slider _masterVolumeSlider;
        private Slider _sfxVolumeSlider;
        private Slider _musicVolumeSlider;
        #endregion

        public event Action OnMenuActivated = delegate { };
        public event Action OnMenuDeactivated = delegate { };

        [Inject] private IAudioManager _audioManager;
        #endregion

        private void Awake() {
            _root = uiDocument.rootVisualElement;
            _audioSettingsMenu = _root.Q<VisualElement>("AudioSettingsMenu");

            _masterVolumeSlider = _audioSettingsMenu.Q<Slider>("MasterVolumeSlider");
            _sfxVolumeSlider = _audioSettingsMenu.Q<Slider>("SFXVolumeSlider");
            _musicVolumeSlider = _audioSettingsMenu.Q<Slider>("MusicVolumeSlider");
        }

        private void Start() {
            _masterVolumeSlider.value = _audioManager.MasterVolume;
            _sfxVolumeSlider.value = _audioManager.SFXVolume;
            _musicVolumeSlider.value = _audioManager.MusicVolume;

            _masterVolumeSlider.RegisterValueChangedCallback(evt => _audioManager.SetMasterVolume(evt.newValue));

            _sfxVolumeSlider.RegisterValueChangedCallback(evt => _audioManager.SetSFXVolume(evt.newValue));

            _musicVolumeSlider.RegisterValueChangedCallback(evt => _audioManager.SetMusicVolume(evt.newValue));
        }

        public void Activate() {
            _audioSettingsMenu.SetEnabled(true);
            _audioSettingsMenu.RemoveFromClassList("left");
            _audioSettingsMenu.RemoveFromClassList("right");
            _masterVolumeSlider.Focus();

            OnMenuActivated.Invoke();
        }

        public void Deactivate(string addToClassList) {
            _audioSettingsMenu.SetEnabled(false);

            if (addToClassList != null) {
                _audioSettingsMenu.AddToClassList(addToClassList);
            }

            OnMenuDeactivated.Invoke();
        }
    }
}