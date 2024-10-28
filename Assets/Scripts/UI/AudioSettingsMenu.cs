using FMODScripts;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI {
    public class AudioSettingsMenu : MonoBehaviour {
        [Header("Settings UI Document")] 
        [SerializeField] private UIDocument uiDocument;

        private static FMODManager FMODManager => FMODManager.Instance;

        private void Start() {
            var audioTab = uiDocument.rootVisualElement.Q<Tab>("AudioTab");
            audioTab.dataSource = FMODManager;

            var masterVolumeSlider = uiDocument.rootVisualElement.Q<Slider>("MasterVolumeSlider");
            var sfxVolumeSlider = uiDocument.rootVisualElement.Q<Slider>("SFXVolumeSlider");
            var musicVolumeSlider = uiDocument.rootVisualElement.Q<Slider>("MusicVolumeSlider");

            masterVolumeSlider.value = FMODManager.MasterVolume;
            sfxVolumeSlider.value = FMODManager.SfxVolume;
            musicVolumeSlider.value = FMODManager.MusicVolume;

            masterVolumeSlider.RegisterValueChangedCallback(evt => FMODManager.SetMasterVolume(evt.newValue));
            sfxVolumeSlider.RegisterValueChangedCallback(evt => FMODManager.SetSfxVolume(evt.newValue));
            musicVolumeSlider.RegisterValueChangedCallback(evt => FMODManager.SetMusicVolume(evt.newValue));
        }
    }
}