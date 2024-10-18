using UnityEngine;

namespace Utilities.UI.DisplaySettings {
    [CreateAssetMenu(fileName = "FullScreenModeScrub", menuName = "Scrubs/SettingsMenu/FullScreenModeScrub")]
    public class FullScreenModeScrub : ScriptableObject {
        [SerializeField] public string[] screenModes = {
            "FullScreen",
            "Borderless Window",
            "Maximized Window",
            "Windowed"
        };

        public FullScreenMode GetFullScreenMode(int value) {
            return value switch {
                0 => FullScreenMode.ExclusiveFullScreen,
                1 => FullScreenMode.FullScreenWindow,
                2 => FullScreenMode.MaximizedWindow,
                3 => FullScreenMode.Windowed,
                _ => Screen.fullScreenMode
            };
        }
    }
}