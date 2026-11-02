using Game.Abilities;

namespace Game.Runtime
{
    public readonly struct CombatLogEntry
    {
        public int Round { get; }
        public string ActorName { get; }
        public string TargetName { get; }
        public EAbilityType AbilityType { get; }
        public EActionOutcomeKind Kind { get; }
        public float StatDelta { get; }

        public CombatLogEntry(
            int round,
            string actorName,
            string targetName,
            EAbilityType abilityType,
            EActionOutcomeKind kind,
            float statDelta)
        {
            Round = round;
            ActorName = actorName;
            TargetName = targetName;
            AbilityType = abilityType;
            Kind = kind;
            StatDelta = statDelta;
        }
    }
}
