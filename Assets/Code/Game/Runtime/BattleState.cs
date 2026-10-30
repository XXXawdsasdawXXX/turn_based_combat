using System.Collections.Generic;
using System.Linq;
using Game.Abilities;
using Game.Character;
using R3;

namespace Game.Runtime
{
    public class BattleState
    {
        public ReactiveProperty<EBattlePhase> Phase { get; } = new(EBattlePhase.NotStarted);
        public List<CharacterActor> PlayerTeam { get; } = new();
        public List<CharacterActor> EnemyTeam { get; } = new();
        public ReactiveProperty<int> Round { get; } = new(0);
        public ReactiveProperty<CharacterActor> TurnActor { get; } = new(null);
        public ReactiveProperty<CharacterActor> TurnTarget { get; } = new(null);
        
        public ReactiveProperty<Ability> TurnAbility { get; } = new(null);
        

        public bool IsBattleEnd(out ETeamId winner)
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

        public void ResetTurn()
        {
            TurnActor.Value = null;
            TurnTarget.Value = null;
            TurnAbility.Value = null;
        }
    }
}