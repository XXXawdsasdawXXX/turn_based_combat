using Game.Character;
using Game.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class EndGameWindowView : UIWindowView
    {
        [field: SerializeField] public Button ButtonRestart { get; private set; }
        [field: SerializeField] public TMP_Text TextWinner { get; private set; }
    }

    public class EndGameWindowController : UIWindowController<EndGameWindowView>
    {
        private readonly IBattleController _battleController;

        
        public EndGameWindowController(IBattleController battleController, EndGameWindowView view) : base(view)
        {
            _battleController = battleController;
        }

        protected override void onSubscribe()
        {
            _battleController.State.GameEnded += _onGameEnded;
            view.ButtonRestart.onClick.AddListener(_restartGame);
        }
        
        protected override void onDispose()
        {        
            _battleController.State.GameEnded += _onGameEnded;
            view.ButtonRestart.onClick.RemoveListener(_restartGame);
        }

        private void _restartGame()
        {
            _battleController.Restart();
            view.Close();
        }

        private void _onGameEnded(ETeamId team)
        {
            view.TextWinner.SetText($"{team} is winner");
            view.Open();
        }
    }
}