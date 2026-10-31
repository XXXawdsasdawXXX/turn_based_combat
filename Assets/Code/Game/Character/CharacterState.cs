using System;
using System.Collections.Generic;
using Game.Configurations;
using Game.Stats;
using R3;

namespace Game.Character
{
    public class CharacterState
    {
        public Dictionary<EStatType, RuntimeStat> Stats { get; }
        
        public ReadOnlyReactiveProperty<bool> IsTurn => _isTurn;
        private readonly ReactiveProperty<bool> _isTurn = new(false);
        
        public ReadOnlyReactiveProperty<bool> IsObserved => _isObserved;
        private readonly ReactiveProperty<bool> _isObserved = new(false);
        
        public ReadOnlyReactiveProperty<bool> IsTarget => _isTarget;
        private readonly ReactiveProperty<bool> _isTarget = new(false);
        
        public ETeamId Team { get; }
        public int Slot { get; }
        
        private bool _isCanBeTarget;
        
        
        public CharacterState(CharacterConfiguration configuration, ETeamId team, int slot)
        {
            Team = team;
            Slot = slot;
            
            Stats = new Dictionary<EStatType, RuntimeStat>();

            foreach (Stat stat in configuration.Stats)
            {
                if(!Stats.TryAdd(stat.Type, new RuntimeStat(stat.Type, stat.BaseValue)))
                {
                    throw new Exception($"character state already has stat {stat.Type}");
                }
            }
        }
        
        public void SetTargetState(bool isTarget)
        {
            _isTarget.Value = IsAlive() && _isCanBeTarget && isTarget;
        }
        
        public void SetObservedState(bool isObserved)
        {
            _isObserved.Value = IsAlive() && _isCanBeTarget && isObserved;
        }
        
        public void SetCanBeTarget(bool isCanBeTarget)
        {
            _isCanBeTarget = isCanBeTarget;
        }
        
        public void SetTurnState(bool isTurn)
        {
            _isTurn.Value = isTurn;
        }
        
        public bool IsAlive()
        {
            return Stats[EStatType.Health].Current.Value > 0;
        }

        public void Dispose()
        {
            _isTurn?.Dispose();
            _isObserved?.Dispose();
            _isTarget?.Dispose();
        }
    }
}