using Game.Runtime;
using VContainer.Unity;

namespace Code.UI
{
    public class StartWindowController : UIWindowController<StartWindowView>
    {
        private readonly UIWindowNavigator _windowNavigator;
        private readonly IBattleController _battleController;

        
        public StartWindowController(
            StartWindowView view, 
            UIWindowNavigator windowNavigator, 
            IBattleController battleController) : base(view)
        {
            _windowNavigator = windowNavigator;
            _battleController = battleController;
        }

        public override void Start()
        {
            base.Start();
            view.Open();
        }

        protected override void onSubscribe()
        {
            view.ButtonStart.onClick.AddListener(_startBattle);
        }

        protected override void onDispose()
        {
            view.ButtonStart.onClick.RemoveListener(_startBattle);
        }

        private void _startBattle()
        {
            _battleController.StartBattle();
            
            _windowNavigator.Open(typeof(BattleWindowView));
         
            view.Close();
        }
    }
}