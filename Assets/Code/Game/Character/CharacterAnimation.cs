using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

#pragma warning disable CS4014
namespace Game.Character
{
    [Serializable]
    public class CharacterAnimation
    {
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _body;
        [SerializeField] private SpriteRenderer[] _renderers;

        private Sequence _sequence;
        
        
        public void PlayIdle()
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence();
            
            const float defaultHeadPosition = 2.5f;
            const float targetHeadPosition = 2.2f;
            const float speed = 4f;
            
            _sequence.Append(_head.DOLocalMoveY(defaultHeadPosition, speed))
                .SetSpeedBased()
                .SetEase(Ease.Linear);
            
            _sequence.Append(_head.DOLocalMoveY(targetHeadPosition, speed))
                .SetSpeedBased()
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Yoyo);
            
            _sequence.ToUniTask();
        }

        public async UniTask PlayDie()
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence();
            
            foreach (SpriteRenderer renderer in _renderers)
            {
                Tween fade = renderer.DOFade(0.3f, 1f);
                _sequence.Join(fade);
            }
            
            await _sequence.ToUniTask();
        }

        public async UniTask PlayUseAbility()
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            Tween fade = _body.DOLocalMoveY(0.5f, 0.5f).SetLoops(2, LoopType.Yoyo);
            
            _sequence.Append(fade);

            await _sequence.ToUniTask();
        }

        public async UniTask PlayTakeDamage()
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence();
            
            await _sequence.Append(_body.DOShakeRotation(1, 30));

            await _sequence.ToUniTask();
        }

        public void Dispose()
        {
            _sequence?.Kill();
        }
    }
}
#pragma warning restore CS4014
