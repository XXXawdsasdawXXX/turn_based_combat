using System.Collections.Generic;
using Code.Core;
using Game.Character;
using UnityEngine;

namespace Code.UI
{
    public class TeamView : MonoBehaviour
    {
        [field: SerializeField]
        public ETeamId Team
        {
            get;
            private set;
        } 
        
        [SerializeField] private MonoPool<UICharacterView> _pool;

        private int _selectedSlot = -1;
        
        public void SetTeam(List<CharacterActor> actors, bool rightToLeft = false)
        {
            _pool.DisableAll();
            
            foreach (CharacterActor actor in actors)
            {
                UICharacterView view = _pool.GetNext();
                view.SetActor(actor);
                view.Enable();
            }

            if (rightToLeft)
            {
                _pool.ReverseHierarchy();
            }
        }
    }
}