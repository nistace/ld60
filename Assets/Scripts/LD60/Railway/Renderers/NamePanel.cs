using Nrjwolf.Tools.AttachAttributes;
using TMPro;
using UnityEngine;

namespace LD60.Railway.Renderers
{
    [RequireComponent(typeof(TMP_Text))]
    public class NamePanel : MonoBehaviour
    {
        [SerializeField, GetComponent] private TMP_Text _text;

        private void Awake() => Refresh();

        [ContextMenu("Refresh")]
        private void Refresh()
        {
            var panelNameItem = GetComponentInParent<IPanelNameItem>();

            _text.text = panelNameItem?.NameOnPanel ?? "";
        }

        private void OnValidate() => Refresh();
    }
}