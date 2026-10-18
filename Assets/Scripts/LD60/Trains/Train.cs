using System;
using LD60.Railway;
using UnityEngine;

namespace LD60.Trains
{
    public class Train : MonoBehaviour, IPanelNameItem
    {
        [SerializeField] private TrainIdentity _identity;
        [SerializeField] private RailwayChunkWithDirection _currentRailwayChunk;
        [SerializeField, Range(0, 1)] private float _progressOnChunkRatio;
        [SerializeField] private float _acceleration = 1;
        [SerializeField] private float _deceleration = 1;
        [SerializeField] private float _maxSpeed = 2;
        [SerializeField] private float _stopDistance = .5f;

        public float Speed { get; private set; }
        public RailwayChunkWithDirection CurrentRailwayChunk => _currentRailwayChunk;
        public float ProgressOnChunkRatio => _progressOnChunkRatio;

        public string NameOnPanel => _identity ? _identity.NameOnPanel : string.Empty;
        private float TimeToStop => Speed / _deceleration;
        private float RequiredDistanceToStop => Speed * TimeToStop * .5f;
        private float CurrentDistanceFromChunkEnd => (1 - _progressOnChunkRatio) * _currentRailwayChunk.ChunkLength;
        public Color Color => _identity.Color;
        private bool HasCrashed { get; set; }
        private Vector2 CrashedVelocity { get; set; }

        public event Action<Train, ExitRailwayComponent> ExitReached;
        public event Action<Train, StationRailwayComponent> StationReached;
        public event Action<Train> Crashed;

        private bool ShouldBrake()
        {
            if (_currentRailwayChunk == null) return true;
            if (_currentRailwayChunk.Chunk == null) return true;

            if (_currentRailwayChunk.IsNextAccessible()) return false;
            if (RequiredDistanceToStop > CurrentDistanceFromChunkEnd) return false;
            if (CurrentDistanceFromChunkEnd - RequiredDistanceToStop > _stopDistance) return false;

            return true;
        }

        private void Update()
        {
            if (_currentRailwayChunk == null) return;
            UpdateMovement();
            UpdateRailwayChunk();
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (!_currentRailwayChunk.EvaluateWorld(_progressOnChunkRatio, out var worldPosition, out var worldRotation)) return;

            transform.position = worldPosition;
            transform.rotation = worldRotation;

            transform.localScale = Mathf.Abs(Vector3.SignedAngle(Vector3.up, transform.up, Vector3.forward)) > 90 ? new Vector3(1, -1, 1) : Vector3.one;
        }

        private void UpdateRailwayChunk()
        {
            while (_currentRailwayChunk != null && _progressOnChunkRatio >= 1)
            {
                var distanceOverflow = (_progressOnChunkRatio - 1) * _currentRailwayChunk.ChunkLength;
                if (_currentRailwayChunk.TryGetNextChunk(out _currentRailwayChunk))
                {
                    _progressOnChunkRatio = distanceOverflow / _currentRailwayChunk?.ChunkLength ?? 0;
                }
            }
        }

        private void UpdateMovement()
        {
            var chunkLength = _currentRailwayChunk.ChunkLength;
            if (Mathf.Approximately(chunkLength, 0)) return;

            var brake = ShouldBrake();

            Speed = Mathf.MoveTowards(Speed, brake ? 0 : _maxSpeed, Time.deltaTime * (brake ? _deceleration : _acceleration));
            _progressOnChunkRatio += (Speed * Time.deltaTime) / chunkLength;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            _currentRailwayChunk.EvaluateWorld(_progressOnChunkRatio, out var worldPosition, out _);
            Gizmos.DrawLine(transform.position, worldPosition);
        }

        public void OnExitReached(ExitRailwayComponent exit) => ExitReached?.Invoke(this, exit);
        public void OnStationReached(StationRailwayComponent station) => StationReached?.Invoke(this, station);

        private void OnCollisionEnter2D(Collision2D other)
        {
            HasCrashed = true;
            CrashedVelocity = other.relativeVelocity;
            Crashed?.Invoke(this);
        }
    }
}