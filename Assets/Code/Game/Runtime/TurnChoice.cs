using Game.Abilities;
using Game.Character;

namespace Game.Runtime
{
    public readonly struct TurnChoice
    {
        public readonly Ability Ability;
        public readonly CharacterActor Target; 

        public TurnChoice(Ability ability, CharacterActor target)
        {
            Ability = ability;
            Target = target;
        }

        public static TurnChoice Skip() => new(Ability.Skip(), null);
    }
}