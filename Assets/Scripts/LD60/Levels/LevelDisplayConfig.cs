using System;
using UnityEngine;

namespace LD60.Levels
{
    [CreateAssetMenu]
    public class LevelDisplayConfig : ScriptableObject
    {
        [SerializeField] private StateData _incomplete = new(' ', Color.clear);
        [SerializeField] private StateData _success = new('V', Color.clear);
        [SerializeField] private StateData _fail = new('X', Color.orangeRed);

        public StateData GetData(ELevelStatus status) => status switch
        {
            ELevelStatus.Incomplete => _incomplete,
            ELevelStatus.Complete => _success,
            _ => _fail
        };

        [Serializable]
        public class StateData
        {
            [SerializeField] private char _text;
            [SerializeField] private Color _color;

            public char Text => _text;

            public bool TryGetColor(out Color color)
            {
                color = _color;
                return color != Color.clear;
            }

            public StateData() : this(' ', Color.clear) { }

            public StateData(char text, Color color)
            {
                _text = text;
                _color = color;
            }
        }
    }
}