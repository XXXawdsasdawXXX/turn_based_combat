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

namespace Game.Runtime
{
    public interface IBattleController
    {
        BattleState State { get; }
        void StartBattle();
        void Pause();
        void Resume();
        void Restart();
        void Exit();
    }

    public sealed class BattleController : IBattleController, IStartable, IDisposable
    {
        public BattleState State { get; } = new();

        private readonly ICharacterFactory _characterFactory;
        private readonly TeamsConfiguration _teamsConfiguration;
        private readonly ITurnDecider _playerDecider = new PlayerTurnDecider();
        private readonly ITurnDecider _enemyDecider = new EnemyTurnDecider();
        private readonly List<IDisposable> _actorSubscriptions = new();

        private CancellationTokenSource _cts;
        private UniTask _battleTask;
        private bool _isStopping;


        public BattleController(ICharacterFactory characterFactory, TeamsConfiguration teamsConfiguration)
        {
            _characterFactory = characterFactory;
            _teamsConfiguration = teamsConfiguration;
        }

        public void Start()
        {
            _resetCts();
        }
        
        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _teardownActors();
            State.Dispose();
        }

        public void StartBattle()
        {
            if (State.Phase.CurrentValue is not EBattlePhase.NotStarted)
            {
                return;
            }

            if (_cts == null || _cts.IsCancellationRequested)
            {
                _resetCts();
            }

            State.RoundActorsQueue.Clear();
            
            State.SetPhase(EBattlePhase.Initialize);

            _createTeam(ETeamId.Player, _teamsConfiguration.Player, State.PlayerTeam);
            _createTeam(ETeamId.Enemy, _teamsConfiguration.Enemy, State.EnemyTeam);

            State.RoundActorsQueue.AddCharacters(State.PlayerTeam);
            State.RoundActorsQueue.AddCharacters(State.EnemyTeam);

            State.SetPhase(EBattlePhase.Start);

            _battleTask = _runBattle(_cts.Token);
        }

        public void Pause()
        {
            if (State.IsPaused.CurrentValue)
            {
                return;
            }

            if (State.Phase.CurrentValue is EBattlePhase.NotStarted)
            {
                return;
            }

            State.SetPaused(true);
            State.DisableChoiceTargetMode();
            
            _foreachActor(actor => actor.WorldView.Animation.Pause());
        }

        public void Resume()
        {
            if (!State.IsPaused.CurrentValue)
            {
                return;
            }

            _foreachActor(actor => actor.WorldView.Animation.Resume());
            
            State.SetPaused(false);
            _restoreTargetingIfNeeded();
        }

        public void Restart()
        {
            _restartAsync().Forget();
        }

        public void Exit()
        {
            _exitAsync().Forget();
        }

        private async UniTaskVoid _restartAsync()
        {
            await _endSessionAsync();
            StartBattle();
        }

        private async UniTaskVoid _exitAsync()
        {
            await _endSessionAsync();
        }

        private async UniTask _endSessionAsync()
        {
            if (_isStopping)
            {
                return;
            }

            _isStopping = true;

            try
            {
                State.SetPaused(false);

                if (_cts is { IsCancellationRequested: false })
                {
                    _cts.Cancel();
                }

                try
                {
                    await _battleTask;
                }
                catch (OperationCanceledException)
                {
                }

                _teardownBattle();
                _resetCts();
            }
            finally
            {
                _isStopping = false;
            }
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
                        
                        State.InvokeEndGame(winner);
                        
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

            ITurnDecider decider = actor.State.Team is ETeamId.Player
                ? _playerDecider
                : _enemyDecider;

            TurnChoice choice = await decider.DecideAsync(actor, State, token);
            await _executeChoice(actor, choice, token);

            State.Turn.ResetState();
            State.SetPhase(EBattlePhase.EndTurn);
        }

        private async UniTask _executeChoice(CharacterActor actor, TurnChoice choice, CancellationToken token)
        {
            if (choice.Target == null || choice.Ability.Type is EAbilityType.Skip)
            {
                State.DisableChoiceTargetMode();
                return;
            }

            CharacterActor target = choice.Target;

            try
            {
                State.DisableChoiceTargetMode(keepTargetHighlight: target);
                target.State.ForceTargetState(true);

                ActionOutcome outcome = ActionResolver.Resolve(
                    actor,
                    choice.Ability,
                    target,
                    State.GetAllies());

                await ActionRunner.StartAction(actor, outcome, token);
            }
            finally
            {
                target.State.ForceTargetState(false);
                State.DisableChoiceTargetMode();
            }
        }

        private void _teardownBattle()
        {
            State.DisableChoiceTargetMode();
            // Turn still holds the active actor — clear it before disposing CharacterState.
            State.Turn.ResetState();
            _teardownActors();
            State.PlayerTeam.Clear();
            State.EnemyTeam.Clear();
            State.ResetState();
        }

        private void _teardownActors()
        {
            foreach (IDisposable subscription in _actorSubscriptions)
            {
                subscription?.Dispose();
            }

            _actorSubscriptions.Clear();

            _characterFactory.ReleaseAll();
        }

        private void _resetCts()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        private void _restoreTargetingIfNeeded()
        {
            Ability ability = State.Turn.Ability.CurrentValue;

            if (!State.Turn.IsPlayerTurn()
                || ability == null
                || ability.Type is EAbilityType.Skip
                || State.Turn.Target.CurrentValue != null)
            {
                return;
            }

            State.SyncChoiceTargetMode();
        }

        private void _foreachActor(Action<CharacterActor> action)
        {
            foreach (CharacterActor actor in State.PlayerTeam)
            {
                action(actor);
            }

            foreach (CharacterActor actor in State.EnemyTeam)
            {
                action(actor);
            }
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

                _actorSubscriptions.Add(actor.State.IsTarget.DistinctUntilChanged().Subscribe(isTarget =>
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
                }));
            }
        }
    }
}
