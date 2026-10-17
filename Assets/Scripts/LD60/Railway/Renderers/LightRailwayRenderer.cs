using Nrjwolf.Tools.AttachAttributes;
using UnityEngine;

namespace LD60.Railway.Renderers
{
    public class LightRailwayRenderer : MonoBehaviour
    {
        [SerializeField, GetComponentInParent] private LightRailwayComponent _light;
        [SerializeField] private GameObject _onObject;
        [SerializeField] private GameObject _offObject;

        private void OnEnable()
        {
            Refresh();
            _light.TurnedOnChanged += HandleSwitchTurnedOnChanged;
        }

        private void OnDisable()
        {
            _light.TurnedOnChanged -= HandleSwitchTurnedOnChanged;
        }

        private void HandleSwitchTurnedOnChanged(bool on) => Refresh();

        private void Refresh()
        {
            _onObject.SetActive(_light.TurnedOn);
            _offObject.SetActive(!_light.TurnedOn);
        }
    }
}