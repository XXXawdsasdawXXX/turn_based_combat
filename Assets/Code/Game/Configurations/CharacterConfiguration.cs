using Game.Abilities;
using Game.Stats;
using TriInspector;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Configurations
{
    [CreateAssetMenu(fileName = "CharacterConfiguration", menuName = "Game/CharacterConfiguration")]
    public class CharacterConfiguration : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; }

        [field: SerializeField] public Color Color = Color.white;

        [field: SerializeField] public Stat[] Stats { get; private set; } = 
        {
            new(EStatType.Health, 10),
            new(EStatType.Speed, 5),
            new(EStatType.Attack, 1),
            new(EStatType.Defence, 0.7f),
        };
        
        [field: SerializeField] public Ability[] Abilities { get; private set; }
        
        
        public bool TryGetStatValue(EStatType type, out float value)
        {
            value = 0;
            
            foreach (Stat stat in Stats)
            {
                if (stat.Type == type)
                {
                    value = stat.BaseValue;
                    return true;
                }
            }

            return false;
        }

        public float GetStatValue(EStatType type)
        {
            foreach (Stat stat in Stats)
            {
                if (stat.Type == type)
                {
                    return stat.BaseValue;
                }
            }

            return 0;
        }
        
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

        [Button]
        private void _randomizeStat(EStatType type, float minValue = 0, float maxValue = 1)
        {
            bool isFindStat = false;
            
            foreach (Stat stat in Stats)
            {
                if (stat.Type == type)
                {
                    stat._setValue(Random.Range(minValue, maxValue));
                    isFindStat = true;
                    Undo.RecordObject(this, "Randomize stat");
                    break;
                }
            }

            if (!isFindStat)
            {
                Debug.LogWarning($"Not found stat with type {type}");
                return;
            }
            
            EditorUtility.SetDirty(this);
        }
        
#endif
    }
}