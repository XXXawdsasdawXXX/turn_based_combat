using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class StartWindowView : UIWindowView
    {
        [field: SerializeField] public Button ButtonStart { get; private set; }
    }
}