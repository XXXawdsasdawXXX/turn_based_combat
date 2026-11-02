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

        private const float DefaultHeadLocalY = 2.5f;
        private const float TargetHeadLocalY = 2.2f;
        private static readonly Vector3 DefaultBodyLocalPosition = Vector3.zero;
        private static readonly Quaternion DefaultBodyLocalRotation = Quaternion.identity;

        private Sequence _sequence;
        
        
        public void PlayIdle()
        {
            _sequence?.Kill();
            _resetPose();
            _sequence = DOTween.Sequence();
            
            const float speed = 4f;

            
            if (_body != null)
            {
                _sequence.Append(_head.DOLocalMoveY(DefaultHeadLocalY, speed))
                    .SetSpeedBased()
                    .SetEase(Ease.Linear);
            }

            if (_head != null)
            {
                _sequence.Append(_head.DOLocalMoveY(TargetHeadLocalY, speed))
                    .SetSpeedBased()
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }

        public async UniTask PlayDie(CancellationToken token = default)
        {
            _sequence?.Kill();
            
            if (_body == null)
            {
                return;
            }
            
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

            if (_body == null)
            {
                return;
            }
            
            _sequence = DOTween.Sequence();
            
            Tween fade = _body.DOLocalMoveY(0.5f, 0.5f).SetLoops(2, LoopType.Yoyo);
            
            _sequence.Append(fade);

            await _awaitSequence(token);
        }

        public async UniTask PlayTakeDamage(CancellationToken token = default)
        {
            _sequence?.Kill();
           
            if (_body == null)
            {
                return;
            }
            
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
            _resetPose();
        }

        private void _resetPose()
        {
            if (_body != null)
            {
                _body.localPosition = DefaultBodyLocalPosition;
                _body.localRotation = DefaultBodyLocalRotation;
            }

            if (_head != null)
            {
                Vector3 headLocalPosition = _head.localPosition;
                headLocalPosition.y = DefaultHeadLocalY;
                _head.localPosition = headLocalPosition;
            }

            if (_renderers == null)
            {
                return;
            }

            foreach (SpriteRenderer renderer in _renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                Color color = renderer.color;
                color.a = 1f;
                renderer.color = color;
            }
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
