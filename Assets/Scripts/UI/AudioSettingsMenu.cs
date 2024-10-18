using UnityEngine;
using UnityEngine.UIElements;

namespace UI {
    public class AudioSettingsMenu : MonoBehaviour {
        [Header("Settings UI Document")] [SerializeField]
        private UIDocument uiDocument;

        private void Start() {
            var audioTab = uiDocument.rootVisualElement.Q<Tab>("AudioTab");
            audioTab.dataSource = FmodManager.Instance;
        }
    }
}