using System;
using System.Collections.Generic;
using System.Text;
using LD60.Objectives;
using LD60.Railway;
using LD60.Trains;
using UnityEngine;

namespace LD60.Levels
{
    public class Level : MonoBehaviour, IObjectiveHolder
    {
        [SerializeField] private string _introText;
        [SerializeField] private string _tipText;
        [SerializeField] private LevelTrainObjective[] _objectives;
        [SerializeField] private LevelDisplayConfig _displayConfig;
        [SerializeField] private float _orthographicSize = 4;

        private readonly Dictionary<Train, LevelTrainObjective> _objectivesPerTrain = new();
        private readonly HashSet<LevelTrainObjective> _incompleteObjects = new();

        public float OrthographicSize => _orthographicSize;
        public float FirstInputTime { get; set; }
        public float EndTime { get; set; }
        public ELevelStatus Status { get; private set; }
        private string ResultText { get; set; }
        public float Duration => EndTime - FirstInputTime;

        public event Action ObjectivesStateChanged;
        public event Action Succeeded;
        public event Action<ELevelStatus> Failed;

        private void Start()
        {
            foreach (var objective in _objectives)
            {
                if (objective.Status == ELevelStatus.Incomplete)
                {
                    _incompleteObjects.Add(objective);
                }

                _objectivesPerTrain.Add(objective.Train, objective);
                objective.Train.ExitReached += HandleTrainExitReached;
                objective.Train.StationReached += HandleTrainStationReached;
            }
        }

        private void OnDestroy()
        {
            foreach (var objective in _objectives)
            {
                if (objective.Train)
                {
                    objective.Train.ExitReached -= HandleTrainExitReached;
                }
            }
        }

        private void HandleTrainExitReached(Train train, ExitRailwayComponent exit)
        {
            if (Status != ELevelStatus.Incomplete) return;

            if (!_objectivesPerTrain.TryGetValue(train, out var trainObjective)) return;

            foreach (var trainCollider in train.GetComponents<Collider>())
            {
                trainCollider.enabled = false;
            }

            if (trainObjective.Exit != exit)
            {
                trainObjective.Status = ELevelStatus.WrongExit;
                Fail(ELevelStatus.WrongExit);
            }
            else if (trainObjective.HasNextStop)
            {
                trainObjective.Status = ELevelStatus.SkippedStops;
                Fail(ELevelStatus.SkippedStops);
            }
            else
            {
                trainObjective.Status = ELevelStatus.Complete;
            }

            if (trainObjective.Status == ELevelStatus.Complete)
            {
                _incompleteObjects.Remove(trainObjective);

                if (_incompleteObjects.Count == 0)
                {
                    Status = ELevelStatus.Complete;
                    Succeeded?.Invoke();
                }
            }

            ObjectivesStateChanged?.Invoke();
        }

        private void Fail(ELevelStatus status)
        {
            Status = status;
            Failed?.Invoke(Status);
        }

        private void HandleTrainStationReached(Train train, StationRailwayComponent station)
        {
            if (Status != ELevelStatus.Incomplete) return;
            if (!_objectivesPerTrain.TryGetValue(train, out var trainObjective)) return;

            if (trainObjective.TryStop(station))
            {
                ObjectivesStateChanged?.Invoke();
            }
        }

        public string GetObjectivesText()
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrEmpty(_introText))
            {
                builder.AppendLine(_introText);
            }

            foreach (var objective in _objectives)
            {
                var displayData = _displayConfig.GetData(objective.Status);
                if (displayData.TryGetColor(out var textColor))
                {
                    builder.Append($"<#{ColorUtility.ToHtmlStringRGB(textColor)}>");
                }

                builder.Append($"[{displayData.Text}] ");
                builder.Append($"Lead Train {objective.Train.NameOnPanel} to ");

                for (var stopIndex = 0; stopIndex < objective.Stops.Length; stopIndex++)
                {
                    var stop = objective.Stops[stopIndex];
                    var stopStatus = objective.CompletedStops > stopIndex ? 'V' : objective.Status == ELevelStatus.Incomplete ? ' ' : 'X';
                    builder.Append($"[{stopStatus}] {stop.NameOnPanel}, ");
                }

                builder.Append(objective.Exit.NameOnPanel);
                builder.Append("</color>");
                builder.AppendLine();
            }

            if (!string.IsNullOrEmpty(ResultText))
            {
                var displayData = _displayConfig.GetData(Status);

                if (displayData.TryGetColor(out var textColor))
                {
                    builder.Append($"<#{ColorUtility.ToHtmlStringRGB(textColor)}>");
                }

                builder.Append(ResultText);
            }

            return builder.ToString();
        }

        public string GetTipText() => _tipText;

        public void SetResultText(string resultText)
        {
            ResultText = resultText;
            ObjectivesStateChanged?.Invoke();
        }
    }
}