using Attributes;
using FMODScripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace UI {
    public class DeathScreenController : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
        
        private static FMODManager FMODManager => FMODManager.Instance;
        #endregion
        
        private void Start() {
            // TODO
            FMODManager.StopMusic();

            var root = uiDocument.rootVisualElement;
            
            var yesButton = root.Q<Button>("YesButton");
            yesButton.clicked += () => { SceneManager.LoadScene("Level 1 test"); };

            var noButton = root.Q<Button>("NoButton");
            noButton.clicked += () => { SceneManager.LoadScene("MainMenu"); };
        }
    }
}