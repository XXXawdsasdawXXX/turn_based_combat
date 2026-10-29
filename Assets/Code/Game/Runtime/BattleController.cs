using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Abilities;
using Game.Character;
using Game.Configurations;
using UnityEngine;
using VContainer.Unity;

namespace Game.Runtime
{
    public class BattleController : IStartable
    {
        public BattleState State { get; } = new();
        
        private readonly ICharacterFactory _characterFactory;
        private readonly TeamsConfiguration _teamsConfiguration;

        private readonly ActionRunner _actionRunner;
        private TurnQueue _queue;

        
        public BattleController(ICharacterFactory characterFactory, TeamsConfiguration teamsConfiguration)
        {
            _characterFactory = characterFactory;
            _teamsConfiguration = teamsConfiguration;
            _actionRunner = new ActionRunner();
        }
        
        public void Start()
        {
            _createTeam(ETeamId.Player, _teamsConfiguration.Player, State.PlayerTeam);
            _createTeam(ETeamId.Enemy, _teamsConfiguration.Enemy, State.EnemyTeam);
          
            _queue = new TurnQueue(State.PlayerTeam, State.EnemyTeam);
            
            _startRound().Forget();
        }
        
        private void _createTeam(
            ETeamId team, 
            IReadOnlyList<CharacterConfiguration> configurations, 
            List<CharacterActor> actors)
        {   
             const float Y = -1;
             const float StartX = 8;
             const float CharacterDistance = 2;
            
            if (configurations == null || configurations.Count == 0)
            {
                throw new Exception($"configurations from team {team} is empty");
            }

            // Slot 0 at front line (near center), higher slots go outward. No Reverse — list[i].Slot == i.
            float outward = team is ETeamId.Player ? -1f : 1f;
            float frontX = outward * (StartX - (configurations.Count - 1) * CharacterDistance);

            for (int slot = 0; slot < configurations.Count; slot++)
            {
                CharacterConfiguration characterConfiguration = configurations[slot];

                if (characterConfiguration == null)
                {
                    throw new Exception($"character configuration #{slot} from team {team} is null");
                }

                float x = frontX + outward * slot * CharacterDistance;
                CharacterActor actor = _characterFactory.Create(characterConfiguration, team, slot);
                actors.Add(actor);
                actor.WorldView.SetPosition(new Vector3(x, Y, 0), team);
            }
        }

        private async UniTask _startRound()
        {
            _queue.CreateRoundQueue();
            
            while (true)
            {
                if (_queue.TryGetNextActor(out CharacterActor actor))
                {
                    State.SetCurrentActor(actor);
                    actor.WorldView.ChangeTurnView(true);

                    if (actor.State.Team is ETeamId.Enemy)
                    {
                        CharacterActor target = State.PlayerTeam.FirstOrDefault(a => a.State.IsAlive());
                  
                        target.WorldView.ChangeTargetView(true);
                        await _actionRunner.StartAction(
                            actor,
                            new List<CharacterActor>() { target },
                            actor.Configuration.Abilities.FirstOrDefault(
                                a => a.Type == EAbilityType.MeleeAttack));
                        
                        target.WorldView.ChangeTargetView(false);
                    }
                    
                    actor.WorldView.ChangeTurnView(false);
                    
                    State.ResetCurrentActor();

                    if (State.BattleEnd(out ETeamId winner))
                    {
                        Debug.Log($"team is won {winner}");
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            
            State.MoveToNextRound();
        }
    }
}