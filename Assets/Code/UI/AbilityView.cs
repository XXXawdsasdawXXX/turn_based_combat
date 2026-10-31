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
        public event Action Clicked;

        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _textDescriptiom;
        [SerializeField] private Image _imageBackground;
        [SerializeField] private GameObject _markerSelect;
        
        public void Enable()
        {
            _button.onClick.AddListener(_onClicked);
            
            gameObject.SetActive(true);            
        }

        public void SetInteractable(bool isInteractable)
        {
            _button.interactable = isInteractable;
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
                    _imageBackground.color = Color.black;
                    break;
            }
        }

        public void SetSelectedState(bool isSelect)
        {
            _markerSelect.SetActive(isSelect);
        }

        public void Disable()
        {
            gameObject.SetActive(false);   
            
            Clicked = null;
            
            _button.onClick.RemoveListener(_onClicked);

            SetSelectedState(false);
        }

        private void _onClicked()
        {
            Clicked?.Invoke();
        }
    }
}