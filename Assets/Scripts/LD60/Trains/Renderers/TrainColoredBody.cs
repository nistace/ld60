using System;
using Nrjwolf.Tools.AttachAttributes;
using UnityEngine;

namespace LD60.Trains.Renderers
{
    public class TrainColoredBody : MonoBehaviour
    {
        [SerializeField, GetComponent] private SpriteRenderer _spriteRenderer;
        [SerializeField, GetComponentInParent] private Train _train;

        private void Awake() => Refresh();
        private void OnValidate() => Refresh();

        private void Refresh()
        {
            _spriteRenderer.color = _train.Color;
        }
    }
}