using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Abilities;
using Game.Character;

namespace Game.Runtime
{
    public sealed class PlayerTurnDecider : ITurnDecider
    {
        public async UniTask<TurnChoice> DecideAsync(
            CharacterActor actor,
            BattleState state,
            CancellationToken token)
        {
            while (state.Turn.Target.CurrentValue == null)
            {
                await UniTask.WaitUntil(
                    () => state.Turn.Ability.CurrentValue != null,
                    cancellationToken: token);

                Ability chosen = state.Turn.Ability.CurrentValue;

                if (chosen.Type is EAbilityType.Skip)
                {
                    return TurnChoice.Skip();
                }

                state.SyncChoiceTargetMode();

                await UniTask.WaitUntil(
                    () => state.Turn.Target.CurrentValue != null
                          || state.Turn.Ability.CurrentValue != chosen,
                    cancellationToken: token);
            }

            return new TurnChoice(
                state.Turn.Ability.CurrentValue,
                state.Turn.Target.CurrentValue);
        }
    }
}
