using System.Text;
using TMPro;
using UnityEngine;

namespace LD60.Inputs.Renderers
{
    public class CommandStackRenderer : MonoBehaviour
    {
        [SerializeField] private CommandExecutor _commandExecutor;
        [SerializeField] private TMP_Text _commandsText;
        [SerializeField] private Color _validCommandColor;
        [SerializeField] private Color _invalidCommandColor;

        private void OnEnable()
        {
            Refresh();
            _commandExecutor.CommandHandled += HandleCommandHandled;
            _commandExecutor.Cleared += HandleCleared;
        }

        private void OnDisable()
        {
            _commandExecutor.CommandHandled -= HandleCommandHandled;
            _commandExecutor.Cleared -= HandleCleared;
        }

        private void HandleCommandHandled(CommandExecutor.ExecutedCommand obj) => Refresh();
        private void HandleCleared() => Refresh();

        private void Refresh()
        {
            StringBuilder builder = new();

            foreach (var executedCommand in _commandExecutor.LastCommands)
            {
                builder.Append($"<#{ColorUtility.ToHtmlStringRGB(executedCommand.Valid ? _validCommandColor : _invalidCommandColor)}>");
                builder.Append($"> {executedCommand.Command}");
                builder.Append(" </color>");
                builder.AppendLine();
            }

            _commandsText.text = builder.ToString();
        }
    }
}