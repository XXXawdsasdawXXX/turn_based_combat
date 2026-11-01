using System;
using Code.Core;
using Game.Abilities;
using Game.Character;
using Game.Runtime;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class ActiveActorView : MonoBehaviour
    {
        [field: SerializeField] public Button SkipButton { get; private set; }

        [SerializeField] private GameObject _locker;
        [SerializeField] private MonoPool<AbilityView> _abilitiesPool;

        private BattleState _state;
        private AbilityView _current;


        public void SubscribeToBattleState(BattleState state)
        {
            _state = state;
            
            state.Turn.Actor.DistinctUntilChanged().Subscribe(_setActor);
            state.Turn.Ability.DistinctUntilChanged().Subscribe(_ =>
            {
                _setInteractable(state.Turn.IsPlayerTurn() && state.Turn.Target.CurrentValue == null);
            });
            
            SkipButton.onClick.AddListener(_skip);
        }

        public void Dispose()
        {
            SkipButton.onClick.RemoveListener(_skip);
        }

        private void _setActor(CharacterActor actor)
        {
            _abilitiesPool.DisableAll();

            if (actor == null)
            {
                return;
            }
            
            for (int i = 0; i < actor.Configuration.Abilities.Length; i++)
            {
                Ability ability = actor.Configuration.Abilities[i];
                AbilityView abilityView = _abilitiesPool.GetNext();
                abilityView.transform.SetSiblingIndex(0);
                abilityView.SetAbility(ability);

                bool isActive = ActionResolver.CanUse(
                        _state.Turn.Actor.CurrentValue, 
                        ability, 
                        _state.GetAllies());
                
                abilityView.SetInteractable(isActive);
                
                int index = i;
                abilityView.Clicked += () =>
                {
                    if (_current != null)
                    {
                        _current.SetSelectedState(false);
                    }
              
                    _current = abilityView;
                    _current.SetSelectedState(true);
                    
                    _state.Turn.SetAbility(actor.Configuration.Abilities[index]);
                };
            }
            
            _setInteractable(actor.State.Team == ETeamId.Player);
        }

        private void _skip()
        {
            _state.Turn.SetAbility(Ability.Skip());
        }

        private void _setInteractable(bool isInteractable)
        {
            _locker.SetActive(!isInteractable);
        }
    }
}