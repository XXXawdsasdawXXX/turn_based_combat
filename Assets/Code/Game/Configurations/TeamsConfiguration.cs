using UnityEngine;

namespace Game.Configurations
{
    [CreateAssetMenu(fileName = "TeamConfiguration", menuName = "Game/TeamConfiguration")]
    public class TeamsConfiguration : ScriptableObject
    {
        [field: SerializeField] public CharacterConfiguration[] TeamLeft { get; private set; }
        [field: SerializeField] public CharacterConfiguration[] TeamRight { get; private set; }
    }
}