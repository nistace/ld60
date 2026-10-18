using LD60.Games;
using Nrjwolf.Tools.AttachAttributes;
using UnityEngine;

namespace LD60.Audio
{
    [RequireComponent(typeof(GameController))]
    [RequireComponent(typeof(AudioSource))]
    public class GameControllerAudio : MonoBehaviour
    {
        [SerializeField, GetComponent] private AudioSource _audioSource;
        [SerializeField, GetComponent] private GameController _gameController;
        [SerializeField] private AudioClip _positiveFeedback;
        [SerializeField] private AudioClip _negativeFeedback;

        private void OnEnable()
        {
            _gameController.PositiveFeedbackWritten += HandlePositiveFeedback;
            _gameController.NegativeFeedbackWritten += HandleNegativeFeedback;
        }

        private void OnDisable()
        {
            _gameController.PositiveFeedbackWritten -= HandlePositiveFeedback;
            _gameController.NegativeFeedbackWritten -= HandleNegativeFeedback;
        }

        private void HandlePositiveFeedback() => _audioSource.PlayOneShot(_positiveFeedback);
        private void HandleNegativeFeedback() => _audioSource.PlayOneShot(_negativeFeedback);
    }
}