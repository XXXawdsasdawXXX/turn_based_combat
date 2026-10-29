using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Abilities;
using Game.Character;

namespace Game.Runtime
{
    public class ActionRunner
    {
        public async UniTask StartAction(CharacterActor actor, List<CharacterActor> targets, Ability ability)
        {
            await actor.WorldView.Animation.PlayUseAbility();
            
            actor.WorldView.Animation.PlayIdle();
            
            List<UniTask> _impacts = new();
            
            foreach (CharacterActor target in targets)
            {
                if (target.State.Stats.ContainsKey(ability.EffectValue.Type))
                {
                    target.State.Stats[ability.EffectValue.Type].Current.Value += ability.EffectValue.BaseValue;

                    if (ability.IsAttacking())
                    {
                        _impacts.Add(target.WorldView.Animation.PlayTakeDamage());
                    }
                }
            }

            await UniTask.WhenAll(_impacts);

            if (ability.IsAttacking())
            {
                _impacts.Clear();
               
                foreach (CharacterActor target in targets)
                {
                    if (!target.State.IsAlive())
                    {
                        _impacts.Add(target.WorldView.Animation.PlayDie());
                    }
                    else
                    {
                        target.WorldView.Animation.PlayIdle();
                    }
                }
                
                await UniTask.WhenAll(_impacts);
            }
        }
    }
}