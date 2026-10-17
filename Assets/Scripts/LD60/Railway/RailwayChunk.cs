using System;
using Nrjwolf.Tools.AttachAttributes;
using UnityEngine;
using UnityEngine.Splines;

namespace LD60.Railway
{
    [RequireComponent(typeof(SplineContainer))]
    public class RailwayChunk : MonoBehaviour
    {
        [SerializeField, GetComponent] private SplineContainer _splineContainer;
        [SerializeField] private RailwayChunkWithDirection _previousChunk;
        [SerializeField] private bool _isPreviousAccessible;
        [SerializeField] private RailwayChunkWithDirection _nextChunk;
        [SerializeField] private bool _isNextAccessible;

        public Vector3 WorldOrigin => transform.TransformPoint(_splineContainer.Spline.EvaluatePosition(0));
        public Vector3 WorldDestination => transform.TransformPoint(_splineContainer.Spline.EvaluatePosition(1));

        public RailwayChunkWithDirection PreviousChunk
        {
            get => _previousChunk;
            set => _previousChunk = value;
        }

        public RailwayChunkWithDirection NextChunk
        {
            get => _nextChunk;
            set => _nextChunk = value;
        }

        public bool IsPreviousAccessible
        {
            get => _isPreviousAccessible;
            set => _isPreviousAccessible = value;
        }

        public bool IsNextAccessible
        {
            get => _isNextAccessible;
            set => _isNextAccessible = value;
        }

        public float SplineLength => _splineContainer ? _splineContainer.Spline.GetLength() : 0;

        private void OnDrawGizmos()
        {
            if (_previousChunk.Chunk == null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(WorldOrigin, .1f);
            }
            else if (_previousChunk.WorldOrigin != WorldOrigin)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(WorldOrigin, .05f);
                Gizmos.DrawSphere(_previousChunk.WorldOrigin, .05f);
                Gizmos.DrawLine(WorldOrigin, _previousChunk.WorldOrigin);
            }

            if (_nextChunk.Chunk == null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(WorldDestination, .1f);
            }
            else if (_nextChunk.WorldOrigin != WorldDestination)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(WorldOrigin, .05f);
                Gizmos.DrawSphere(_nextChunk.WorldOrigin, .05f);
                Gizmos.DrawLine(WorldDestination, _nextChunk.WorldOrigin);
            }
        }

        public bool EvaluateWorld(float ratio, out Vector3 worldPosition, out Quaternion worldRotation)
        {
            worldPosition = Vector3.zero;
            worldRotation = Quaternion.identity;
            if (!_splineContainer.Spline.Evaluate(ratio, out var position, out var tangent, out _)) return false;

            worldPosition = transform.TransformPoint(position);
            worldRotation = Quaternion.LookRotation(Vector3.forward, tangent) * Quaternion.Euler(0, 0, 90);

            return true;
        }
    }
}