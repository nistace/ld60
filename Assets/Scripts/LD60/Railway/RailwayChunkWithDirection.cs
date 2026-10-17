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

        public RailwayChunkWithDirection() { }

        public RailwayChunkWithDirection(RailwayChunk chunk, EDirection direction)
        {
            _chunk = chunk;
            _direction = direction;
        }

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

        public Vector3 WorldPosition(float ratio) => _chunk ? _chunk.WorldPosition(_direction == EDirection.Forward ? ratio : 1 - ratio) : Vector3.zero;

        public float ChunkLength => _chunk ? _chunk.SplineLength : 0;

        public void ChangeDestination(RailwayChunkWithDirection nextAtDestination)
        {
            if (_direction == EDirection.Forward) _chunk.NextChunkForward = nextAtDestination;
            else if (_direction == EDirection.Backward) _chunk.NextChunkBackward = nextAtDestination;
        }

        public void SetOriginAccessible(bool accessible)
        {
            if (_direction == EDirection.Forward) _chunk.IsNextChunkBackwardAccessible = accessible;
            else if (_direction == EDirection.Backward) _chunk.IsNextChunkForwardAccessible = accessible;
        }

        public bool IsNextAccessible()
        {
            if (!TryGetNextChunk(out _)) return false;

            return _direction switch
            {
                EDirection.Forward => _chunk.IsNextChunkForwardAccessible,
                EDirection.Backward => _chunk.IsNextChunkBackwardAccessible,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public bool TryGetNextChunk(out RailwayChunkWithDirection nextChunk)
        {
            nextChunk = null;
            if (_chunk == null) return false;

            nextChunk = _direction switch
            {
                EDirection.Forward => _chunk.NextChunkForward,
                EDirection.Backward => _chunk.NextChunkBackward,
                _ => throw new ArgumentOutOfRangeException()
            };

            return nextChunk != null && nextChunk.Chunk;
        }

        public bool EvaluateWorld(float positionOnChunk, out Vector3 worldPosition, out Quaternion worldRotation)
        {
            worldPosition = Vector3.zero;
            worldRotation = Quaternion.identity;

            if (!_chunk) return false;

            return _chunk.EvaluateWorld(_direction == EDirection.Forward ? positionOnChunk : 1 - positionOnChunk,
                _direction == EDirection.Forward ? Quaternion.Euler(0, 0, 90) : Quaternion.Euler(0, 0, -90),
                out worldPosition,
                out worldRotation);
        }
    }
}