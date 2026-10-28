using System;
using R3;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class Stat
    {
        [field: SerializeField] public EStatType Type { get; private set; }
        [field: SerializeField] public float Value { get; private set; }
    }

    public sealed class RuntimeStat : Stat
    {
        public ReactiveProperty<float> DerivedValue { get; private set; }
    }
}