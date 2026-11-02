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

            builder.Register<UIWindowNavigator>(Lifetime.Singleton);
            
            builder.Register<StartWindowController>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<BattleWindowController>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<PauseWindowController>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<EndGameWindowView>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}