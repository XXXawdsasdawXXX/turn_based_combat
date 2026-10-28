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
    }
}