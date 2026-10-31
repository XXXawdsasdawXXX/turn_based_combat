using System;
using System.Collections.Generic;
using Code.Core;
using Game.Character;
using Game.Configurations;
using Game.Stats;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.UI
{
    public class UICharacterView : MonoBehaviour, IPoolEntity, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private TMP_Text _text_Name;
        [SerializeField] private TMP_Text _text_Health;
        [SerializeField] private Image _imageCharacter;
        
        [SerializeField] private Image _markerActor;
        [SerializeField] private Image _markerTarget;
        [SerializeField] private Image _markerObserved;

        private readonly List<IDisposable> _subscriptions = new();

        private CharacterState _state; 
            
        
        public void Enable()
        {
            _changeSelectMarker(false);
            
            gameObject.SetActive(true);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _state.SetObservedState(true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _state.SetTargetState(true);
            _state.SetObservedState(false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _state.SetObservedState(false);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
            
            _dispose();
        }
        
        public void SetActor(CharacterActor actor)
        {
            _dispose();

            _state = actor.State;
            
            RuntimeStat healthStat = _state.Stats[EStatType.Health]; 
            _subscriptions.Add(healthStat.Current
                .DistinctUntilChanged()
                .Subscribe(vale =>
                {
                    _updateHealth($"{Mathf.Round(vale)}/{Mathf.Round(healthStat.BaseValue)}");
                }));
            _updateHealth($"{Mathf.Round(healthStat.Current.Value)}/{Mathf.Round(healthStat.BaseValue)}");

            _subscriptions.Add(_state.IsTurn
                .DistinctUntilChanged()
                .Subscribe(_changeSelectMarker));
            
            _subscriptions.Add(_state.IsTarget
                .DistinctUntilChanged()
                .Subscribe(_changeTargetMarker));
            
            _subscriptions.Add(_state.IsObserved
                .DistinctUntilChanged()
                .Subscribe(_changeObservedMarker));
            
            CharacterConfiguration configuration = actor.Configuration;
            
            _text_Name.SetText(configuration.Name);
            _imageCharacter.color = configuration.Color;
        }

        private void _updateHealth(string text)
        {
            _text_Health.SetText(text);
        }

        private void _changeSelectMarker(bool isActive)
        {
            _markerActor.gameObject.SetActive(isActive);
        }
        
        private void _changeTargetMarker(bool isActive)
        {
            _markerTarget.gameObject.SetActive(isActive);
        }
        
        private void _changeObservedMarker(bool isActive)
        {
            _markerObserved.gameObject.SetActive(isActive);
        }
        
        private void _dispose()
        {
            foreach (IDisposable subscription in _subscriptions)
            {
                subscription?.Dispose();
            }

            _subscriptions?.Clear();
        }
    }
}