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


        public void DisableTargetMode()
        {
            foreach (CharacterActor actor in EnemyTeam)
            {
                actor.State.SetCanBeTarget(false);
            }    
            
            foreach (CharacterActor actor in PlayerTeam)
            {
                actor.State.SetCanBeTarget(false);
            }    
        }

        public void EnableTargetMode(IReadOnlyList<CharacterActor> actors)
        {
            foreach (CharacterActor actor in actors)
            {
                actor.State.SetCanBeTarget(true);
            }
        }

        public IReadOnlyList<CharacterActor> GetTeam(ETeamId team)
        {
            return team is ETeamId.Player ? PlayerTeam : EnemyTeam;
        }
        
        public bool IsBattleEnd(out ETeamId winner)
        {
            bool TeamHasAliveActor(ETeamId team)
            {
                return team is ETeamId.Player
                    ? PlayerTeam.Any(a => a.State.IsAlive())
                    : EnemyTeam.Any(a => a.State.IsAlive());
            }
            
            winner = ETeamId.Player;
         
            if (!TeamHasAliveActor(ETeamId.Enemy))
            {
                return true;
            }
            
            if (!TeamHasAliveActor(ETeamId.Player))
            {
                winner = ETeamId.Enemy;
                return true;
            }
            
            return false;
        }

        public void ResetTurnProperties()
        {
            TurnActor.Value = null;
            TurnTarget.Value = null;
            TurnAbility.Value = null;
        }
    }
}