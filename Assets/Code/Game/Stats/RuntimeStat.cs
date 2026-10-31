using R3;

namespace Game.Stats
{
    public sealed class RuntimeStat : Stat
    {
        public ReactiveProperty<float> Current { get; } 
        
        public RuntimeStat(EStatType type, float value) : base(type, value)
        {
            Current = new ReactiveProperty<float>(value);
        }

        public void SetCurrent(float value)
        {
            if (value < 0)
            {
                value = 0;
            }
            else if(value > BaseValue)
            {
                value = BaseValue;
            }

            Current.Value = value;
        }

        public bool IsFull()
        {
            return Current.CurrentValue >= BaseValue;
        }
    }
}