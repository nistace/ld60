using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using LD60.Inputs;
using LD60.Levels;
using LD60.Objectives;
using LD60.Web;
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
        [SerializeField] private LoginObjectiveHolder _loginObjectiveHolder;

        private string _userName = "";
        private Dictionary<string, (string name, int time)> _highScores = new();
        public Level CurrentLevelInstance { get; private set; }

        private CancellationTokenSource _resultCancellationTokenSource;

        public event Action PositiveFeedbackWritten;
        public event Action NegativeFeedbackWritten;

        private void Start()
        {
#if !UNITY_EDITOR
            _levelIndex = 0;
#endif

            DoLoginFlowAsync().Forget();
        }

        private async UniTask DoLoginFlowAsync()
        {
            var delay = UniTask.WaitForSeconds(.5f, cancellationToken: destroyCancellationToken);
            _highScores = await ScoresWeb.Get(destroyCancellationToken);

            await delay;

            _loginObjectiveHolder.Reset();
            _objectiveManager.CurrentObjectiveHolder = _loginObjectiveHolder;
            PositiveFeedbackWritten?.Invoke();

            var completionSource = new UniTaskCompletionSource<string>();
            CommandExecutor.SubscribeCommandHandler(_loginObjectiveHolder.Command, command => completionSource.TrySetResult(command));

            var result = await completionSource.Task;

            CommandExecutor.UnsubscribeCommandHandler(_loginObjectiveHolder.Command);

            _userName = result[_loginObjectiveHolder.Command.Length..].Trim();

            if (!string.IsNullOrEmpty(_userName))
            {
                _loginObjectiveHolder.Add(string.Empty);
                _loginObjectiveHolder.Add($"Hello {_userName} ^_^");
                PositiveFeedbackWritten?.Invoke();
                await UniTask.WaitForSeconds(2);
            }

            _loginObjectiveHolder.Add("Loading...");
            await UniTask.WaitForSeconds(2);

            StartCurrentLevel();
        }

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

            if (_highScores != null && _highScores.TryGetValue(CurrentLevelInstance.ID, out var highScore))
            {
                CurrentLevelInstance.SetHighScore(highScore.name, LevelDurationUtils.IntDurationToFloat(highScore.time));
            }

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
                PositiveFeedbackWritten?.Invoke();
                await UniTask.WaitForSeconds(2, cancellationToken: cancellationToken);

                resultBuilder.AppendLine($"Time: {LevelDurationUtils.FormatLevelDuration(CurrentLevelInstance.Duration)}");
                CurrentLevelInstance.SetResultText(resultBuilder.ToString());
                await UniTask.WaitForSeconds(2, cancellationToken: cancellationToken);

                if (!string.IsNullOrEmpty(_userName))
                {
                    resultBuilder.AppendLine("Registering score...");
                    CurrentLevelInstance.SetResultText(resultBuilder.ToString());

                    var postResult = await ScoresWeb.Post(new ScoresWeb.ScoreEntry
                        {
                            level = CurrentLevelInstance.ID,
                            time = LevelDurationUtils.DurationToInt(CurrentLevelInstance.Duration),
                            user_name = _userName
                        },
                        cancellationToken);

                    if (postResult is { success: true, newBest: true })
                    {
                        resultBuilder.AppendLine("**NEW BEST TIME**");
                        CurrentLevelInstance.SetResultText(resultBuilder.ToString());
                        PositiveFeedbackWritten?.Invoke();
                        var delay = UniTask.WaitForSeconds(2, cancellationToken: cancellationToken);

                        _highScores = await ScoresWeb.Get(destroyCancellationToken);

                        await delay;
                    }
                }

                resultBuilder.AppendLine("Loading next mission...");
            }
            else
            {
                resultBuilder.AppendLine(CurrentLevelInstance.Status switch
                {
                    ELevelStatus.WrongExit => "Failed: one train left at the wrong exit",
                    ELevelStatus.SkippedStops => "Failed: one train left before stopping at all stations",
                    ELevelStatus.TrainCrashed => "Failed: one train crashed",
                    ELevelStatus.Incomplete or ELevelStatus.Complete => "???",
                    _ => throw new ArgumentOutOfRangeException()
                });

                CurrentLevelInstance.SetResultText(resultBuilder.ToString());
                NegativeFeedbackWritten?.Invoke();
                await UniTask.WaitForSeconds(2, cancellationToken: cancellationToken);

                resultBuilder.AppendLine("Restarting mission...");
            }

            CurrentLevelInstance.SetResultText(resultBuilder.ToString());
            await UniTask.WaitForSeconds(3, cancellationToken: cancellationToken);

            callback?.Invoke();
        }
    }
}