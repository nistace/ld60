using UnityEngine;
using UnityEngine.InputSystem;

namespace LD60.Games
{
    public class QuitHandler : MonoBehaviour
    {
        [SerializeField] private InputActionReference _quitAction;

        private void OnEnable() => _quitAction.action.performed += HandleQuitPerformed;
        private void OnDisable() => _quitAction.action.performed -= HandleQuitPerformed;

        private static void HandleQuitPerformed(InputAction.CallbackContext obj) => Application.Quit();
    }
}