using Nrjwolf.Tools.AttachAttributes;
using UnityEngine;

namespace LD60.Railway
{
    public class StationLaneTrigger : MonoBehaviour
    {
        [SerializeField, GetComponentInParent] private StationRailwayComponent _station;

        public StationRailwayComponent Station => _station;

        private void OnTriggerEnter2D(Collider2D other) => _station.StopTrains();
    }
}