using System;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "CharacterConfiguration", menuName = "Game/CharacterConfiguration")]
    public class CharacterConfiguration : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public Stat[] Stats { get; private set; }
    }
}