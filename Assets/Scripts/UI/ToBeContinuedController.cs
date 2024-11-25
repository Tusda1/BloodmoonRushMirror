using System.Collections;
using Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace UI {
    public class ToBeContinuedController : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
        
        private VisualElement _root;
        #endregion

        private IEnumerator Start() {
            _root = uiDocument.rootVisualElement;

            var exitButton = _root.Q<Button>("ExitButton");
            exitButton.clicked += () => { SceneManager.LoadScene("MainMenu"); };

            var fadeIn = _root.Q<VisualElement>("FadeIn");
            yield return new WaitForSeconds(1);
            fadeIn.RemoveFromClassList("opacity0");
        }
    }
}