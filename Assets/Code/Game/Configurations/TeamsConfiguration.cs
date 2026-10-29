using UnityEngine;

namespace Game.Configurations
{
    [CreateAssetMenu(fileName = "TeamConfiguration", menuName = "Game/TeamConfiguration")]
    public class TeamsConfiguration : ScriptableObject
    {
        [field: SerializeField] public CharacterConfiguration[] Player { get; private set; }
        [field: SerializeField] public CharacterConfiguration[] Enemy { get; private set; }
    }
}