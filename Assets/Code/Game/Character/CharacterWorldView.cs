using UnityEngine;

namespace Game.Character
{
    public class CharacterWorldView : MonoBehaviour
    {
        [SerializeField] private Transform _root;
        [SerializeField] private SpriteRenderer[] _renderers;

        
        public void SetPosition(Vector3 position, ETeamId team)
        {
            _root.position = position;
            
            float scaleX = team == ETeamId.Left ? 1 : -1;
            _root.localScale = new Vector3(scaleX,1,1);
        }
        
        public void SetBodyColor(Color color)
        {
            foreach (SpriteRenderer renderer in _renderers)
            {
                renderer.color = color;
            }
        }
    }
}