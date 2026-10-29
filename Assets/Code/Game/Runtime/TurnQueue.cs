using System.Collections.Generic;
using Game.Character;
using Game.Stats;

namespace Game.Runtime
{
    public class TurnQueue
    {
        private readonly List<CharacterActor> _allActors = new();

        private int _lastIndex;
        
        
        public TurnQueue(IEnumerable<CharacterActor> left, IEnumerable<CharacterActor> right)
        {
            _allActors.AddRange(left);
            _allActors.AddRange(right);
        }

        public void CreateRoundQueue()
        {
            _allActors.Sort((a, b) => a.State.Stats[EStatType.Speed].Current.Value
                .CompareTo(b.State.Stats[EStatType.Speed].Current.Value));

            _allActors.Reverse();
            
            _lastIndex = 0;
        }

        public bool TryGetNextActor(out CharacterActor actor)
        {
            actor = null;
            
            for (int i = _lastIndex; i < _allActors.Count; i++)
            {
                _lastIndex++;
                
                if (_allActors[i].State.IsAlive())
                {
                    actor = _allActors[i];
                    return true;
                }
            }

            return false;
        }
    }
}