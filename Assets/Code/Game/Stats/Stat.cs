using System;
using UnityEngine;

namespace Game.Stats
{
    [Serializable]
    public class Stat
    {
        [field: SerializeField] public EStatType Type { get; private set; }
        [field: SerializeField] public float Value { get; private set; }

        public Stat(EStatType type, float value)
        {
            Type = type;
            Value = value;
        }
    }
}