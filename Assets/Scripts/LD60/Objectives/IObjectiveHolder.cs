using System;

namespace LD60.Objectives
{
    public interface IObjectiveHolder
    {
        event Action ObjectivesStateChanged;

        string GetObjectivesText();
        public string GetTipText();
    }
}