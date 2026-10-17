using System;
using LD60.Inputs;
using Nrjwolf.Tools.AttachAttributes;
using UnityEngine;

namespace LD60.Railway
{
    [RequireComponent(typeof(CommandHolder))]
    public class SwitchRailwayComponent : MonoBehaviour
    {
        [SerializeField] private RailwayChunkWithDirection _splittingChunk;
        [SerializeField] private RailwayChunkWithDirection _targetChunkWhenOn;
        [SerializeField] private RailwayChunkWithDirection _targetChunkWhenOff;
        [SerializeField, GetComponent] private CommandHolder _commandHolder;
        [SerializeField] private bool _turnedOn = true;

        public bool TurnedOn => _turnedOn;

        public event Action<bool> TurnedOnChanged;

        private void OnEnable()
        {
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

        private void RefreshImpactedChunks() => _splittingChunk.ChangeDestination(_turnedOn ? _targetChunkWhenOn : _targetChunkWhenOff);
    }
}