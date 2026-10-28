using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game
{ 
    public class GameScope : LifetimeScope
    {
        [SerializeField] private TeamsConfiguration _teamsConfiguration;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_teamsConfiguration).AsSelf();
            
            base.Configure(builder);
        }
    }
}