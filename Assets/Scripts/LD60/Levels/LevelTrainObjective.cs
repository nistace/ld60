using System;
using LD60.Railway;
using LD60.Trains;
using UnityEngine;

namespace LD60.Levels
{
    [Serializable]
    public class LevelTrainObjective
    {
        [SerializeField] private Train _train;
        [SerializeField] private ExitRailwayComponent _exit;

        public Train Train => _train;
        public ExitRailwayComponent Exit => _exit;

        public ELevelStatus Status { get; set; } = ELevelStatus.Incomplete;

        public string ToDisplayString() => $"Train {_train.NameOnPanel} to {_exit.NameOnPanel}";
    }
}