using System;
using UnityEngine;

namespace LD60.Objectives
{
    public class ObjectiveManager : MonoBehaviour
    {
        private IObjectiveHolder _currentObjectiveHolder;

        public IObjectiveHolder CurrentObjectiveHolder
        {
            get => _currentObjectiveHolder;
            set
            {
                if (_currentObjectiveHolder == value) return;

                var previousObjectiveHolder = _currentObjectiveHolder;

                _currentObjectiveHolder = value;

                OnObjectiveHolderChanged?.Invoke((previousObjectiveHolder, _currentObjectiveHolder));
            }
        }

        public event Action<(IObjectiveHolder previous, IObjectiveHolder newHolder)> OnObjectiveHolderChanged;
    }
}