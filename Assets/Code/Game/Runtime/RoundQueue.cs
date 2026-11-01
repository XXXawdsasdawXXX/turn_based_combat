using System.Collections.Generic;
using Game.Character;
using Game.Stats;

namespace Game.Runtime
{
    public class RoundQueue
    {
        public IReadOnlyList<CharacterActor> Queue => _allActors;
        private readonly List<CharacterActor> _allActors = new();
        public int LastIndex { get; private set; }
        

        public void AddCharacters(IEnumerable<CharacterActor> actors)
        {
            _allActors.AddRange(actors);
        }
        
        public void CreateRoundQueue()
        {
            _allActors.Sort((a, b) => 
                b.State.Stats[EStatType.Speed].Current.Value
                .CompareTo(a.State.Stats[EStatType.Speed].Current.Value));

            LastIndex = 0;
        }

        public bool TryGetNextActor(out CharacterActor actor)
        {
            actor = null;
            
            for (int i = LastIndex; i < _allActors.Count; i++)
            {
                LastIndex++;
                
                if (_allActors[i].State.IsAlive())
                {
                    actor = _allActors[i];
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            _allActors.Clear();
        }
    }
}