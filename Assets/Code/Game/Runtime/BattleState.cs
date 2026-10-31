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


        public void DisableChoiceTargetMode(CharacterActor keepTargetHighlight = null)
        {
            _clearChoiceTargetMode(EnemyTeam, keepTargetHighlight);
            _clearChoiceTargetMode(PlayerTeam, keepTargetHighlight);
        }

        public void EnableChoiceTargetMode(IEnumerable<CharacterActor> actors)
        {
            foreach (CharacterActor actor in actors)
            {
                actor.State.SetCanBeTarget(true);
            }
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

        public IReadOnlyList<CharacterActor> GetAllies()
        {
            return _isPlayerTurn() ? PlayerTeam : EnemyTeam;
        }

        public IReadOnlyList<CharacterActor> GetEnemies()
        {
            return _isPlayerTurn() ? EnemyTeam : PlayerTeam;
        }

        private bool _isPlayerTurn()
        {
            return TurnActor.Value != null && TurnActor.Value.State.Team == ETeamId.Player;
        }

        private static void _clearChoiceTargetMode(
            IEnumerable<CharacterActor> team,
            CharacterActor keepTargetHighlight)
        {
            foreach (CharacterActor actor in team)
            {
                actor.State.SetCanBeTarget(false);
                actor.State.SetObservedState(false);

                if (actor != keepTargetHighlight)
                {
                    actor.State.SetTargetState(false);
                }
            }
        }
    }
}