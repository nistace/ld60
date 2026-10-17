using System;
using LD60.Inputs;
using Nrjwolf.Tools.AttachAttributes;
using UnityEngine;

namespace LD60.Railway
{
    [RequireComponent(typeof(CommandHolder))]
    public class SwitchRailwayComponent : MonoBehaviour, IPanelNameItem

    {
        [SerializeField] private string _nameOnPanel = "A";
        [SerializeField] private RailwayChunkWithDirection _splittingChunk;
        [SerializeField] private RailwayChunkWithDirection _targetChunkWhenOn;
        [SerializeField] private RailwayChunkWithDirection _targetChunkWhenOff;
        [SerializeField, GetComponent] private CommandHolder _commandHolder;
        [SerializeField] private bool _turnedOn = true;

        public bool TurnedOn => _turnedOn;
        public string NameOnPanel => _nameOnPanel;

        public event Action<bool> TurnedOnChanged;

        private void OnEnable()
        {
            RefreshImpactedChunks();
            _commandHolder.Executed += HandleCommandExecuted;
        }

        private void OnDisable()
        {
            _commandHolder.Executed -= HandleCommandExecuted;
        }

        private void HandleCommandExecuted()
        {
            _turnedOn = !_turnedOn;

            RefreshImpactedChunks();

            TurnedOnChanged?.Invoke(_turnedOn);
        }

        private void OnDrawGizmos()
        {
            if (_splittingChunk is { Chunk: not null })
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(_splittingChunk.WorldPosition(.9f), transform.position);
            }

            if (_targetChunkWhenOn is { Chunk: not null })
            {
                Gizmos.color = _turnedOn ? Color.green : Color.red;
                Gizmos.DrawLine(_targetChunkWhenOn.WorldPosition(.1f), transform.position);
            }

            if (_targetChunkWhenOff is { Chunk: not null })
            {
                Gizmos.color = _turnedOn ? Color.red : Color.green;
                Gizmos.DrawLine(_targetChunkWhenOff.WorldPosition(.1f), transform.position);
            }
        }

        private void RefreshImpactedChunks() => _splittingChunk.ChangeDestination(_turnedOn ? _targetChunkWhenOn : _targetChunkWhenOff);

        private void OnValidate() => _commandHolder.Command = $"SWITCH {NameOnPanel}";
    }
}