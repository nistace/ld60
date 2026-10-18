using UnityEngine;
using UnityEngine.UI;

namespace LD60.Audio.Renderers
{
    public class MusicPlayerVolumeRenderer : MonoBehaviour
    {
        [SerializeField] private MusicPlayer _musicPlayer;
        [SerializeField] private Image[] _images;
        [SerializeField] private Sprite _onSprite;
        [SerializeField] private Sprite _offSprite;

        private void OnEnable()
        {
            Refresh();
            _musicPlayer.VolumeChanged += HandleVolumeChanged;
        }

        private void OnDisable()
        {
            _musicPlayer.VolumeChanged -= HandleVolumeChanged;
        }

        private void HandleVolumeChanged(float newVolume) => Refresh();

        private void Refresh()
        {
            var _lightsOn = Mathf.RoundToInt(_musicPlayer.Volume * _images.Length);

            for (var i = 0; i < _images.Length; i++)
            {
                _images[i].overrideSprite = i < _lightsOn ? _onSprite : _offSprite;
            }
        }
    }
}