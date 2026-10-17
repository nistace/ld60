using System;
using System.Collections.Generic;
using UnityEngine;

namespace LD60.Inputs
{
    public class CommandExecutor : MonoBehaviour
    {
        [SerializeField] private InputParser _inputParser;

        private List<ExecutedCommand> _lastCommands = new();
        private static Dictionary<string, Action> subscribedCommands = new();

        public static void SubscribeCommandHandler(string command, Action callback)
        {
            if (!subscribedCommands.TryAdd(command, callback))
            {
                Debug.LogWarning($"Cannot subscribe command. Command was already subscribed: {command}");
            }
        }

        public static void UnsubscribeCommandHandler(string command)
        {
            if (!subscribedCommands.Remove(command))
            {
                Debug.LogWarning($"Cannot unsubscribe command. Command was not subscribed: {command}");
            }
        }

        private void OnEnable() => _inputParser.InputCommitted += HandleInputCommitted;
        private void OnDisable() => _inputParser.InputCommitted -= HandleInputCommitted;
        private void OnDestroy() => subscribedCommands.Clear();

        private void HandleInputCommitted(string input)
        {
            if (!subscribedCommands.TryGetValue(input, out var callback))
            {
                _lastCommands.Add(new ExecutedCommand(input, false));
                return;
            }

            _lastCommands.Add(new ExecutedCommand(input, true));
            callback?.Invoke();
        }

        private readonly struct ExecutedCommand
        {
            public string Command { get; }
            public bool Valid { get; }

            public ExecutedCommand(string command, bool valid)
            {
                Command = command;
                Valid = valid;
            }
        }
    }
}