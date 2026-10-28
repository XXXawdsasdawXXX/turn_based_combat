using System.Collections.Generic;
using Game.Configurations;
using Game.Stats;

namespace Game.Character
{
    public class CharacterActor
    {
        public CharacterWorldView WorldView { get; }
        public List<RuntimeStat> Stats { get; }
        public ETeamId Team { get; }
        public int Slot { get; }
        
        
        public CharacterActor(CharacterConfiguration configuration, CharacterWorldView worldView, ETeamId team, int slot)
        {
            Team = team;
            Slot = slot;
            
            WorldView = worldView;
            WorldView.SetBodyColor(configuration.Color);
            
            Stats = new List<RuntimeStat>();
            foreach (Stat stat in configuration.Stats)
            {
                Stats.Add(new RuntimeStat(stat.Type, stat.Value));   
            }
        }
    }
}