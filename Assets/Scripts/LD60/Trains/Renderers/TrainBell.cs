using Nrjwolf.Tools.AttachAttributes;
using UnityEngine;

namespace LD60.Trains.Renderers
{
    [RequireComponent(typeof(Train))]
    [RequireComponent(typeof(AudioSource))]
    public class TrainBell : MonoBehaviour
    {
        [SerializeField, GetComponent] private Train _train;
        [SerializeField, GetComponent] private AudioSource _audioSource;
        [SerializeField] private AudioClip _clip;

        private void OnEnable() => _train.StartedToMove += HandleStartedToMove;
        private void OnDisable() => _train.StartedToMove -= HandleStartedToMove;
        private void HandleStartedToMove() => _audioSource.PlayOneShot(_clip);
    }
}