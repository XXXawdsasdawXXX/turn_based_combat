using System.Collections.Generic;
using System.Linq;
using Game.Character;
using R3;

namespace Game.Runtime
{
    public class BattleState
    {
        public ReactiveProperty<int> Round { get; } = new(0);
        public ReactiveProperty<CharacterActor> CurrentActor { get; } = new(null);
        
        public readonly List<CharacterActor> PlayerTeam = new();
        public readonly List<CharacterActor> EnemyTeam = new();
       
        
        public void SetCurrentActor(CharacterActor actor) => CurrentActor.Value = actor;
       
        public void ResetCurrentActor() => CurrentActor.Value = null;
       
        public void MoveToNextRound() => Round.Value++;
        
        public bool BattleEnd(out ETeamId winner)
        {
            bool teamHasAliveActor(ETeamId team)
            {
                return team is ETeamId.Player
                    ? PlayerTeam.Any(a => a.State.IsAlive())
                    : EnemyTeam.Any(a => a.State.IsAlive());
            }
            
            winner = ETeamId.Player;
         
            if (!teamHasAliveActor(ETeamId.Enemy))
            {
                return true;
            }

            if(!teamHasAliveActor(ETeamId.Player))
            {
                winner = ETeamId.Enemy;
                return true;
            }

            return false;
        }
    }
}