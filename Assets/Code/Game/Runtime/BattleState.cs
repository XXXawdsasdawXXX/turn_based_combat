using System;
using System.Collections.Generic;
using System.Linq;
using Game.Character;
using R3;

namespace Game.Runtime
{
    public class BattleState : IDisposable
    {
        public List<CharacterActor> PlayerTeam { get; } = new();
        public List<CharacterActor> EnemyTeam { get; } = new();
     
        public ReadOnlyReactiveProperty<int> Round => _round;
        private readonly ReactiveProperty<int> _round = new(0);
        
        public ReadOnlyReactiveProperty<EBattlePhase> Phase => _phase;
        private readonly ReactiveProperty<EBattlePhase> _phase = new(EBattlePhase.NotStarted);
        
        public RoundQueue RoundActorsQueue { get; } = new();
        public TurnState Turn {get; } = new();

        
        public void SetPhase(EBattlePhase phase)
        {
            _phase.Value = phase;
        }

        public void NextRound()
        {
            _round.Value++;
        }

        public IEnumerable<CharacterActor> GetAllies()
        {
            return Turn.IsPlayerTurn() ? PlayerTeam : EnemyTeam;
        }

        public IEnumerable<CharacterActor> GetEnemies()
        {
            return Turn.IsPlayerTurn() ? EnemyTeam : PlayerTeam;
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

        public void EnableChoiceTargetMode(IEnumerable<CharacterActor> actors)
        {
            foreach (CharacterActor actor in actors)
            {
                actor.State.SetCanBeTarget(true);
            }
        }

        public void DisableChoiceTargetMode(CharacterActor keepTargetHighlight = null)
        {
            _clearChoiceTargetMode(EnemyTeam, keepTargetHighlight);
            _clearChoiceTargetMode(PlayerTeam, keepTargetHighlight);
        }

        public void SyncChoiceTargetMode()
        {
            DisableChoiceTargetMode();

            IReadOnlyList<CharacterActor> validTargets = ActionResolver.GetValidTargets(
                Turn.Ability.CurrentValue,
                GetAllies(),
                GetEnemies());

            if (validTargets.Count == 0)
            {
                return;
            }

            EnableChoiceTargetMode(validTargets);
        }
        
        public void ResetState()
        {
            Turn.ResetState();            
            RoundActorsQueue.Clear();
            
            _phase.Value = EBattlePhase.NotStarted;
            _round.Value = 0;
        }
        

        public void Dispose()
        {
            _phase.Dispose();
            _round.Dispose();
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