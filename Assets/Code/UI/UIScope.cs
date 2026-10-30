using TriInspector;
using UnityEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Code.UI
{
    public class UIScope : LifetimeScope
    {
        [SerializeField] private UIWindowView[] _views;

        protected override void Configure(IContainerBuilder builder)
        {
            foreach (UIWindowView windowView in _views)
            {
                builder.RegisterInstance(windowView).AsSelf();
            }

            //builder.Register<MenuNavigator>(Lifetime.Singleton);
            
            builder.Register<HudWindowController>(Lifetime.Singleton).AsImplementedInterfaces();
        }

#if UNITY_EDITOR
        [Button]
        private void _findViews()
        {
            _views = GetComponentsInChildren<UIWindowView>(true);
            EditorUtility.SetDirty(this);
        }
#endif
    }
}