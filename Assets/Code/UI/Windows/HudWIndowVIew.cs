using TMPro;
using UnityEngine;

namespace Code.UI
{
    public class HudWindowView : UIWindowView
    {
        [field: SerializeField] public TeamView PlayerTeam { get; private set; }
        [field: SerializeField] public TeamView EnemyTeam { get; private set; }
        [field: SerializeField] public TMP_Text TextRound { get; private set; }
        [field: SerializeField] public ActiveActorView ActiveActor { get; private set; }

        public void UpdateRoundNumber(int round)
        {
            TextRound.SetText($"round: {round}");
        }
    }
}