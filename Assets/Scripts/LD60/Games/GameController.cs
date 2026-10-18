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

        public event Action PostiveFeedbackWritten;
        public event Action NegativeFeedbackWritten;

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
            if (CurrentLevelInstance == null || CurrentLevelInstance.Status == ELevelStatus.Incomplete)
            {
                Debug.LogWarning("Running result, but the level is incomplete...");
                callback?.Invoke();
                return;
            }

            await UniTask.WaitForSeconds(.5f, cancellationToken: cancellationToken);

            var resultBuilder = new StringBuilder();
            CurrentLevelInstance.EndTime = Time.time;

            if (CurrentLevelInstance.Status == ELevelStatus.Complete)
            {
                resultBuilder.AppendLine("SUCCESS!");
                CurrentLevelInstance.SetResultText(resultBuilder.ToString());
                PostiveFeedbackWritten?.Invoke();
                await UniTask.WaitForSeconds(2, cancellationToken: cancellationToken);

                resultBuilder.AppendLine($"Time: {FormatLevelDuration(CurrentLevelInstance.Duration)}");
                CurrentLevelInstance.SetResultText(resultBuilder.ToString());
                PostiveFeedbackWritten?.Invoke();
                await UniTask.WaitForSeconds(2, cancellationToken: cancellationToken);

                // SUBMIT SCORE

                resultBuilder.AppendLine("<br>Loading next mission...");
                CurrentLevelInstance.SetResultText(resultBuilder.ToString());
                PostiveFeedbackWritten?.Invoke();
            }
            else
            {
                resultBuilder.Append(CurrentLevelInstance.Status switch
                {
                    ELevelStatus.WrongExit => "Failed: one train left at the wrong exit",
                    ELevelStatus.SkippedStops => "Failed: one train left before stopping at all stations",
                    ELevelStatus.Incomplete or ELevelStatus.Complete => "???",
                    _ => throw new ArgumentOutOfRangeException()
                });

                CurrentLevelInstance.SetResultText(resultBuilder.ToString());
                NegativeFeedbackWritten?.Invoke();
                await UniTask.WaitForSeconds(2, cancellationToken: cancellationToken);

                resultBuilder.AppendLine("<br>Restarting mission...");
                CurrentLevelInstance.SetResultText(resultBuilder.ToString());
                NegativeFeedbackWritten?.Invoke();
            }

            await UniTask.WaitForSeconds(3, cancellationToken: cancellationToken);

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