using System;
using UnityEngine;

namespace LD60.Inputs
{
    public class CommandHolder : MonoBehaviour
    {
        [SerializeField] private string _command;

        public event Action Executed;

        private void OnEnable() => CommandExecutor.SubscribeCommandHandler(_command, HandleCommandExecuted);
        private void OnDisable() => CommandExecutor.UnsubscribeCommandHandler(_command);

        private void HandleCommandExecuted() => Executed?.Invoke();

        private void OnValidate()
        {
            var upper = _command.ToUpper();
            if (_command == upper) return;

            _command = upper;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}