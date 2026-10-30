using UnityEngine;

namespace Code.UI
{
    public abstract class UIWindowView : MonoBehaviour
    {
        public bool IsOpened { get; protected set; } 
            
        [SerializeField] protected RectTransform rect;
        [SerializeField] protected GameObject body;


        public virtual void Open()
        {
            body.SetActive(true);
            IsOpened = true;
        }

        public void Close()
        {
            body.SetActive(false);
            IsOpened = false;
        }

        public void Switch()
        {
            IsOpened = !IsOpened;
            body.SetActive(IsOpened);
        }

        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (rect == null)
            {
                rect = transform.GetComponent<RectTransform>();
            }

            if (body == null)
            {
                body = gameObject;
            }
        }
#endif
    }
}