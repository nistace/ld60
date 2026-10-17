using System;
using System.Collections;
using LD60.Railway;
using LD60.Trains;
using UnityEngine;

namespace LD60.Levels
{
    [Serializable]
    public class LevelTrainObjective
    {
        [SerializeField] private Train _train;
        [SerializeField] private StationRailwayComponent[] _stops;
        [SerializeField] private ExitRailwayComponent _exit;

        public Train Train => _train;
        public ExitRailwayComponent Exit => _exit;

        public ELevelStatus Status { get; set; } = ELevelStatus.Incomplete;

        public int CompletedStops { get; set; }
        public int NextStopIndex => CompletedStops;

        public bool HasNextStop => NextStopIndex < _stops.Length;

        public StationRailwayComponent[] Stops => _stops;

        public bool TryStop(StationRailwayComponent station)
        {
            if (NextStopIndex >= _stops.Length) return false;
            if (station != _stops[NextStopIndex]) return false;

            CompletedStops++;
            return true;
        }
    }
}