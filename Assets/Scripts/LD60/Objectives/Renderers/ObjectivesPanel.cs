using TMPro;
using UnityEngine;

namespace LD60.Objectives.Renderers
{
    public class ObjectivesPanel : MonoBehaviour
    {
        [SerializeField] private ObjectiveManager _objectiveManager;
        [SerializeField] private TMP_Text _objectivesText;
        [SerializeField] private TMP_Text _tipText;

        private void OnEnable()
        {
            Refresh();

            if (_objectiveManager.CurrentObjectiveHolder != null)
            {
                _objectiveManager.CurrentObjectiveHolder.ObjectivesStateChanged += HandleCurrentObjectiveHolderObjectivesChanged;
            }

            _objectiveManager.OnObjectiveHolderChanged += HandleObjectiveHolderChanged;
        }

        private void OnDisable()
        {
            _objectiveManager.OnObjectiveHolderChanged -= HandleObjectiveHolderChanged;

            if (_objectiveManager.CurrentObjectiveHolder != null)
            {
                _objectiveManager.CurrentObjectiveHolder.ObjectivesStateChanged -= HandleCurrentObjectiveHolderObjectivesChanged;
            }
        }

        private void HandleCurrentObjectiveHolderObjectivesChanged() => Refresh();

        private void HandleObjectiveHolderChanged((IObjectiveHolder previousHolder, IObjectiveHolder currentHolder) data)
        {
            if (data.previousHolder != null)
            {
                data.previousHolder.ObjectivesStateChanged -= HandleCurrentObjectiveHolderObjectivesChanged;
            }

            if (data.currentHolder != null)
            {
                data.currentHolder.ObjectivesStateChanged += HandleCurrentObjectiveHolderObjectivesChanged;
            }

            Refresh();
        }

        private void Refresh()
        {
            if (_objectiveManager.CurrentObjectiveHolder == null)
            {
                _objectivesText.text = string.Empty;
                _tipText.text = string.Empty;
                return;
            }

            _objectivesText.text = _objectiveManager.CurrentObjectiveHolder.GetObjectivesText();
            _tipText.text = _objectiveManager.CurrentObjectiveHolder.GetTipText();
        }
    }
}