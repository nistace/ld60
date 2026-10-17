using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using LD60.Inputs;
using LD60.Levels;
using LD60.Objectives;
using UnityEngine;

namespace LD60.Games
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private LevelsLibrary _levelsLibrary;
        [SerializeField] private Camera _camera;
        [SerializeField] private ObjectiveManager _objectiveManager;
        [SerializeField] private CommandExecutor _executor;
        [SerializeField] private InputParser _inputParser;
        [SerializeField] private int _levelIndex;

        private Level _currentLevelInstance;
        public bool HasCurrentLevel => _currentLevelInstance;

        private CancellationTokenSource _resultCancellationTokenSource;

        private void Start() => StartCurrentLevel();

        private void OnDestroy() => EndCurrentLevel(false);

        private void EndCurrentLevel(bool destroyInstance = true)
        {
            _resultCancellationTokenSource?.Cancel();
            _resultCancellationTokenSource?.Dispose();
            _resultCancellationTokenSource = null;

            if (_currentLevelInstance)
            {
                _currentLevelInstance.Succeeded -= HandleCurrentLevelSucceeded;
                _currentLevelInstance.Failed -= HandleCurrentLevelFailed;

                if (destroyInstance)
                {
                    Destroy(_currentLevelInstance.gameObject);
                }

                _currentLevelInstance = null;
            }
        }

        public void RestartCurrentLevel()
        {
            EndCurrentLevel();

            StartCurrentLevel();
        }

        private void StartNextLevel()
        {
            EndCurrentLevel();

            _levelIndex++;

            StartCurrentLevel();
        }

        private void StartCurrentLevel()
        {
            _executor.Clear();
            _inputParser.Clear();
            _inputParser.CurrentInputChanged -= RecordStartTime;
            _inputParser.CurrentInputChanged += RecordStartTime;

            _currentLevelInstance = Instantiate(_levelsLibrary[_levelIndex], Vector3.zero, Quaternion.identity);
            _currentLevelInstance.Succeeded += HandleCurrentLevelSucceeded;
            _currentLevelInstance.Failed += HandleCurrentLevelFailed;
            _objectiveManager.CurrentObjectiveHolder = _currentLevelInstance;

            _camera.orthographicSize = _currentLevelInstance.OrthographicSize;
        }

        private void RecordStartTime(string _)
        {
            _inputParser.CurrentInputChanged -= RecordStartTime;

            if (!_currentLevelInstance) return;

            _currentLevelInstance.FirstInputTime = Time.time;
        }

        private void HandleCurrentLevelFailed(ELevelStatus status)
        {
            _resultCancellationTokenSource?.Cancel();
            _resultCancellationTokenSource?.Dispose();
            _resultCancellationTokenSource = new CancellationTokenSource();

            RunResultAsync(RestartCurrentLevel, _resultCancellationTokenSource.Token).Forget();
        }

        private void HandleCurrentLevelSucceeded()
        {
            _resultCancellationTokenSource?.Cancel();
            _resultCancellationTokenSource?.Dispose();
            _resultCancellationTokenSource = new CancellationTokenSource();

            RunResultAsync(StartNextLevel, _resultCancellationTokenSource.Token).Forget();
        }

        private async UniTask RunResultAsync(Action callback, CancellationToken cancellationToken)
        {
            if (_currentLevelInstance)
            {
                _currentLevelInstance.EndTime = Time.time;

                var resultText = _currentLevelInstance.Status switch
                {
                    ELevelStatus.Incomplete => "???",
                    ELevelStatus.Complete => "SUCCESS",
                    ELevelStatus.WrongExit => "FAILED: ONE TRAIN LEFT AT THE WRONG EXIT",
                    ELevelStatus.SkippedStops => "FAILED: ONE TRAIN LEFT BEFORE STOPPING IN STATIONS",
                    _ => throw new ArgumentOutOfRangeException()
                };

                _currentLevelInstance.SetResultText(resultText);

                await UniTask.WaitForSeconds(2, cancellationToken: cancellationToken);

                if (_currentLevelInstance.Status == ELevelStatus.Complete)
                {
                    _currentLevelInstance.SetResultText(resultText + "<br>" + "Time: " + FormatLevelDuration(_currentLevelInstance.Duration));

                    await UniTask.WaitForSeconds(2, cancellationToken: cancellationToken);
                }
            }

            callback?.Invoke();
        }

        private static string FormatLevelDuration(float duration)
        {
            if (duration > 60)
            {
                return $"{(int)duration / 60:00}:{duration % 60:00.00}";
            }

            return $"{duration:00.00} seconds";
        }
    }
}