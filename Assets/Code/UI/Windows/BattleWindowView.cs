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
        [field: SerializeField] public CombatLogView CombatLog { get; private set; }
        
        
        public void UpdateRoundNumber(int round)
        {
            TextRound.SetText($"round: {round}");
        }

        public override void Open()
        {
            base.Open();

            EnsureCombatLog();
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }

        public void EnsureCombatLog()
        {
            if (CombatLog != null)
            {
                return;
            }

            Transform existing = transform.Find("CombatLog");
            if (existing != null)
            {
                CombatLog = existing.GetComponent<CombatLogView>();
                if (CombatLog != null)
                {
                    return;
                }
            }

            GameObject logGo = new("CombatLog");
            logGo.transform.SetParent(transform, false);
            CombatLog = logGo.AddComponent<CombatLogView>();
        }


    }
}