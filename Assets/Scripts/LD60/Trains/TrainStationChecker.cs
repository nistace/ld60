using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LD60.Railway;
using Nrjwolf.Tools.AttachAttributes;
using UnityEngine;

namespace LD60.Trains
{
    [RequireComponent(typeof(Train))]
    public class TrainStationChecker : MonoBehaviour
    {
        [SerializeField, GetComponent] private Train _train;

        private StationLaneTrigger _currentTrigger;

        private CancellationTokenSource _checkStopCancellationTokenSource;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var stationLane = other.GetComponentInParent<StationLaneTrigger>();
            if (!stationLane) return;

            _currentTrigger = stationLane;
            _checkStopCancellationTokenSource?.Cancel();
            _checkStopCancellationTokenSource?.Dispose();
            _checkStopCancellationTokenSource = new CancellationTokenSource();

            CheckStopAsync(_checkStopCancellationTokenSource.Token).Forget();
        }

        private async UniTask CheckStopAsync(CancellationToken token)
        {
            await UniTask.WaitUntil(() => Mathf.Approximately(_train.Speed, 0), cancellationToken: token);

            if (!_currentTrigger) return;

            _train.OnStationReached(_currentTrigger.Station);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!_currentTrigger) return;

            var stationLane = other.GetComponentInParent<StationLaneTrigger>();

            if (_currentTrigger != stationLane) return;

            _checkStopCancellationTokenSource?.Cancel();
            _checkStopCancellationTokenSource?.Dispose();
            _checkStopCancellationTokenSource = null;
            _currentTrigger = null;
        }

        private void OnDestroy()
        {
            _checkStopCancellationTokenSource?.Cancel();
            _checkStopCancellationTokenSource?.Dispose();
            _checkStopCancellationTokenSource = null;
        }
    }
}