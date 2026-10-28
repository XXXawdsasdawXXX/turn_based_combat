using Game.Character;
using UnityEngine;

namespace Game.Configurations
{
    [CreateAssetMenu(fileName = "AssetStorage", menuName = "Game/AssetStorage")]
    public class AssetStorage : ScriptableObject
    {
        [field: SerializeField] public CharacterWorldView CharacterWorldView { get; private set; }
    }
}