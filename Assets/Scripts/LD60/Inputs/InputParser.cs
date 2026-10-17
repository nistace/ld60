using System;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LD60.Inputs
{
    public class InputParser : MonoBehaviour
    {
        [SerializeField] private InputActionReference _validateInputActionReference;
        [SerializeField] private InputActionReference _eraseInputActionReference;
        private Keyboard _keyboard;

        private StringBuilder _inputBuilder = new();

        public event Action<string> CurrentInputChanged;
        public event Action<string> InputCommitted;
        public string CurrentInput => _inputBuilder.ToString();

        private void OnEnable()
        {
            _validateInputActionReference.action.performed += HandleValidatePerformed;
            _eraseInputActionReference.action.performed += HandleErasePerformed;

            _keyboard = Keyboard.current;

            if (_keyboard == null) return;

            _keyboard.onTextInput += HandleTextInput;
        }

        private void OnDisable()
        {
            _validateInputActionReference.action.performed -= HandleValidatePerformed;
            _eraseInputActionReference.action.performed -= HandleValidatePerformed;

            if (_keyboard == null) return;

            _keyboard.onTextInput -= HandleTextInput;
        }

        private void HandleValidatePerformed(InputAction.CallbackContext obj)
        {
            var committedInput = _inputBuilder.ToString();
            _inputBuilder.Clear();
            CurrentInputChanged?.Invoke(_inputBuilder.ToString());
            InputCommitted?.Invoke(committedInput);
        }

        private void HandleErasePerformed(InputAction.CallbackContext obj)
        {
            if (_inputBuilder.Length == 0) return;

            _inputBuilder.Remove(_inputBuilder.Length - 1, 1);
            CurrentInputChanged?.Invoke(_inputBuilder.ToString());
        }

        private void HandleTextInput(char character)
        {
            if (!TryParseCharacter(character, out var parsed)) return;

            _inputBuilder.Append(parsed);
            CurrentInputChanged?.Invoke(_inputBuilder.ToString());
        }

        private static bool TryParseCharacter(char character, out char parsed)
        {
            parsed = character;
            if (character == ' ') return true;
            if (character is >= 'A' and <= 'Z') return true;
            if (character is >= 'a' and <= 'z')
            {
                parsed = (char)(parsed + 'A' - 'a');
                return true;
            }

            if (character is >= '0' and <= '9') return true;

            return false;
        }
    }
}