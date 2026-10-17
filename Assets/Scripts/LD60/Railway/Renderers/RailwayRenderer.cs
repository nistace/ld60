using Nrjwolf.Tools.AttachAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace LD60.Railway.Renderers
{
    [RequireComponent(typeof(SplineContainer))]
    [ExecuteInEditMode]
    public class RailwayRenderer : MonoBehaviour
    {
        [SerializeField, GetComponent] private SplineContainer _spline;
        [SerializeField] private GameObject _plankPrefab;
        [SerializeField] private Quaternion _plankRotation = Quaternion.Euler(0, 0, 90);
        [SerializeField] private float _gapBetweenPlanks = .1f;
        [SerializeField] private float _gapAtEnd = .1f;
        [SerializeField] private float _builtWithLength = -1;

#if UNITY_EDITOR
        private void Update()
        {
            if (!_spline || _spline.Spline == null) return;
            if (!_plankPrefab) return;
            if (_gapBetweenPlanks < .01f) return;
            if (Mathf.Approximately(_builtWithLength, _spline.Spline.GetLength())) return;

            _builtWithLength = _spline.Spline.GetLength();

            var plankIndex = 0;

            for (var distance = 0f; distance < _builtWithLength - _gapAtEnd; distance += _gapBetweenPlanks)
            {
                if (!_spline.Spline.Evaluate(distance / _builtWithLength, out var position, out var tangent, out _)) continue;

                while (transform.childCount <= plankIndex)
                {
                    PrefabUtility.InstantiatePrefab(_plankPrefab.gameObject, transform);
                }

                var plank = transform.GetChild(plankIndex);

                plank.transform.localPosition = position;
                plank.transform.localRotation = Quaternion.LookRotation(Vector3.forward, tangent) * _plankRotation;

                plankIndex++;
            }

            while (transform.childCount > plankIndex)
            {
                DestroyImmediate(transform.GetChild(plankIndex).gameObject);
            }
        }
#endif
    }
}