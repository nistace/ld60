using System;
using System.Collections.Generic;
using System.Text;
using LD60.Objectives;
using UnityEngine;

namespace LD60.Games
{
    [Serializable]
    public class LoginObjectiveHolder : IObjectiveHolder
    {
        [SerializeField] private string _text = "Hello. Thank you for trying the <i>Train-o-matic 3000</i>. To continue, type <b>LOGIN <user name></b>.";
        [SerializeField] private string _tip = "Simply type <b>LOGIN</b> if you don't want to submit your scores to the leaderboard.";
        [SerializeField] private string _command = "LOGIN";

        public List<string> _additions = new();

        public event Action ObjectivesStateChanged;
        public string Command => _command;

        public string GetObjectivesText()
        {
            var builder = new StringBuilder();
            builder.AppendLine(_text);
            foreach (var addition in _additions)
            {
                builder.AppendLine(addition);
            }

            return builder.ToString();
        }

        public string GetTipText() => _tip;

        public void Reset()
        {
            _additions.Clear();

            ObjectivesStateChanged?.Invoke();
        }

        public void Add(string message)
        {
            _additions.Add(message);

            ObjectivesStateChanged?.Invoke();
        }
    }
}