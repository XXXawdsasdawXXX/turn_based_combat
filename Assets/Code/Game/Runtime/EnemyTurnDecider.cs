using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Abilities;
using Game.Character;
using Game.Stats;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class EnemyTurnDecider : ITurnDecider
    {
        public UniTask<TurnChoice> DecideAsync(
            CharacterActor actor,
            BattleState state,
            CancellationToken token)
        {
            Ability[] abilities = actor.Configuration.Abilities;
            if (abilities == null || abilities.Length == 0)
            {
                return UniTask.FromResult(TurnChoice.Skip());
            }

            Ability ability = _choiceAbility(actor, state);
            state.Turn.SetAbility(ability);

            if (!ActionResolver.CanUse(actor, ability, state.GetAllies()))
            {
                state.Turn.SetAbility(Ability.Skip());
                return UniTask.FromResult(TurnChoice.Skip());
            }

            state.SyncChoiceTargetMode();

            IReadOnlyList<CharacterActor> validTargets = ActionResolver.GetValidTargets(
                ability,
                state.GetAllies(),
                state.GetEnemies());

            if (validTargets.Count == 0)
            {
                state.Turn.SetAbility(Ability.Skip());
                return UniTask.FromResult(TurnChoice.Skip());
            }

            CharacterActor target = validTargets[Random.Range(0, validTargets.Count)];
            state.Turn.SetTarget(target);

            return UniTask.FromResult(new TurnChoice(ability, target));
        }

        private Ability _choiceAbility(CharacterActor actor, BattleState state)
        {
            Ability[] abilities = actor.Configuration.Abilities;
            Ability ability;

            if (state.EnemyTeam.Any(a =>
                    a.State.Stats[EStatType.Health].Current.CurrentValue <
                    a.State.Stats[EStatType.Health].BaseValue * 0.3f))
            {
                ability = abilities.FirstOrDefault(a => a.Type == EAbilityType.Heal);
                
                if (ability != null)
                {
                    return ability;
                }
            }
            else if(ActionResolver.IsFrontlineCharacter(actor, state.EnemyTeam))
            {
                ability = abilities.FirstOrDefault(a => a.Type == EAbilityType.MeleeAttack);

                if (ability != null)
                {
                    return ability;
                }
            }
            else
            {
                ability = abilities.FirstOrDefault(a => a.Type == EAbilityType.RangeAttack);

                if (ability != null)
                {
                    return ability;
                }
            }

            return Ability.Skip();
        }
    }
}
