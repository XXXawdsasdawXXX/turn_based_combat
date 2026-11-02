using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class PauseWindowView : UIWindowView
    {
        [field: SerializeField] public Button ButtonContinue { get; private set; }
        [field: SerializeField] public Button ButtonRestart { get; private set; }
    }
}
