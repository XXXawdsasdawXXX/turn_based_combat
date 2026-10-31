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
        
        private IDisposable _actorSubscription;
        private IDisposable _abilitySubscription;

        private AbilityView _current;


        public void SubscribeToBattleState(BattleState state)
        {
            _state = state;
            
            _actorSubscription = state.TurnActor.DistinctUntilChanged().Subscribe(_setActor);
            _abilitySubscription = state.TurnAbility.DistinctUntilChanged().Subscribe(ability =>
            {
                _setInteractable(state.TurnActor.Value.State.Team == ETeamId.Player && state.TurnTarget.Value == null);
            });
            
            SkipButton.onClick.AddListener(_skip);
        }

        public void DisposeBattleSubscription()
        {
            _actorSubscription?.Dispose();
            _actorSubscription = null;
            
            _abilitySubscription?.Dispose();
            _abilitySubscription = null;
           
            SkipButton.onClick.RemoveListener(_skip);
        }

        private void _setActor(CharacterActor actor)
        {
            _abilitiesPool.DisableAll();

            for (int i = 0; i < actor.Configuration.Abilities.Length; i++)
            {
                Ability ability = actor.Configuration.Abilities[i];
                AbilityView abilityView = _abilitiesPool.GetNext();
                abilityView.transform.SetSiblingIndex(0);
                abilityView.SetAbility(ability);

                bool isActive = ActionResolver.CanUse(
                        _state.TurnActor.Value, 
                        ability, 
                        _state.GetTeam(actor.State.Team));
                
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
                    
                    _state.TurnAbility.Value = actor.Configuration.Abilities[index];
                };
                
                abilityView.Enable();
            }
            
            _setInteractable(actor.State.Team == ETeamId.Player);
        }

        private void _skip()
        {
            _state.TurnAbility.Value = Ability.Skip();
        }

        private void _setInteractable(bool isInteractable)
        {
            _locker.SetActive(!isInteractable);
        }
    }
}