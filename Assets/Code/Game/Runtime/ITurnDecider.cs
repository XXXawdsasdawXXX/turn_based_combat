using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Character;

namespace Game.Runtime
{
    public interface ITurnDecider
    {
        UniTask<TurnChoice> DecideAsync(
            CharacterActor actor,
            BattleState state,
            CancellationToken token);
    }
}
