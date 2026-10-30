using System;
using Code.Core;
using Game.Abilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class AbilityView : MonoBehaviour, IPoolEntity
    {
        [SerializeField] private TMP_Text _textDescriptiom;
        [SerializeField] private Image _imageBackground; 
        
        public void Enable()
        {
            gameObject.SetActive(true);            
        }

        public void SetAbility(Ability ability)
        {
            _textDescriptiom.SetText($"{ability.Type} - {ability.Effect.BaseValue}");
            
            switch (ability.Type)
            {
                case EAbilityType.MeleeAttack:
                    _imageBackground.color = new Color(0.6f, 0.4f, 0.2f, 1);
                    break;
                case EAbilityType.RangeAttack:
                    _imageBackground.color = new Color(0.6f, 0.2f, 0.4f, 1);
                    break;
                case EAbilityType.Heal:
                    _imageBackground.color = new Color(0.3f, 0.4f, 0.2f, 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Disable()
        {
            gameObject.SetActive(false);            
        }
    }
}