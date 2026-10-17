using System;
using LD60.Inputs;
using Nrjwolf.Tools.AttachAttributes;
using UnityEngine;

namespace LD60.Railway
{
    [RequireComponent(typeof(CommandHolder))]
    public class LightRailwayComponent : MonoBehaviour
    {
        [SerializeField, GetComponent] private CommandHolder _commandHolder;
        [SerializeField] private RailwayChunkWithDirection[] _chunks;
        [SerializeField] private bool _turnedOn = true;

        public bool TurnedOn => _turnedOn;

        public event Action<bool> TurnedOnChanged;

        private void OnEnable()
        {
            RefreshRailwayChunks();
            _commandHolder.Executed += HandleCommandExecuted;
        }

        private void OnDisable()
        {
            _commandHolder.Executed -= HandleCommandExecuted;
        }

        private void HandleCommandExecuted()
        {
            _turnedOn = !_turnedOn;

            RefreshRailwayChunks();

            TurnedOnChanged?.Invoke(_turnedOn);
        }

        private void RefreshRailwayChunks()
        {
            foreach (var chunk in _chunks)
            {
                chunk.SetOriginAccessible(_turnedOn);
            }
        }

        private void OnDrawGizmos()
        {
            if (_chunks == null) return;

            Gizmos.color = _turnedOn ? Color.green : Color.red;

            foreach (var chunk in _chunks)
            {
                if (chunk.Chunk == null) continue;
                Gizmos.DrawLine(transform.position, chunk.WorldOrigin);
            }
        }
    }
}