using System;
using Game.Character;
using Game.Configurations;
using Object = UnityEngine.Object;

namespace Game.Runtime
{
    public interface ICharacterFactory
    {
        public CharacterActor Create(CharacterConfiguration configuration, ETeamId team, int slot);
    }
    
    public class CharacterFactory : ICharacterFactory
    {
        private readonly AssetStorage _assetStorage;
        
        public CharacterFactory(AssetStorage assetStorage)
        {
            _assetStorage = assetStorage;
        }
        
        public CharacterActor Create(CharacterConfiguration configuration, ETeamId team, int slot)
        {
            if (_assetStorage.CharacterWorldView == null)
            {
                throw new Exception("character world view prefab is null");
            }
            
            CharacterWorldView view = Object.Instantiate(_assetStorage.CharacterWorldView);
            CharacterState state = new(configuration, team, slot); 
            CharacterActor actor = new(configuration, view, state);
            view.SetActor(actor);

#if UNITY_EDITOR
            view.SetGizmoModel(new CharacterWorldView.CharacterGizmoModel
            {
                Name = configuration.Name,
                Slot = slot
            });
#endif
            
            return actor;
        }
    }
}