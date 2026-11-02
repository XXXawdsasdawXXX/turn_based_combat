using System;
using Code.Core;
using Game.Character;
using Game.Configurations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Runtime
{
    public interface ICharacterFactory
    {
        CharacterActor Create(CharacterConfiguration configuration, ETeamId team, int slot);
        void ReleaseAll();
    }
    
    public class CharacterFactory : ICharacterFactory, IDisposable
    {
        private readonly MonoPool<CharacterWorldView> _viewPool = new();
        private readonly Transform _poolRoot;
        private bool _disposed;

        
        public CharacterFactory(AssetStorage assetStorage)
        {
            if (assetStorage.CharacterWorldView == null)
            {
                throw new Exception("character world view prefab is null");
            }

            _poolRoot = new GameObject("CharacterWorldViewPool").transform;
            _viewPool.Initialize(assetStorage.CharacterWorldView, _poolRoot);
        }
        
        public CharacterActor Create(CharacterConfiguration configuration, ETeamId team, int slot)
        {
            CharacterWorldView view = _viewPool.GetNext();
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

        public void Release(CharacterActor actor)
        {
            if (actor?.WorldView == null)
            {
                return;
            }

            _viewPool.Disable(actor.WorldView);
        }

        public void ReleaseAll()
        {
            _viewPool.DisableAll();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_poolRoot != null)
            {
                Object.Destroy(_poolRoot.gameObject);
            }
        }
    }
}
