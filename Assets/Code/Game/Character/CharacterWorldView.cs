using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using TriInspector;
#endif

namespace Game.Character
{
    public class CharacterWorldView : MonoBehaviour
    {
        [field: SerializeField] public CharacterAnimation Animation { get; private set; }
        
        [SerializeField] private SpriteRenderer[] _renderers;
        
        [Header("Markers")]
        [SerializeField] private GameObject _turnMarker;
        [SerializeField] private GameObject _targetMarker;

        
        private void OnEnable()
        {
            Animation.PlayIdle();
        }

        public void SetPosition(Vector3 position, ETeamId team)
        {
            transform.position = position;
            
            float scaleX = team == ETeamId.Player ? 1 : -1;
            transform.localScale = new Vector3(scaleX,1,1);
        }

        public void SetBodyColor(Color color)
        {
            foreach (SpriteRenderer renderer in _renderers)
            {
                renderer.color = color;
            }
        }

        public void ChangeTurnView(bool isActive)
        {
            _turnMarker.SetActive(isActive);
        }

        public void ChangeTargetView(bool isActive)
        {
            _targetMarker.SetActive(isActive);
        }
        
#if UNITY_EDITOR
        
        public class CharacterGizmoModel
        {
            public string Name;
            public int Slot;
        }
        
        [SerializeField, Group("gizmo"), GUIColor(0.75f, 0.75f, 0.3f, 0.9f)]
        private bool _isDrawGizmo;
        
        [SerializeField, Group("gizmo"), GUIColor(0.75f, 0.75f, 0.3f, 0.9f)]
        private Font _font;
        
        [SerializeField, Group("gizmo"), GUIColor(0.75f, 0.75f, 0.3f, 0.9f)]
        private Color _gizmoColor;
        
        private CharacterGizmoModel _gizmoModel;

        
        public void SetGizmoModel(CharacterGizmoModel model)
        {
            _gizmoModel = model;
        }
        
        private void OnDrawGizmos()
        {
            if (!_isDrawGizmo || _gizmoModel == null)
            {
                return;
            }
            
            /*GUIStyle labelStyle = new()
            {
                alignment = TextAnchor.MiddleCenter,
                font = _font,
                fontSize = 15,
                fontStyle = FontStyle.Italic,
                normal = { textColor = _gizmoColor },
            };
            
            
            Handles.Label(transform.position + Vector3.up, $"name: {_gizmoModel.Name}\nslot: {_gizmoModel.Slot}", labelStyle);*/

            string text = $"name: {_gizmoModel.Name}\nslot: {_gizmoModel.Slot}";
            Handles.color = _gizmoColor;
            Handles.Label(transform.position + Vector3.up * 0.5f, text);
        }
#endif
    }
}