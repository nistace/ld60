using UnityEngine;

namespace LD60.Trains
{
    [CreateAssetMenu]
    public class TrainIdentity : ScriptableObject
    {
        [SerializeField] private string _nameOnPanel;
        [SerializeField] private Color _color = new(.3f, .4f, .7f, 1);

        public string NameOnPanel => _nameOnPanel;
        public Color Color => _color;
    }
}