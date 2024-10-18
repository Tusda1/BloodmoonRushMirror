using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace UI {
    public class ToBeContinuedController : MonoBehaviour {
        [SerializeField] private UIDocument uiDocument;

        private IEnumerator Start() {

            var root = uiDocument.rootVisualElement;

            var exitButton = root.Q<Button>("ExitButton");
            exitButton.clicked += () => { SceneManager.LoadScene("MainMenu"); };

            var fadeIn = root.Q<VisualElement>("FadeIn");
            yield return new WaitForSeconds(1);
            fadeIn.RemoveFromClassList("opacity0");
        }
    }
}