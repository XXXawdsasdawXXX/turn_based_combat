using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using TriInspector;
using UnityEditor;
#endif


namespace Code.UI
{
    public class BattleWindowView : UIWindowView
    {
        [field: SerializeField] public TeamView PlayerTeam { get; private set; }
        [field: SerializeField] public TeamView EnemyTeam { get; private set; }
        [field: SerializeField] public TMP_Text TextRound { get; private set; }
        [field: SerializeField] public ActiveActorView ActiveActor { get; private set; }
        [field: SerializeField] public RoundQueueView RoundQueue { get; private set; }
        
        
        public void UpdateRoundNumber(int round)
        {
            TextRound.SetText($"round: {round}");
        }

        public override void Open()
        {
            base.Open();
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }


    }
}