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
        [field: SerializeField] public Stat EffectValue { get; private set; }

        public bool IsAttacking() => Type is not EAbilityType.Heal;
    }
}