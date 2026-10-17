using System;
using System.Collections.Generic;
using UnityEngine;

namespace LD60.Inputs
{
    public class CommandExecutor : MonoBehaviour
    {
        [SerializeField] private InputParser _inputParser;

        private readonly List<ExecutedCommand> _lastCommands = new();

        public IReadOnlyList<ExecutedCommand> LastCommands => _lastCommands;

        private static readonly Dictionary<string, Action> SubscribedCommands = new();

        public event Action<ExecutedCommand> CommandHandled;
        public event Action Cleared;

        public static void SubscribeCommandHandler(string command, Action callback)
        {
            if (!SubscribedCommands.TryAdd(command, callback))
            {
                Debug.LogWarning($"Cannot subscribe command. Command was already subscribed: {command}");
            }
        }

        public static void UnsubscribeCommandHandler(string command)
        {
            if (!SubscribedCommands.Remove(command))
            {
                Debug.LogWarning($"Cannot unsubscribe command. Command was not subscribed: {command}");
            }
        }

        private void OnEnable() => _inputParser.InputCommitted += HandleInputCommitted;
        private void OnDisable() => _inputParser.InputCommitted -= HandleInputCommitted;
        private void OnDestroy() => SubscribedCommands.Clear();

        private void HandleInputCommitted(string input)
        {
            var commandExists = SubscribedCommands.TryGetValue(input, out var callback);
            var executedCommand = new ExecutedCommand(input, commandExists);

            _lastCommands.Add(executedCommand);

            if (commandExists)
            {
                callback?.Invoke();
            }

            CommandHandled?.Invoke(executedCommand);
        }

        public void Clear()
        {
            _lastCommands.Clear();
            Cleared?.Invoke();
        }

        public readonly struct ExecutedCommand
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