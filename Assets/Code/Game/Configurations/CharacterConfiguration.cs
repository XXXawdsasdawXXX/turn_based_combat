using Game.Stats;
using TriInspector;
using UnityEditor;
using UnityEngine;

namespace Game.Configurations
{
    [CreateAssetMenu(fileName = "CharacterConfiguration", menuName = "Game/CharacterConfiguration")]
    public class CharacterConfiguration : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; }

        [field: SerializeField] public Stat[] Stats { get; private set; } = 
        {
            new(EStatType.Health, 10),
            new(EStatType.Speed, 5),
            new(EStatType.Attack, 1),
            new(EStatType.Defence, 0.7f),
        };
        
        [field: SerializeField] public Color Color = Color.white;

        
#if UNITY_EDITOR
        
        [Button]
        private void _setRandomColor()
        {
            Undo.RecordObject(this, "Set Random Color");

            Color = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                1f
            );

            EditorUtility.SetDirty(this);
        }
        
#endif
    }
}