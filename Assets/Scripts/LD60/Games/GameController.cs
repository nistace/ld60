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

        public Level CurrentLevelInstance { get; private set; }

        private CancellationTokenSource _resultCancellationTokenSource;

        private void Start() => StartCurrentLevel();

        private void OnDestroy() => EndCurrentLevel(false);

        private void EndCurrentLevel(bool destroyInstance = true)
        {
            _resultCancellationTokenSource?.Cancel();
            _resultCancellationTokenSource?.Dispose();
            _resultCancellationTokenSource = null;

            if (CurrentLevelInstance)
            {
                CurrentLevelInstance.Succeeded -= HandleCurrentLevelSucceeded;
                CurrentLevelInstance.Failed -= HandleCurrentLevelFailed;

                if (destroyInstance)
                {
                    Destroy(CurrentLevelInstance.gameObject);
                }

                CurrentLevelInstance = null;
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

            CurrentLevelInstance = Instantiate(_levelsLibrary[_levelIndex], Vector3.zero, Quaternion.identity);
            CurrentLevelInstance.Succeeded += HandleCurrentLevelSucceeded;
            CurrentLevelInstance.Failed += HandleCurrentLevelFailed;
            _objectiveManager.CurrentObjectiveHolder = CurrentLevelInstance;

            _camera.orthographicSize = CurrentLevelInstance.OrthographicSize;
        }

        private void RecordStartTime(string _)
        {
            _inputParser.CurrentInputChanged -= RecordStartTime;

            if (!CurrentLevelInstance) return;

            CurrentLevelInstance.FirstInputTime = Time.time;
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
            if (CurrentLevelInstance)
            {
                CurrentLevelInstance.EndTime = Time.time;

                var resultText = CurrentLevelInstance.Status switch
                {
                    ELevelStatus.Incomplete => "???",
                    ELevelStatus.Complete => "SUCCESS",
                    ELevelStatus.WrongExit => "FAILED: ONE TRAIN LEFT AT THE WRONG EXIT",
                    ELevelStatus.SkippedStops => "FAILED: ONE TRAIN LEFT BEFORE STOPPING IN STATIONS",
                    _ => throw new ArgumentOutOfRangeException()
                };

                CurrentLevelInstance.SetResultText(resultText);

                await UniTask.WaitForSeconds(2, cancellationToken: cancellationToken);

                if (CurrentLevelInstance.Status == ELevelStatus.Complete)
                {
                    CurrentLevelInstance.SetResultText(resultText + "<br>" + "Time: " + FormatLevelDuration(CurrentLevelInstance.Duration));

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