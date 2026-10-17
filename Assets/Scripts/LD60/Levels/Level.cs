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

        private readonly Dictionary<Train, LevelTrainObjective> _objectivesPerTrain = new();
        private readonly HashSet<LevelTrainObjective> _incompleteObjects = new();

        public event Action ObjectivesStateChanged;
        public event Action Succeeded;
        public event Action<ELevelFailedReason> Failed;

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
            if (!_objectivesPerTrain.TryGetValue(train, out var trainObjective)) return;
            if (trainObjective.Exit != exit)
            {
                trainObjective.Status = ELevelStatus.WrongExit;
                Failed?.Invoke(ELevelFailedReason.WrongExit);
            }

            trainObjective.Status = ELevelStatus.Complete;

            foreach (var trainCollider in train.GetComponents<Collider>())
            {
                trainCollider.enabled = false;
            }

            if (trainObjective.Status == ELevelStatus.Complete)
            {
                _incompleteObjects.Remove(trainObjective);

                if (_incompleteObjects.Count == 0)
                {
                    Succeeded?.Invoke();
                }
            }

            ObjectivesStateChanged?.Invoke();
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
                builder.Append(objective.ToDisplayString());
                builder.Append("</color>");
                builder.AppendLine();
            }

            return builder.ToString();
        }

        public string GetTipText() => _tipText;
    }
}