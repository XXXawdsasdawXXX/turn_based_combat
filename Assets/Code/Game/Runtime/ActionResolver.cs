using System.Collections.Generic;
using System.Linq;
using Game.Abilities;
using Game.Character;
using Game.Stats;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Runtime
{
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
    
    public static class ActionResolver
    {
        public static bool CanUse(CharacterActor actor, Ability ability, IEnumerable<CharacterActor> allies)
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
                        if (ally.State.IsAlive() && !ally.State.Stats[EStatType.Health].IsFull())
                        {
                            return true;
                        }
                    }
                    return false;
                
                default:
                    return false;
            }
        }

        public static IReadOnlyList<CharacterActor> GetValidTargets(
            Ability ability,
            IEnumerable<CharacterActor> allies,
            IEnumerable<CharacterActor> enemies)
        {
            List<CharacterActor> targets = new();
            
            switch (ability.Type)
            {
                default:
                case EAbilityType.Skip:
                    break;

                case EAbilityType.MeleeAttack:
                    CharacterActor frontlineEnemy = _getFrontline(enemies);
                    if (frontlineEnemy != null)
                    {
                        targets.Add(frontlineEnemy);
                    }
                    break;
                    
                case EAbilityType.RangeAttack:
                    targets.AddRange(enemies.Where(e => e.State.IsAlive()));
                    break;

                case EAbilityType.Heal:
                    targets.AddRange(allies.Where(e => 
                        e.State.IsAlive() && !e.State.Stats[EStatType.Health].IsFull()));
                    break;
            }
            
            return targets;
        }

        public static ActionOutcome Resolve(
            CharacterActor actor,
            Ability ability,
            CharacterActor target,
            IEnumerable<CharacterActor> allies)
        {
            bool IsHit()
            {
                float attack = actor.State.Stats[EStatType.Attack].Current.CurrentValue;
                float defence = target.State.Stats[EStatType.Defence].Current.CurrentValue;
                float chance = Mathf.Clamp(attack / (attack + defence), 0.05f, 0.95f);
                if (ability.Type is EAbilityType.RangeAttack)
                {
                    bool isFrontlineActor = actor.State.Slot == _getFrontline(allies).State.Slot;
                    if (isFrontlineActor)
                    {
                        chance *= 0.75f;
                    }
                }
                return Random.value <= chance;
            }
            
            switch (ability.Type)
            {
                case EAbilityType.Skip:
                default:
                    return ActionOutcome.Skip();
                    
                case EAbilityType.MeleeAttack:
                    bool isHitMelee = IsHit();
                    return new ActionOutcome(
                        isHitMelee ? EActionOutcomeKind.Hit : EActionOutcomeKind.Miss,
                        isHitMelee ? ability.Effect.BaseValue : 0,
                        ability.Effect.Type,
                        target);
                
                case EAbilityType.RangeAttack:
                    bool isHitRange = IsHit();
                    return new ActionOutcome(
                        isHitRange ? EActionOutcomeKind.Hit : EActionOutcomeKind.Miss,
                        isHitRange ? ability.Effect.BaseValue : 0,
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

        private static CharacterActor _getFrontline(IEnumerable<CharacterActor> team)
        {
            return team.FirstOrDefault(actor => actor.State.IsAlive());
        }
    }
}