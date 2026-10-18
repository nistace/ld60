using System;
using Nrjwolf.Tools.AttachAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LD60.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        [SerializeField, GetComponent] private AudioSource _source;
        [SerializeField] private InputActionReference _reduceVolumeActionReference;
        [SerializeField] private InputActionReference _augmentVolumeActionReference;
        [SerializeField] private float _volumeStep = .2f;
        

        public float Volume => _source.volume;

        public event Action<float> VolumeChanged;

        private void OnEnable()
        {
            _reduceVolumeActionReference.action.performed += HandleReduceVolumePerformed;
            _augmentVolumeActionReference.action.performed += HandleAugmentVolumePerformed;
        }

        private void OnDisable()
        {
            _reduceVolumeActionReference.action.performed -= HandleReduceVolumePerformed;
            _augmentVolumeActionReference.action.performed -= HandleAugmentVolumePerformed;
        }

        private void HandleReduceVolumePerformed(InputAction.CallbackContext obj) => SetVolume(_source.volume - _volumeStep);
        private void HandleAugmentVolumePerformed(InputAction.CallbackContext obj) => SetVolume(_source.volume + _volumeStep);

        private void SetVolume(float newVolume)
        {
            var clampedVolume = Mathf.Clamp01(newVolume);
            if (Mathf.Approximately(_source.volume, clampedVolume)) return;

            _source.volume = clampedVolume;

            VolumeChanged?.Invoke(clampedVolume);
        }
    }
}