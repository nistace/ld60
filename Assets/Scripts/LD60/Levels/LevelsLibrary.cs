using UnityEngine;

namespace LD60.Levels
{
    [CreateAssetMenu]
    public class LevelsLibrary : ScriptableObject
    {
        [SerializeField] private Level[] _levelPrefabs;

        public int Count => _levelPrefabs.Length;
        public Level this[int index] => _levelPrefabs[index];
    }
}