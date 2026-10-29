using System;
using UnityEngine;

namespace Game.Stats
{
    [Serializable]
    public class Stat
    {
        [field: SerializeField] public EStatType Type { get; private set; }
        [field: SerializeField] public float BaseValue { get; protected set; }

        
        public Stat(EStatType type, float value)
        {
            Type = type;
            BaseValue = value;
        }

#if UNITY_EDITOR
        
        public void _setValue(float value)
        {
            BaseValue = value;
        }

#endif
  
    }
}