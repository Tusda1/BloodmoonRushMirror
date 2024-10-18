using Fmod;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI {
    public class AudioSettingsMenu : MonoBehaviour {
        [Header("Settings UI Document")] 
        [SerializeField] private UIDocument uiDocument;

        private FmodManager fmodManager => FmodManager.Instance;

        private void Start() {
            var audioTab = uiDocument.rootVisualElement.Q<Tab>("AudioTab");
            audioTab.dataSource = fmodManager;

            var masterVolumeSlider = uiDocument.rootVisualElement.Q<Slider>("MasterVolumeSlider");
            var sfxVolumeSlider = uiDocument.rootVisualElement.Q<Slider>("SFXVolumeSlider");
            var musicVolumeSlider = uiDocument.rootVisualElement.Q<Slider>("MusicVolumeSlider");

            masterVolumeSlider.value = FmodManager.masterVolume;
            sfxVolumeSlider.value = FmodManager.sfxVolume;
            musicVolumeSlider.value = FmodManager.musicVolume;

            masterVolumeSlider.RegisterValueChangedCallback(evt => fmodManager.SetMasterVolume(evt.newValue));
            sfxVolumeSlider.RegisterValueChangedCallback(evt => fmodManager.SetSfxVolume(evt.newValue));
            musicVolumeSlider.RegisterValueChangedCallback(evt => fmodManager.SetMusicVolume(evt.newValue));
        }
    }
}