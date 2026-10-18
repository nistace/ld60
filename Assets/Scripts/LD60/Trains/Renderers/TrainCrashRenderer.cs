using Nrjwolf.Tools.AttachAttributes;
using UnityEngine;

namespace LD60.Trains.Renderers
{
    [RequireComponent(typeof(Train))]
    [RequireComponent(typeof(AudioSource))]
    public class TrainCrashRenderer : MonoBehaviour
    {
        [SerializeField, GetComponent] private Train _train;
        [SerializeField, GetComponent] private AudioSource _audioSource;
        [SerializeField] private AudioClip _clip;
        [SerializeField] private ParticleSystem[] _particleSystems;

        private void OnEnable()
        {
            _train.Crashed += HandleCrashed;
        }

        private void OnDisable()
        {
            _train.Crashed -= HandleCrashed;
        }

        private void HandleCrashed(Train obj, Vector3 vector3)
        {
            if (_clip) _audioSource.PlayOneShot(_clip);

            foreach (var system in _particleSystems)
            {
                system.Play();
            }
        }
    }
}