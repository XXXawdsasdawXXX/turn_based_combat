using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Character;
using Game.Stats;

namespace Game.Runtime
{
    public static class ActionRunner
    {
        public static async UniTask StartAction(CharacterActor actor, ActionOutcome outcome)
        {
            if (outcome.Kind is EActionOutcomeKind.Skip)
            {
                return;
            }
            
            await actor.WorldView.Animation.PlayUseAbility();
            actor.WorldView.Animation.PlayIdle();
            
            if (outcome.Kind is EActionOutcomeKind.Miss)
            {
                return;
            }
            
            List<UniTask> _impacts = new();

            if (outcome.Target.State.Stats.TryGetValue(outcome.StatType, out RuntimeStat targetStat))
            {
                targetStat.SetCurrent(targetStat.Current.CurrentValue + outcome.StatDelta);
            }
            
            if (outcome.Kind is EActionOutcomeKind.Hit)
            {
                await outcome.Target.WorldView.Animation.PlayTakeDamage();

                if (outcome.Target.State.IsAlive())
                {
                    outcome.Target.WorldView.Animation.PlayIdle();
                }
                else
                {
                    await outcome.Target.WorldView.Animation.PlayDie();
                }
            }
        }
    }
}