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
        
        #region ***---SoundID---***
        private const string ButtonDownMasterBus_SoundID = "UI_ButtonDown[MasterBus]";
        private const string ButtonUpMasterBus_SoundID = "UI_ButtonUp[MasterBus]";
        private const string ButtonDownMusicBus_SoundID = "UI_ButtonDown[MusicBus]";
        private const string ButtonUpMusicBus_SoundID = "UI_ButtonUp[MusicBus]";
        private const string ButtonDownSFXBus_SoundID = "UI_ButtonDown[SFXBus]";
        private const string ButtonUpSFXBus_SoundID = "UI_ButtonUp[SFXBus]";
        #endregion
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

            _masterVolumeSlider.RegisterValueChangedCallback(evt => {
                _audioManager.SetMasterVolume(evt.newValue);

                _audioManager.PlayOneShot(evt.newValue > evt.previousValue
                    ? ButtonUpMasterBus_SoundID
                    : ButtonDownMasterBus_SoundID);
            });

            _sfxVolumeSlider.RegisterValueChangedCallback(evt => {
                _audioManager.SetSFXVolume(evt.newValue);

                _audioManager.PlayOneShot(evt.newValue > evt.previousValue
                    ? ButtonUpSFXBus_SoundID
                    : ButtonDownSFXBus_SoundID);
            });

            _musicVolumeSlider.RegisterValueChangedCallback(evt => {
                _audioManager.SetMusicVolume(evt.newValue);

                _audioManager.PlayOneShot(evt.newValue > evt.previousValue
                    ? ButtonUpMusicBus_SoundID
                    : ButtonDownMusicBus_SoundID);
            });
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