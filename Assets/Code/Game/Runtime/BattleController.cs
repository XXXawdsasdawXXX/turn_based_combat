using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

    public class BattleController : IBattleController, IStartable
    {
        public BattleState State { get; } = new();

        private readonly ICharacterFactory _characterFactory;
        private readonly TeamsConfiguration _teamsConfiguration;

        private TurnQueue _queue;

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

        public void StartBattle()
        {
            if (State.Phase.Value is not EBattlePhase.NotStarted)
            {
                return;
            }

            State.Phase.Value = EBattlePhase.Initialize;

            _createTeam(ETeamId.Player, _teamsConfiguration.Player, State.PlayerTeam);
            _createTeam(ETeamId.Enemy, _teamsConfiguration.Enemy, State.EnemyTeam);

            _queue = new TurnQueue(State.PlayerTeam, State.EnemyTeam);

            State.Phase.Value = EBattlePhase.Start;

            _runBattle(_cts.Token).Forget();
        }

        private async UniTask _runBattle(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await _runRound(token);

                    if (State.Phase.Value is EBattlePhase.End)
                    {
                        break;
                    }

                    State.Phase.Value = EBattlePhase.EndRound;
                }
            }
            catch (OperationCanceledException)
            {
                // restart/exit later
            }
        }

        private async UniTask _runRound(CancellationToken token)
        {
            State.Round.Value++;
            State.Phase.Value = EBattlePhase.StartRound;

            _queue.CreateRoundQueue();

            while (!token.IsCancellationRequested)
            {
                if (_queue.TryGetNextActor(out CharacterActor actor))
                {
                    await _runTurn(actor, token);

                    if (State.IsBattleEnd(out ETeamId winner))
                    {
                        Debug.Log($"team is won {winner}");

                        State.Phase.Value = EBattlePhase.End;

                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            State.Phase.Value = EBattlePhase.EndRound;
        }

        private async UniTask _runTurn(CharacterActor actor, CancellationToken token)
        {
            State.DisableTargetMode();

            State.TurnActor.Value = actor;
            actor.State.SetTurnState(true);

            if (actor.State.Team is ETeamId.Enemy)
            {
                State.Phase.Value = EBattlePhase.EnemyTurn;
        
                while (!ActionResolver.CanUse(State.TurnActor.Value, State.TurnAbility.Value, State.EnemyTeam))
                {
                    int randomAbilityIndex = Random.Range(0, actor.Configuration.Abilities.Length);
                    State.TurnAbility.Value = actor.Configuration.Abilities[randomAbilityIndex];
                    Debug.Log($"enemy choice ability {State.TurnAbility.Value.Type}");
                }
                
                _syncTargetModeForAbility();

                IReadOnlyList<CharacterActor> validTargets = ActionResolver.GetValidTargets(
                    State.TurnAbility.Value, State.GetTeam(ETeamId.Enemy), State.GetTeam(ETeamId.Player));

                if (validTargets == null)
                {
                    Debug.Log($"enemy return ");
                    return;
                }
                CharacterActor enemyTarget = validTargets[Random.Range(0, validTargets.Count)];

                State.TurnTarget.Value = enemyTarget;
            }
            else
            {
                State.Phase.Value = EBattlePhase.PlayerTurn;

                while (State.TurnTarget.Value == null)
                {
                    await UniTask.WaitUntil(() => State.TurnAbility.Value != null, cancellationToken: token);

                    Ability chosen = State.TurnAbility.Value;
                    
                    if (chosen.Type is EAbilityType.Skip)
                    {
                        break;
                    }

                    _syncTargetModeForAbility();

                    await UniTask.WaitUntil(
                        () => State.TurnTarget.Value != null || State.TurnAbility.Value != chosen,
                        cancellationToken: token);
                }
            }

            State.DisableTargetMode();

            CharacterActor target = State.TurnTarget.Value;

            if (target != null && State.TurnAbility.Value.Type is not EAbilityType.Skip)
            {
                target.State.SetTargetState(true);

                ActionOutcome outcome = ActionResolver.Resolve(
                    actor, 
                    State.TurnAbility.Value, 
                    target, 
                    State.PlayerTeam, 
                    State.EnemyTeam);
                
                await ActionRunner.StartAction(actor, outcome);
                
                target.State.SetTargetState(false);
            }

            actor.State.SetTurnState(false);

            State.ResetTurnProperties();
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
                        State.TurnTarget.Value?.State.SetTargetState(false);
                        State.TurnTarget.Value = actor;
                    }
                    else
                    {
                        State.TurnTarget.Value = null;
                    }
                });
            }
        }

        private void _syncTargetModeForAbility()
        {   
            State.DisableTargetMode();
        
            State.TurnTarget.Value = null;

            bool isPlayerTurn = State.TurnActor.Value.State.Team is ETeamId.Player;

            IReadOnlyList<CharacterActor> validTargets = ActionResolver.GetValidTargets(
                State.TurnAbility.Value,
                isPlayerTurn ? State.PlayerTeam : State.EnemyTeam,
                isPlayerTurn ? State.EnemyTeam : State.PlayerTeam);

            if (validTargets == null)
            {
                return;
            }
            
            State.EnableTargetMode(validTargets);
        }
    }
}