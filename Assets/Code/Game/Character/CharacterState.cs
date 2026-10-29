using System;
using System.Collections.Generic;
using Game.Configurations;
using Game.Stats;

namespace Game.Character
{
    public class CharacterState
    {
        public Dictionary<EStatType, RuntimeStat> Stats { get; }
        public ETeamId Team { get; }
        public int Slot { get; }
        

        public CharacterState(CharacterConfiguration configuration, ETeamId team, int slot)
        {
            Team = team;
            Slot = slot;
            
            Stats = new Dictionary<EStatType, RuntimeStat>();
         
            foreach (Stat stat in configuration.Stats)
            {
                if(!Stats.TryAdd(stat.Type, new RuntimeStat(stat.Type, stat.BaseValue)))
                {
                    throw new Exception($"character state already has stat {stat.Type}");
                }
            }
        }

        public bool IsAlive()
        {
            return Stats[EStatType.Health].Current.Value > 0;
        }
    }
}