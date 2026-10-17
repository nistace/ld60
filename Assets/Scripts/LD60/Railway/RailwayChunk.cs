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
        [SerializeField] private RailwayChunkWithDirection _nextChunkForward;
        [SerializeField] private bool _isNextChunkForwardAccessible;
        [SerializeField] private RailwayChunkWithDirection _nextChunkBackward;
        [SerializeField] private bool _isNextChunkBackwardAccessible;

        public Vector3 WorldOrigin => WorldPosition(0);
        public Vector3 WorldDestination => WorldPosition(1);

        public Vector3 WorldPosition(float ratio) => transform.TransformPoint(_splineContainer.Spline.EvaluatePosition(ratio));

        public RailwayChunkWithDirection NextChunkForward
        {
            get => _nextChunkForward;
            set => _nextChunkForward = value;
        }

        public RailwayChunkWithDirection NextChunkBackward
        {
            get => _nextChunkBackward;
            set => _nextChunkBackward = value;
        }

        public bool IsNextChunkBackwardAccessible
        {
            get => _isNextChunkBackwardAccessible;
            set => _isNextChunkBackwardAccessible = value;
        }

        public bool IsNextChunkForwardAccessible
        {
            get => _isNextChunkForwardAccessible;
            set => _isNextChunkForwardAccessible = value;
        }

        public float SplineLength => _splineContainer ? _splineContainer.Spline.GetLength() : 0;

        private void OnDrawGizmos()
        {
            if (NextChunkBackward.Chunk == null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(WorldOrigin, .1f);
            }
            else if (NextChunkBackward.WorldOrigin != WorldOrigin)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(WorldOrigin, NextChunkBackward.WorldOrigin);
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(WorldOrigin, .05f);
            }

            if (NextChunkForward.Chunk == null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(WorldDestination, .1f);
            }
            else if (NextChunkForward.WorldOrigin != WorldDestination)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(WorldDestination, NextChunkForward.WorldOrigin);
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(WorldDestination, .05f);
            }
        }

        public bool EvaluateWorld(float ratio, Quaternion rotationFix, out Vector3 worldPosition, out Quaternion worldRotation)
        {
            worldPosition = Vector3.zero;
            worldRotation = Quaternion.identity;
            if (!_splineContainer.Spline.Evaluate(ratio, out var position, out var tangent, out _)) return false;

            worldPosition = transform.TransformPoint(position);
            worldRotation = Quaternion.LookRotation(Vector3.forward, tangent) * rotationFix;

            return true;
        }
    }
}