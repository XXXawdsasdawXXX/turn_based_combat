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

        private IDisposable _actorSubscription;
        private IDisposable _abilitySubscription;
        
        public void SubscribeToBattleState(BattleState state)
        {
            _actorSubscription = state.TurnActor.DistinctUntilChanged().Subscribe(_setActor);
            _abilitySubscription = state.TurnAbility.DistinctUntilChanged().Subscribe(ability =>
            {
                _setInteractable(state.TurnActor.Value.State.Team == ETeamId.Player && ability == null);
            });
        }

        public void DisposeBattleSubscription()
        {
            _actorSubscription?.Dispose();
            _actorSubscription = null;
            
            _abilitySubscription?.Dispose();
            _abilitySubscription = null;
        }
        
        private void _setActor(CharacterActor stateCurrentActor)
        {
            _abilitiesPool.DisableAll();
            
            foreach (Ability ability in stateCurrentActor.Configuration.Abilities)
            {
                AbilityView abilityView = _abilitiesPool.GetNext();
                abilityView.transform.SetSiblingIndex(0);
                abilityView.SetAbility(ability);
                abilityView.Enable();
            }
        }

        private void _setInteractable(bool isInteractable)
        {
            _locker.SetActive(!isInteractable);
        }
    }
}