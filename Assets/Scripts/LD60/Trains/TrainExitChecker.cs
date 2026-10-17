using LD60.Railway;
using Nrjwolf.Tools.AttachAttributes;
using UnityEngine;

namespace LD60.Trains
{
    [RequireComponent(typeof(Train))]
    public class TrainExitChecker : MonoBehaviour
    {
        [SerializeField, GetComponent] private Train _train;

        private StationLaneTrigger _currentTrigger;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var exit = other.GetComponentInParent<ExitRailwayComponent>();
            if (!exit) return;

            _train.OnExitReached(exit);
        }
    }
}