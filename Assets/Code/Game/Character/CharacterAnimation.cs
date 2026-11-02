using System;
using System.Threading;
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
        }

        public async UniTask PlayDie(CancellationToken token = default)
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence();
            
            foreach (SpriteRenderer renderer in _renderers)
            {
                Tween fade = renderer.DOFade(0.3f, 1f);
                _sequence.Join(fade);
            }
            
            await _awaitSequence(token);
        }

        public async UniTask PlayUseAbility(CancellationToken token = default)
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            Tween fade = _body.DOLocalMoveY(0.5f, 0.5f).SetLoops(2, LoopType.Yoyo);
            
            _sequence.Append(fade);

            await _awaitSequence(token);
        }

        public async UniTask PlayTakeDamage(CancellationToken token = default)
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence();
            
            _sequence.Append(_body.DOShakeRotation(1, 30));

            await _awaitSequence(token);
        }

        public void Pause()
        {
            _sequence?.Pause();
        }

        public void Resume()
        {
            _sequence?.Play();
        }

        public void Dispose()
        {
            if (_sequence != null && _sequence.IsActive())
            {
                _sequence.Kill();
            }

            _sequence = null;
        }

        private async UniTask _awaitSequence(CancellationToken token)
        {
            try
            {
                await _sequence.ToUniTask(TweenCancelBehaviour.CancelAwait, token);
            }
            catch (OperationCanceledException)
            {
                Dispose();
                throw;
            }
        }
    }
}
#pragma warning restore CS4014
