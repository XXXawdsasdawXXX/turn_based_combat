using System;
using System.Collections.Generic;
using Code.Core;
using R3;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using TriInspector;
#endif

namespace Game.Character
{
    public class CharacterWorldView : MonoBehaviour, IPoolEntity
    {
        [field: SerializeField] public CharacterAnimation Animation { get; private set; }
        
        [SerializeField] private SpriteRenderer[] _renderers;
        
        [Header("Markers")]
        [SerializeField] private GameObject _turnMarker;
        [SerializeField] private GameObject _targetMarker;
        [SerializeField] private GameObject _observedMarker;

        private readonly List<IDisposable> _subscriptions = new();
        
        private CharacterState _state;

        
        public void Enable()
        {
            _turnMarker.SetActive(false);
            _targetMarker.SetActive(false);
            _observedMarker.SetActive(false);
            gameObject.SetActive(true);
            Animation.PlayIdle();
        }

        public void Disable()
        {
            Animation.Dispose();
            _dispose();
            gameObject.SetActive(false);
        }
        
        private void OnMouseEnter()
        {
            _state?.SetObservedState(true);
        }

        private void OnMouseDown()
        {
            _state?.SetTargetState(true);
            _state?.SetObservedState(false);
        }
        
        private void OnMouseExit()
        {
            _state?.SetObservedState(false);
        }
        
        public void SetActor(CharacterActor actor)
        {
            _dispose();
            
            _state = actor.State;

            _setBodyColor(actor.Configuration.Color);

            _subscriptions.Add(_state.IsTurn.DistinctUntilChanged().Subscribe(_changeTurnView));
            _subscriptions.Add(_state.IsTarget.DistinctUntilChanged().Subscribe(_changeTargetView));
            _subscriptions.Add(_state.IsObserved.DistinctUntilChanged().Subscribe(_changeObservedView));
        }

        public void SetPosition(Vector3 position, ETeamId team)
        {
            transform.position = position;
            
            float scaleX = team == ETeamId.Player ? 1 : -1;
            transform.localScale = new Vector3(scaleX,1,1);
        }

        private void _dispose()
        {
            if (_subscriptions.Count == 0)
            {
                return;
            }

            foreach (IDisposable disposable in _subscriptions)
            {
                disposable?.Dispose();
            }
            
            _subscriptions.Clear();

            _state = null;
        }

        private void _setBodyColor(Color color)
        {
            foreach (SpriteRenderer renderer in _renderers)
            {
                renderer.color = color;
            }
        }

        private void _changeTurnView(bool isActive)
        {
            _turnMarker.SetActive(isActive);
        }

        private void _changeTargetView(bool isActive)
        {
            _targetMarker.SetActive(isActive);
        }
        
        private void _changeObservedView(bool isActive)
        {
            _observedMarker.SetActive(isActive);
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
         
            string text = $"name: {_gizmoModel.Name}\nslot: {_gizmoModel.Slot}";
            Handles.color = _gizmoColor;
            Handles.Label(transform.position + Vector3.up * 0.5f, text);
        }
#endif
    }
}