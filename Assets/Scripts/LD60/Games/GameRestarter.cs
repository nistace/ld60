using System;
using Nrjwolf.Tools.AttachAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LD60.Games
{
    [RequireComponent(typeof(GameController))]
    public class GameRestarter : MonoBehaviour
    {
        [SerializeField, GetComponent] private GameController _gameController;
        [SerializeField] private InputActionReference _restartReference;

        private void OnEnable()
        {
            _restartReference.action.performed += HandleActionPerformed;
        }

        private void OnDisable()
        {
            _restartReference.action.performed -= HandleActionPerformed;
        }

        private void HandleActionPerformed(InputAction.CallbackContext context)
        {
            if (!_gameController.CurrentLevelInstance) return;

            _gameController.RestartCurrentLevel();
        }
    }
}