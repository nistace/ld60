using UnityEngine;

namespace LD60.Railway
{
    public class StationRailwayComponent : MonoBehaviour, IPanelNameItem
    {
        [SerializeField] private string _stationName = "OLD";
        [SerializeField] private RailwayChunkWithDirection[] _stationExits;
        [SerializeField] private float _waitDelay = 3f;

        public string NameOnPanel => _stationName;

        private bool _stopped;
        private float _nextCancelStopTime;

        public void StopTrains()
        {
            foreach (var stationExit in _stationExits)
            {
                stationExit.SetOriginAccessible(false);
            }

            _stopped = true;
            _nextCancelStopTime = Time.time + _waitDelay;
        }

        private void Update()
        {
            if (!_stopped) return;
            if (_nextCancelStopTime > Time.time) return;

            _stopped = false;

            foreach (var stationExit in _stationExits)
            {
                stationExit.SetOriginAccessible(true);
            }
        }
    }
}