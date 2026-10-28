using Game.Character;
using Game.Configurations;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Runtime
{ 
    public class GameScope : LifetimeScope
    {
        [SerializeField] private TeamsConfiguration _teamsConfiguration;
        [SerializeField] private AssetStorage _assetStorage;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_teamsConfiguration).AsSelf();
            builder.RegisterInstance(_assetStorage).AsSelf();

            builder.Register<ICharacterFactory, CharacterFactory>(Lifetime.Singleton);
            builder.Register<BattleController>(Lifetime.Singleton).AsImplementedInterfaces();
            
            base.Configure(builder);
        }
    }
}
