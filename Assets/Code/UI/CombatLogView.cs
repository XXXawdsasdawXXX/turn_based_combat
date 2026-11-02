using System.Text;
using Game.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class CombatLogView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private ScrollRect _scroll;
        [SerializeField] private int _maxLines = 40;

        private readonly StringBuilder _builder = new();
        private int _lineCount;

        
        public void Clear()
        {
            _builder.Clear();
            _lineCount = 0;

            if (_text != null)
            {
                _text.SetText(string.Empty);
            }
        }

        public void Append(CombatLogEntry entry)
        {
            if (_text == null)
            {
                return;
            }

            string kindLabel = entry.Kind switch
            {
                EActionOutcomeKind.Hit => "<color=#E74C3C>HIT</color>",
                EActionOutcomeKind.Miss => "<color=#95A5A6>MISS</color>",
                EActionOutcomeKind.Heal => "<color=#2ECC71>HEAL</color>",
                _ => entry.Kind.ToString()
            };

            string delta = entry.Kind switch
            {
                EActionOutcomeKind.Hit => $" {entry.StatDelta}",
                EActionOutcomeKind.Heal => $" +{entry.StatDelta}",
                _ => string.Empty
            };

            string line =
                $"R{entry.Round} {entry.ActorName} → {entry.TargetName} | " +
                $"{entry.AbilityType} | {kindLabel}{delta}";

            if (_lineCount >= _maxLines)
            {
                string current = _builder.ToString();
                int firstNewLine = current.IndexOf('\n');
                if (firstNewLine >= 0)
                {
                    _builder.Remove(0, firstNewLine + 1);
                    _lineCount--;
                }
            }

            if (_builder.Length > 0)
            {
                _builder.Append('\n');
            }

            _builder.Append(line);
            _lineCount++;
            _text.SetText(_builder.ToString());

            Canvas.ForceUpdateCanvases();
            if (_scroll != null)
            {
                _scroll.verticalNormalizedPosition = 0f;
            }
        }
    }
}
