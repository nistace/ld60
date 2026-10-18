using System;
using LD60.Inputs;
using Nrjwolf.Tools.AttachAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LD60.Audio
{
    [RequireComponent(typeof(InputParser))]
    [RequireComponent(typeof(AudioSource))]
    public class InputParserAudio : MonoBehaviour
    {
        [SerializeField, GetComponent] private InputParser _inputParser;
        [SerializeField, GetComponent] private AudioSource _source;
        [SerializeField] private AudioClip[] _keypressClips;
        [SerializeField] private AudioClip _submitClip;

        private void OnEnable()
        {
            _inputParser.CurrentInputChanged += HandleCurrentInputChanged;
            _inputParser.InputCommitted += HandleInputCommitted;
        }

        private void OnDisable()
        {
            _inputParser.CurrentInputChanged -= HandleCurrentInputChanged;
            _inputParser.InputCommitted -= HandleInputCommitted;
        }

        private void HandleInputCommitted(string submitted)
        {
            if (string.IsNullOrEmpty(submitted)) return;
            _source.PlayOneShot(_submitClip);
        }

        private void HandleCurrentInputChanged(string newString)
        {
            if (string.IsNullOrEmpty(newString)) return;
            _source.PlayOneShot(_keypressClips[Random.Range(0, _keypressClips.Length)]);
        }
    }
}