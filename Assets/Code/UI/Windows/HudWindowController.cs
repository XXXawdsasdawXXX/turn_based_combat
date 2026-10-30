using System;
using System.Collections.Generic;
using Game.Runtime;
using R3;
using UnityEngine;

namespace Code.UI
{
    public class HudWindowController : UIWindowController<HudWindowView>
    {
        private readonly IBattleController _battleController;
        private readonly List<IDisposable> _subscription = new();

        
        public HudWindowController(HudWindowView view, IBattleController battleController) : base(view)
        {
            _battleController = battleController;
        }

        protected override void onSubscribe()
        {
            BattleState state = _battleController.State;
            _subscription.Add(state.Phase.DistinctUntilChanged().Subscribe(_onPhaseChanged));
            _subscription.Add(state.Round.Distinct().Subscribe(view.UpdateRoundNumber));
        }

        protected override void onDispose()
        {
            foreach (IDisposable disposable in _subscription)
            {
                disposable?.Dispose();
            }
            
            _subscription.Clear();
        }
        
        private void _onPhaseChanged(EBattlePhase phase)
        {
            var state = _battleController.State;
            
            Debug.Log($"on phase changed  {phase}");
            switch (phase)
            {
                default:
                case EBattlePhase.NotStarted:
                    break;
                case EBattlePhase.Start:
                    view.PlayerTeam.SetTeam(state.PlayerTeam, rightToLeft: true);
                    view.EnemyTeam.SetTeam(state.EnemyTeam);
                    break;
                case EBattlePhase.StartRound:
                    break;
                case EBattlePhase.PlayerTurn:
                    break;
                case EBattlePhase.EnemyTurn:
                    break;
                case EBattlePhase.EndRound:
                    break;
                case EBattlePhase.End:
                    break;
                case EBattlePhase.Pause:
                    break;
            }
            
            view.ActiveActor.SetInteractable(phase is EBattlePhase.PlayerTurn);
        }
    }
}