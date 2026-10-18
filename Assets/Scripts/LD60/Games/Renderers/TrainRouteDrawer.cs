using Unity.Collections;
using UnityEngine;

namespace LD60.Games.Renderers
{
    public class TrainRouteDrawer : MonoBehaviour
    {
        [SerializeField] private GameController _gameController;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private float _distanceToShow = 20;
        [SerializeField] private float _stepSize = .1f;

        private NativeArray<Vector3> _positions = new(2000, Allocator.Persistent);

        private void LateUpdate()
        {
            var enable = _gameController.CurrentLevelInstance is { SelectedObjective : { Train: { CurrentRailwayChunk: { Chunk: not null } } } };
            _lineRenderer.enabled = enable;

            if (!enable) return;

            var train = _gameController.CurrentLevelInstance.SelectedObjective.Train;
            var chunk = train.CurrentRailwayChunk;
            var progressOnChunk = train.ProgressOnChunkRatio;
            var computedDistance = 0f;

            _positions[0] = train.transform.position;
            var index = 1;

            while (chunk != null && computedDistance < _distanceToShow && index < _positions.Length)
            {
                computedDistance += _stepSize;
                progressOnChunk += _stepSize / chunk.ChunkLength;
                _positions[index++] = chunk.WorldPosition(Mathf.Clamp01(progressOnChunk));

                if (progressOnChunk >= 1)
                {
                    chunk.TryGetNextChunk(out chunk);
                    progressOnChunk = 0;
                }
            }

            _lineRenderer.positionCount = index - 1;
            _lineRenderer.SetPositions(_positions);
        }
    }
}