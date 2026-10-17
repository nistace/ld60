using Nrjwolf.Tools.AttachAttributes;
using UnityEngine;

namespace LD60.Railway.Renderers
{
    public class SwitchRailwayRenderer : MonoBehaviour
    {
        [SerializeField, GetComponentInParent] private SwitchRailwayComponent _switch;
        [SerializeField] private GameObject _onObject;
        [SerializeField] private GameObject _offObject;

        private void OnEnable()
        {
            Refresh();
            _switch.TurnedOnChanged += HandleSwitchTurnedOnChanged;
        }

        private void OnDisable()
        {
            _switch.TurnedOnChanged -= HandleSwitchTurnedOnChanged;
        }

        private void HandleSwitchTurnedOnChanged(bool on) => Refresh();

        private void Refresh()
        {
            _onObject.SetActive(_switch.TurnedOn);
            _offObject.SetActive(!_switch.TurnedOn);
        }
    }
}