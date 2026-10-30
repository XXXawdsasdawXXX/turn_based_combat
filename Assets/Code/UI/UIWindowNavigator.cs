using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.UI
{
    public class UIWindowNavigator
    {
        private readonly Dictionary<Type, UIWindowView> _views = new();
        
        
        protected UIWindowNavigator(IEnumerable<UIWindowView> windows)
        {
            UIWindowView[] array = windows.ToArray();

            foreach (UIWindowView view in array)
            {
                Type type = view.GetType();

                if (_views.ContainsKey(type))
                {
                    Debug.LogError( $"[construct] views collection already has view with type {type.Name}");
                    continue;
                }

                _views.Add(type, view);
            }
        }

        public void Open(Type viewType)
        {
            if (_views.TryGetValue(viewType, out UIWindowView view))
            {
                if (view.IsOpened)
                {
                    Debug.LogError(
                        $"[open] unable to open the view with type {viewType.Name} because it is already open");
                    return;
                }

                view.Open();
            }
            else
            {
                Debug.LogError($"[open] can not find view with type {viewType.Name}");
            }
        }

        public void Close(Type viewType)
        {
            if (_views.TryGetValue(viewType, out UIWindowView view))
            {
                if (!view.IsOpened)
                {
                    Debug.LogError($"[close] unable to close the view with type {viewType.Name} because it is already close");
                    return;
                }

                view.Close();
            }
            else
            {
                Debug.LogError($"[close] can not find view with type {viewType.Name}");
            }
        }
    }
}