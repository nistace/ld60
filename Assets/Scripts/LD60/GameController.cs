using LD60.Inputs;
using LD60.Levels;
using LD60.Objectives;
using UnityEngine;

namespace LD60
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private LevelsLibrary _levelsLibrary;
        [SerializeField] private ObjectiveManager _objectiveManager;
        [SerializeField] private CommandExecutor _executor;
        [SerializeField] private InputParser _inputParser;
        [SerializeField] private int _levelIndex;

        private Level _currentLevelInstance;

        private void Start() => StartCurrentLevel();

        private void OnDestroy()
        {
            if (!_currentLevelInstance) return;

            _currentLevelInstance.Succeeded -= HandleCurrentLevelSucceeded;
            _currentLevelInstance.Failed -= HandleCurrentLevelFailed;
        }

        private void DestroyCurrentLevel()
        {
            if (!_currentLevelInstance) return;

            _currentLevelInstance.Succeeded -= HandleCurrentLevelSucceeded;
            _currentLevelInstance.Failed -= HandleCurrentLevelFailed;

            Destroy(_currentLevelInstance.gameObject);
            _currentLevelInstance = null;
        }

        private void RestartCurrentLevel()
        {
            DestroyCurrentLevel();

            StartCurrentLevel();
        }

        private void StartNextLevel()
        {
            DestroyCurrentLevel();

            _levelIndex++;

            StartCurrentLevel();
        }

        private void StartCurrentLevel()
        {
            _executor.Clear();
            _inputParser.Clear();

            _currentLevelInstance = Instantiate(_levelsLibrary[_levelIndex], Vector3.zero, Quaternion.identity);
            _currentLevelInstance.Succeeded += HandleCurrentLevelSucceeded;
            _currentLevelInstance.Failed += HandleCurrentLevelFailed;
            _objectiveManager.CurrentObjectiveHolder = _currentLevelInstance;
        }

        private void HandleCurrentLevelFailed(ELevelFailedReason reason)
        {
            Debug.LogWarning($"Failed: {reason}");
            RestartCurrentLevel();
        }

        private void HandleCurrentLevelSucceeded() => StartNextLevel();
    }
}