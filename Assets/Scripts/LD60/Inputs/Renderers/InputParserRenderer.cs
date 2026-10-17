using System;
using TMPro;
using UnityEngine;

namespace LD60.Inputs.Renderers
{
    public class InputParserRenderer : MonoBehaviour
    {
        [SerializeField] private InputParser _inputParser;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private float _caretDuration = .5f;
        [SerializeField] private char _caret = '█';

        private void Update()
        {
            if (_caretDuration <= 0)
            {
                _text.text = _inputParser.CurrentInput;
            }
            else
            {
                _text.text = (int)(Time.time / _caretDuration) % 2 == 0 ? _inputParser.CurrentInput + _caret : _inputParser.CurrentInput;
            }
        }
    }
}