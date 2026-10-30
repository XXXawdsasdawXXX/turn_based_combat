using Code.Core;
using Game.Abilities;
using Game.Character;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class ActiveActorView : MonoBehaviour
    {
        [field: SerializeField] public Button SkipButton { get; private set; }

        [SerializeField] private GameObject _locker;

        [SerializeField] private MonoPool<AbilityView> _abilitiesPool;
        
        
        public void SetActor(CharacterActor stateCurrentActor)
        {
            _abilitiesPool.DisableAll();
            
            foreach (Ability ability in stateCurrentActor.Configuration.Abilities)
            {
                AbilityView abilityView = _abilitiesPool.GetNext();
                abilityView.SetAbility(ability);
            }
        }

        public void SetInteractable(bool isInteractable)
        {
            _locker.SetActive(isInteractable);
        }
    }
}