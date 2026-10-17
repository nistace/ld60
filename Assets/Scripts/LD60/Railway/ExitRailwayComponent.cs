using UnityEngine;

namespace LD60.Railway
{
    public class ExitRailwayComponent : MonoBehaviour, IPanelNameItem
    {
        [SerializeField] private string _nameOnPanel = "A";

        public string NameOnPanel => _nameOnPanel;
    }
}