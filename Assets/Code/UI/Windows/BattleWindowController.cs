using System;
using System.Collections.Generic;
using Game.Runtime;
using R3;


namespace Code.UI
{
    public class BattleWindowController : UIWindowController<BattleWindowView>
    {
        private readonly IBattleController _battleController;
        private readonly List<IDisposable> _subscription = new();

        
        public BattleWindowController(BattleWindowView view, IBattleController battleController) : base(view)
        {
            _battleController = battleController;
        }

        protected override void onSubscribe()
        {
            BattleState state = _battleController.State;
            
            view.RoundQueue.SubscribeToBattleState(_battleController.State);
            view.ActiveActor.SubscribeToBattleState(_battleController.State);
            
            _subscription.Add(state.Phase.DistinctUntilChanged().Subscribe(_onPhaseChanged));
            _subscription.Add(state.Round.DistinctUntilChanged().Subscribe(view.UpdateRoundNumber));
        }

        protected override void onDispose()
        {
            view.ActiveActor.Dispose();
            
            foreach (IDisposable disposable in _subscription)
            {
                disposable?.Dispose();
            }
            
            _subscription.Clear();
        }
        
        private void _onPhaseChanged(EBattlePhase phase)
        {
            BattleState state = _battleController.State;

            switch (phase)
            {
                default:
                case EBattlePhase.NotStarted:
                    break;
                case EBattlePhase.Initialize:
                    break;
                case EBattlePhase.Start:
                    view.PlayerTeam.SetTeam(state.PlayerTeam, rightToLeft: true);
                    view.EnemyTeam.SetTeam(state.EnemyTeam);
                    break;
                case EBattlePhase.StartRound:
                    break;
                case EBattlePhase.End:
                    break;
            }
        }
    }
}