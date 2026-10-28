using System;
using Game.Configurations;
using Object = UnityEngine.Object;

namespace Game.Character
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
            
            CharacterActor actor = new(configuration, view, team, slot);
            
            return actor;
        }
    }
}