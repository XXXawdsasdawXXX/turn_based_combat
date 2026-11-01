using System;
using Game.Abilities;
using Game.Character;
using R3;

namespace Game.Runtime
{
    public class TurnState : IDisposable
    {
        public ReadOnlyReactiveProperty<CharacterActor> Actor => _actor;
        private readonly ReactiveProperty<CharacterActor> _actor = new(null);

        public ReadOnlyReactiveProperty<CharacterActor> Target => _target;
        private readonly ReactiveProperty<CharacterActor> _target = new(null);

        public ReadOnlyReactiveProperty<Ability> Ability => _ability;
        private readonly ReactiveProperty<Ability> _ability = new(null);


        public void SetActor(CharacterActor actor)
        {
            _actor.Value = actor;
            _actor.Value?.State.SetTurnState(true);
        }

        public void SetTarget(CharacterActor target)
        {
            _target.Value = target;
        }

        public void SetAbility(Ability ability)
        {
            _ability.Value = ability;
        }
        
        public bool IsPlayerTurn()
        {
            return _actor.Value != null && _actor.Value.State.Team == ETeamId.Player;
        }

        public void ResetState()
        {
            _actor.Value.State.SetTurnState(false);
            _actor.Value = null;
            _target.Value = null;
            _ability.Value = null;
        }

        public void Dispose()
        {
            _actor.Dispose();
            _target.Dispose();
            _ability.Dispose();
        }
    }
}