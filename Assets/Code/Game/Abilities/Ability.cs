using System;
using Game.Stats;
using UnityEngine;

namespace Game.Abilities
{
    [Serializable]
    public class Ability
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public EAbilityType Type { get; private set; }
        [field: SerializeField] public Stat Effect { get; private set; }

        
        public bool IsAttacking()
        {
            return Type is not EAbilityType.Heal;
        }

        public static Ability Skip()
        {
            return new Ability()
            {
                Type = EAbilityType.Skip
            };
        } 
    }
}