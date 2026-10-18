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
        private static readonly Dictionary<string, Action<string>> SubscribedCommandsWithParameters = new();

        public event Action<ExecutedCommand> CommandHandled;
        public event Action Cleared;

        public static void SubscribeCommandHandler(string command, Action callback)
        {
            if (!SubscribedCommands.TryAdd(command, callback))
            {
                Debug.LogWarning($"Cannot subscribe command. Command was already subscribed: {command}");
            }
        }

        public static void SubscribeCommandHandler(string command, Action<string> callback)
        {
            if (!SubscribedCommandsWithParameters.TryAdd(command, callback))
            {
                Debug.LogWarning($"Cannot subscribe command. Command was already subscribed: {command}");
            }
        }

        public static void UnsubscribeCommandHandler(string command)
        {
            SubscribedCommands.Remove(command);
            SubscribedCommandsWithParameters.Remove(command);
        }

        private void OnEnable() => _inputParser.InputCommitted += HandleInputCommitted;
        private void OnDisable() => _inputParser.InputCommitted -= HandleInputCommitted;
        private void OnDestroy() => SubscribedCommands.Clear();

        private void HandleInputCommitted(string input)
        {
            ExecutedCommand executedCommand;

            if (SubscribedCommands.TryGetValue(input, out var callback))
            {
                executedCommand = new ExecutedCommand(input, true);

                _lastCommands.Add(executedCommand);
                callback?.Invoke();
                CommandHandled?.Invoke(executedCommand);
                return;
            }

            foreach (var commandWithParameters in SubscribedCommandsWithParameters)
            {
                if (!input.StartsWith(commandWithParameters.Key)) continue;

                executedCommand = new ExecutedCommand(input, true);

                _lastCommands.Add(executedCommand);
                commandWithParameters.Value?.Invoke(input);
                CommandHandled?.Invoke(executedCommand);
                return;
            }

            executedCommand = new ExecutedCommand(input, false);
            _lastCommands.Add(executedCommand);
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