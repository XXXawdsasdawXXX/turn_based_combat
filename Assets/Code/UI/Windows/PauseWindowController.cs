using Game.Runtime;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Code.UI
{
    public class PauseWindowController : UIWindowController<PauseWindowView>, ITickable
    {
        private readonly IBattleController _battleController;
        private readonly UIWindowNavigator _windowNavigator;


        public PauseWindowController(
            PauseWindowView view,
            IBattleController battleController,
            UIWindowNavigator windowNavigator) : base(view)
        {
            _battleController = battleController;
            _windowNavigator = windowNavigator;
        }

        public override void Start()
        {
            view.Close();
            base.Start();
        }

        protected override void onSubscribe()
        {
            view.ButtonContinue.onClick.AddListener(_continue);
            view.ButtonRestart.onClick.AddListener(_restart);
        }

        protected override void onDispose()
        {
            view.ButtonContinue.onClick.RemoveListener(_continue);
            view.ButtonRestart.onClick.RemoveListener(_restart);
        }

        public void Tick()
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
            {
                return;
            }

            BattleState state = _battleController.State;

            if (state.IsPaused.CurrentValue)
            {
                _continue();
                return;
            }

            if (state.Phase.CurrentValue is EBattlePhase.NotStarted)
            {
                return;
            }

            _pause();
        }

        private void _pause()
        {
            _battleController.Pause();

            if (!view.IsOpened)
            {
                _windowNavigator.Open(typeof(PauseWindowView));
            }
        }

        private void _continue()
        {
            _battleController.Resume();
            _closePauseWindow();
        }

        private void _restart()
        {
            _closePauseWindow();
            _battleController.Restart();
        }

        private void _closePauseWindow()
        {
            if (view.IsOpened)
            {
                _windowNavigator.Close(typeof(PauseWindowView));
            }
        }
    }
}