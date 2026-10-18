using System;
using LD60.Levels;
using UnityEngine;

namespace LD60.Games.Renderers
{
    public class TrainTargetHelper : MonoBehaviour
    {
        [SerializeField] private GameController _gameController;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Transform _child;
        [SerializeField] private AnimationCurve _offsetCurve;

        private void LateUpdate()
        {
            var enable = _gameController.CurrentLevelInstance is { SelectedObjective : { Train: { CurrentRailwayChunk: { Chunk: not null } }, Status: not ELevelStatus.Complete } };
            _renderer.enabled = enable;

            if (!enable) return;

            var objective = _gameController.CurrentLevelInstance.SelectedObjective;

            transform.position = objective.TryGetNextStop(out var nextStop) ? nextStop.transform.position : objective.Exit.transform.position;
            transform.rotation = transform.position == objective.Train.transform.position ? Quaternion.identity : Quaternion.FromToRotation(Vector3.up, transform.position - objective.Train.transform.position);

            _child.localPosition = Vector3.up * _offsetCurve.Evaluate(Time.time);
        }
    }
}