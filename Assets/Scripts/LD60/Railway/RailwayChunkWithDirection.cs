using System;
using UnityEngine;

namespace LD60.Railway
{
    [Serializable]
    public class RailwayChunkWithDirection
    {
        public enum EDirection
        {
            Forward = 0,
            Backward = 1
        }

        [SerializeField] private RailwayChunk _chunk;
        [SerializeField] private EDirection _direction;

        public RailwayChunk Chunk => _chunk;
        public EDirection Direction => _direction;

        public Vector3 WorldOrigin => _direction switch
        {
            EDirection.Forward => _chunk.WorldOrigin,
            EDirection.Backward => _chunk.WorldDestination,
            _ => throw new ArgumentOutOfRangeException()
        };

        public Vector3 WorldDestination => _direction switch
        {
            EDirection.Forward => _chunk.WorldDestination,
            EDirection.Backward => _chunk.WorldOrigin,
            _ => throw new ArgumentOutOfRangeException()
        };

        public float ChunkLength => _chunk ? _chunk.SplineLength : 0;

        public void ChangeDestination(RailwayChunkWithDirection nextAtDestination)
        {
            if (_direction == EDirection.Forward) _chunk.NextChunk = nextAtDestination;
            else if (_direction == EDirection.Backward) _chunk.PreviousChunk = nextAtDestination;
        }

        public void SetOriginAccessible(bool accessible)
        {
            if (_direction == EDirection.Forward) _chunk.IsPreviousAccessible = accessible;
            else if (_direction == EDirection.Backward) _chunk.IsNextAccessible = accessible;
        }

        public bool IsNextAccessible()
        {
            if (!TryGetNextChunk(out _)) return false;

            return _direction switch
            {
                EDirection.Forward => _chunk.IsNextAccessible,
                EDirection.Backward => _chunk.IsPreviousAccessible,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public bool TryGetNextChunk(out RailwayChunkWithDirection nextChunk)
        {
            nextChunk = null;
            if (_chunk == null) return false;

            nextChunk = _direction switch
            {
                EDirection.Forward => _chunk.NextChunk,
                EDirection.Backward => _chunk.PreviousChunk,
                _ => throw new ArgumentOutOfRangeException()
            };

            return nextChunk != null && nextChunk.Chunk;
        }

        public bool EvaluateWorld(float positionOnChunk, out Vector3 worldPosition, out Quaternion worldRotation)
        {
            worldPosition = Vector3.zero;
            worldRotation = Quaternion.identity;

            if (!_chunk) return false;

            return _chunk.EvaluateWorld(_direction == EDirection.Forward ? positionOnChunk : 1 - positionOnChunk, out worldPosition, out worldRotation);
        }
    }
}