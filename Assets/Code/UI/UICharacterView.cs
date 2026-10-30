using System;
using System.Collections.Generic;
using Code.Core;
using Game.Character;
using Game.Configurations;
using Game.Stats;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class UICharacterView : MonoBehaviour, IPoolEntity
    {
        [SerializeField] private TMP_Text _text_Name;
        [SerializeField] private TMP_Text _text_Health;
        [SerializeField] private Image _imageCharacter;
        [SerializeField] private Image _imageMarker;

        private readonly List<IDisposable> _subscriptions = new(); 

        
        public void Enable()
        {
            _changeSelectMarker(false);
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
            
            _dispose();
        }
        
        public void SetActor(CharacterActor actor)
        {
            _dispose();

            RuntimeStat healthStat = actor.State.Stats[EStatType.Health]; 
            _subscriptions.Add(healthStat.Current
                .DistinctUntilChanged()
                .Subscribe(vale =>
                {
                    _updateHealth($"{Mathf.Round(vale)}/{Mathf.Round(healthStat.BaseValue)}");
                }));
            _updateHealth($"{Mathf.Round(healthStat.Current.Value)}/{Mathf.Round(healthStat.BaseValue)}");

            _subscriptions.Add(actor.State.IsTurn
                .DistinctUntilChanged()
                .Subscribe(_changeSelectMarker));
            
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
            _imageMarker.gameObject.SetActive(isActive);
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