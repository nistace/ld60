using System;
using LD60.Inputs;
using Nrjwolf.Tools.AttachAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LD60.Audio
{
    [RequireComponent(typeof(CommandExecutor))]
    [RequireComponent(typeof(AudioSource))]
    public class CommandExecutorAudio : MonoBehaviour
    {
        [SerializeField, GetComponent] private CommandExecutor _commandExecutor;
        [SerializeField, GetComponent] private AudioSource _source;
        [SerializeField] private AudioClip _validClip;
        [SerializeField] private AudioClip _invalidClip;

        private void OnEnable()
        {
            _commandExecutor.CommandHandled += HandleCommandHandled;
        }

        private void OnDisable()
        {
            _commandExecutor.CommandHandled -= HandleCommandHandled;
        }

        private void HandleCommandHandled(CommandExecutor.ExecutedCommand executedCommand) => _source.PlayOneShot(executedCommand.Valid ? _validClip : _invalidClip);
    }
}