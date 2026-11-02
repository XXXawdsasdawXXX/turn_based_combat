using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Character;
using Game.Stats;

namespace Game.Runtime
{
    public static class ActionRunner
    {
        public static async UniTask StartAction(
            CharacterActor actor,
            ActionOutcome outcome,
            CancellationToken token)
        {
            if (outcome.Kind is EActionOutcomeKind.Skip)
            {
                return;
            }
            
            await actor.WorldView.Animation.PlayUseAbility(token);
            actor.WorldView.Animation.PlayIdle();
            
            if (outcome.Kind is EActionOutcomeKind.Miss)
            {
                return;
            }

            if (outcome.Target.State.Stats.TryGetValue(outcome.StatType, out RuntimeStat targetStat))
            {
                targetStat.SetCurrent(targetStat.Current.CurrentValue + outcome.StatDelta);
            }
            
            if (outcome.Kind is EActionOutcomeKind.Hit)
            {
                await outcome.Target.WorldView.Animation.PlayTakeDamage(token);

                if (outcome.Target.State.IsAlive())
                {
                    outcome.Target.WorldView.Animation.PlayIdle();
                }
                else
                {
                    await outcome.Target.WorldView.Animation.PlayDie(token);
                }
            }
        }
    }
}
