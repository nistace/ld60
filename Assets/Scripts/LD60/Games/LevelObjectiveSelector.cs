using Nrjwolf.Tools.AttachAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LD60.Games
{
    [RequireComponent(typeof(GameController))]
    public class LevelObjectiveSelector : MonoBehaviour
    {
        [SerializeField, GetComponent] private GameController _gameController;
        [SerializeField] private InputActionReference _navigateUpActionReference;
        [SerializeField] private InputActionReference _navigateDownActionReference;
        [SerializeField] private InputActionReference _unselectActionReference;

        private void OnEnable()
        {
            _navigateDownActionReference.action.performed += HandleNavigationDown;
            _navigateUpActionReference.action.performed += HandleNavigationUp;
            _unselectActionReference.action.performed += HandleUnselect;
        }

        private void HandleUnselect(InputAction.CallbackContext obj)
        {
            if (!_gameController.CurrentLevelInstance) return;

            _gameController.CurrentLevelInstance.UnselectObjective();
        }

        private void HandleNavigationUp(InputAction.CallbackContext obj)
        {
            if (!_gameController.CurrentLevelInstance) return;
            _gameController.CurrentLevelInstance.SelectPreviousObjective();
        }

        private void HandleNavigationDown(InputAction.CallbackContext obj)
        {
            if (!_gameController.CurrentLevelInstance) return;
            _gameController.CurrentLevelInstance.SelectNextObjective();
        }
    }
}