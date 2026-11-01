using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Abilities;
using Game.Character;
using Game.Configurations;
using R3;
using UnityEngine;
using VContainer.Unity;
using Random = UnityEngine.Random;

namespace Game.Runtime
{
    public interface IBattleController
    {
        BattleState State { get; }

        public void StartBattle();
    }

    public sealed class BattleController : IBattleController, IStartable, IDisposable
    {
        public BattleState State { get; } = new();

        private readonly ICharacterFactory _characterFactory;
        private readonly TeamsConfiguration _teamsConfiguration;

        private CancellationTokenSource _cts;


        public BattleController(ICharacterFactory characterFactory, TeamsConfiguration teamsConfiguration)
        {
            _characterFactory = characterFactory;
            _teamsConfiguration = teamsConfiguration;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
        }
        
        public void Dispose()
        {
            _cts?.Dispose();
            State.Dispose();
        }

        public void StartBattle()
        {
            if (State.Phase.CurrentValue is not EBattlePhase.NotStarted)
            {
                return;
            }

            State.RoundActorsQueue.Clear();
            
            State.SetPhase(EBattlePhase.Initialize);

            _createTeam(ETeamId.Player, _teamsConfiguration.Player, State.PlayerTeam);
            _createTeam(ETeamId.Enemy, _teamsConfiguration.Enemy, State.EnemyTeam);

            State.RoundActorsQueue.AddCharacters(State.PlayerTeam);
            State.RoundActorsQueue.AddCharacters(State.EnemyTeam);

            State.SetPhase(EBattlePhase.Start);

            _runBattle(_cts.Token).Forget();
        }

        private async UniTask _runBattle(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await _runRound(token);

                    if (State.Phase.CurrentValue is EBattlePhase.End)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // restart/exit later
            }
        }

        
        private async UniTask _runRound(CancellationToken token)
        {
            State.NextRound();
            
            State.RoundActorsQueue.CreateRoundQueue();
      
            State.SetPhase(EBattlePhase.StartRound);

            
            while (!token.IsCancellationRequested)
            {
                if (State.RoundActorsQueue.TryGetNextActor(out CharacterActor actor))
                {
                    await _runTurn(actor, token);
                    
                    if (State.IsBattleEnd(out ETeamId winner))
                    {
                        Debug.Log($"team is won {winner}");

                        State.SetPhase(EBattlePhase.End);
                        
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private async UniTask _runTurn(CharacterActor actor, CancellationToken token)
        {
            State.DisableChoiceTargetMode();
            State.Turn.SetActor(actor);
            State.SetPhase(EBattlePhase.StartTurn);
            
            if (actor.State.Team is ETeamId.Enemy)
            {
                int randomAbilityIndex = Random.Range(0, actor.Configuration.Abilities.Length);
                State.Turn.SetAbility(actor.Configuration.Abilities[randomAbilityIndex]);
                
                if (!ActionResolver.CanUse(
                        State.Turn.Actor.CurrentValue, 
                        State.Turn.Ability.CurrentValue, 
                        State.EnemyTeam))
                {
                    State.Turn.SetAbility(Ability.Skip());
                }
                
                _syncChoiceTargetMode();

                IReadOnlyList<CharacterActor> validTargets = ActionResolver.GetValidTargets(
                    State.Turn.Ability.CurrentValue, 
                    State.GetAllies(), 
                    State.GetEnemies());

                if (validTargets.Count > 0)
                {
                    CharacterActor enemyTarget = validTargets[Random.Range(0, validTargets.Count)];
                    State.Turn.SetTarget(enemyTarget);
                }
            }
            else
            {
                while (State.Turn.Target.CurrentValue == null)
                {
                    await UniTask.WaitUntil(() => State.Turn.Ability.CurrentValue != null, cancellationToken: token);

                    Ability chosen = State.Turn.Ability.CurrentValue;
                    
                    if (chosen.Type is EAbilityType.Skip)
                    {
                        Debug.Log("player skip");
                        break;
                    }

                    _syncChoiceTargetMode();

                    await UniTask.WaitUntil(
                        () => State.Turn.Target.CurrentValue != null || State.Turn.Ability.CurrentValue != chosen,
                        cancellationToken: token);
                }
            }

            CharacterActor target = State.Turn.Target.CurrentValue;

            if (target != null && State.Turn.Ability.CurrentValue.Type is not EAbilityType.Skip)
            {
                State.DisableChoiceTargetMode(keepTargetHighlight: target);
                target.State.ForceTargetState(true);

                ActionOutcome outcome = ActionResolver.Resolve(
                    actor, 
                    State.Turn.Ability.CurrentValue, 
                    target, 
                    State.GetAllies());
                
                await ActionRunner.StartAction(actor, outcome);
                
                target.State.ForceTargetState(false);
            }
            else
            {
                State.DisableChoiceTargetMode();
            }
            
            State.Turn.ResetState();
            State.SetPhase(EBattlePhase.EndTurn);
        }

        private void _createTeam(
            ETeamId team,
            IReadOnlyList<CharacterConfiguration> configurations,
            ICollection<CharacterActor> actors)
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

                actor.State.IsTarget.DistinctUntilChanged().Subscribe(isTarget =>
                {
                    if (isTarget)
                    {
                        CharacterActor previous = State.Turn.Target.CurrentValue;
                        if (previous != null && previous != actor)
                        {
                            previous.State.SetTargetState(false);
                        }

                        State.Turn.SetTarget(actor);
                    }
                    else if (State.Turn.Target.CurrentValue == actor)
                    {
                        State.Turn.SetTarget(null);
                    }
                });
            }
        }

        private void _syncChoiceTargetMode()
        {   
            State.DisableChoiceTargetMode();
            
            IReadOnlyList<CharacterActor> validTargets = ActionResolver.GetValidTargets(
                State.Turn.Ability.CurrentValue,
                State.GetAllies(),
                State.GetEnemies());

            if (validTargets.Count == 0)
            {
                return;
            }
            
            State.EnableChoiceTargetMode(validTargets);
        }
    }
}