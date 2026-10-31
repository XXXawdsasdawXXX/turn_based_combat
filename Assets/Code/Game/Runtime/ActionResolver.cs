using System.Collections.Generic;
using System.Linq;
using Game.Abilities;
using Game.Character;
using Game.Stats;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Runtime
{
    public static class ActionResolver
    {
        public static CharacterActor GetFrontline(IEnumerable<CharacterActor> team)
        {
            foreach (CharacterActor actor in team)
            {
                if (actor.State.IsAlive())
                {
                    return actor;
                }
            }

            return null;
        }

        public static bool CanUse(
            CharacterActor actor,
            Ability ability,
            IReadOnlyList<CharacterActor> allies)
        {
            if (ability == null)
            {
                return false;
            }
            
            switch (ability.Type)
            {
                case EAbilityType.Skip:
                    return true;
                
                case EAbilityType.MeleeAttack:
                    return !allies.Any(a => a.State.IsAlive() && a.State.Slot < actor.State.Slot);
                
                case EAbilityType.RangeAttack:
                    return true;
              
                case EAbilityType.Heal:
                    foreach (CharacterActor ally in allies)
                    {
                        if (ally.State.IsAlive())
                        {
                            RuntimeStat healthStat = ally.State.Stats[EStatType.Health];
                            return healthStat.Current.CurrentValue < healthStat.BaseValue;
                        }
                    }
                    return false;
                
                default:
                    return false;
            }
        }

        public static IReadOnlyList<CharacterActor> GetValidTargets(
            Ability ability,
            IReadOnlyList<CharacterActor> allies,
            IReadOnlyList<CharacterActor> enemies)
        {
            if (ability == null)
            {
                return null;
            }
            
            switch (ability.Type)
            {
                default:
                case EAbilityType.Skip:
                    return null;

                case EAbilityType.MeleeAttack:
                    foreach (CharacterActor enemy in enemies)
                    {
                        if (enemy.State.IsAlive())
                        {
                            return new []{enemy};
                        }
                    }

                    return null;
                    
                case EAbilityType.RangeAttack:
                    List<CharacterActor> rangeTargets = new();
                    rangeTargets.AddRange(enemies.Where(e => e.State.IsAlive()));
                    return rangeTargets;

                case EAbilityType.Heal:
                    List<CharacterActor> healTargets = new();
                    healTargets.AddRange(allies.Where(e => 
                        e.State.IsAlive() && !e.State.Stats[EStatType.Health].IsFull()));
                    return healTargets;
            }
        }

        public static ActionOutcome Resolve(
            CharacterActor actor,
            Ability ability,
            CharacterActor target,
            IReadOnlyList<CharacterActor> allies,
            IReadOnlyList<CharacterActor> enemies)
        {
            bool IsHit()
            {
                float attack = actor.State.Stats[EStatType.Attack].Current.CurrentValue;
                float defence = target.State.Stats[EStatType.Defence].Current.CurrentValue;
                float chance = Mathf.Clamp(attack / (attack + defence), 0.05f, 0.95f);
                return Random.value <= chance;
            }
            
            switch (ability.Type)
            {
                case EAbilityType.Skip:
                default:
                    return ActionOutcome.Skip();
                    
                case EAbilityType.MeleeAttack:
                    return new ActionOutcome(
                        IsHit() ? EActionOutcomeKind.Hit : EActionOutcomeKind.Miss,
                        ability.Effect.BaseValue,
                        ability.Effect.Type,
                        target);
                
                case EAbilityType.RangeAttack:
                    bool isFrontlineActor = actor.State.Slot == GetFrontline(allies).State.Slot;
                    float damage = ability.Effect.BaseValue * (isFrontlineActor ? 0.75f : 1);
                    return new ActionOutcome(
                        IsHit() ? EActionOutcomeKind.Hit : EActionOutcomeKind.Miss,
                        damage,
                        ability.Effect.Type,
                        target);
                    
                case EAbilityType.Heal:
                    return new ActionOutcome(
                        EActionOutcomeKind.Heal,
                        ability.Effect.BaseValue,
                        ability.Effect.Type,
                        target);
            }
        }
    }

    public enum EActionOutcomeKind
    {
        Skip,
        Miss,
        Hit,
        Heal
    }
    
    public struct ActionOutcome
    {
        public EActionOutcomeKind Kind { get; } 
        public float StatDelta { get; }         
        public EStatType StatType { get; }
        public CharacterActor Target { get; }


        public ActionOutcome(EActionOutcomeKind kind, float statDelta, EStatType statType, CharacterActor target)
        {
            Kind = kind;
            StatDelta = statDelta;
            StatType = statType;
            Target = target;
        }
        
        public static ActionOutcome Skip()
        {
            return new ActionOutcome(EActionOutcomeKind.Skip, 0, EStatType.None, null);
        }
    }
}