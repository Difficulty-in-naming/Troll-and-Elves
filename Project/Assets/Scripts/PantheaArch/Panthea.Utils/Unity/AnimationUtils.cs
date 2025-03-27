using Cysharp.Threading.Tasks;
using Panthea.Common;
using UnityEngine;

namespace Panthea.Utils
{
    public static class AnimationUtils
    {
        public static async UniTask WaitComplete(this Animation animation, string name = null)
        {
            await UniTask.NextFrame();
            if (!string.IsNullOrEmpty(name))
            {
                while (animation.IsPlaying(name))
                    await UniTask.NextFrame();
            }
            else
            {
                while(animation.isPlaying)
                    await UniTask.NextFrame();
            }
        }
        
        public static void PlayForward(this Animation animation, string clipName = null, float speed = 1.0f)
        {
            if (!animation)
            {
                Log.Error("Animation or clip name is null or empty.");
                return;
            }
            
            if (string.IsNullOrEmpty(clipName))
            {
                foreach (AnimationState node in animation)
                {
                    if (node.clip == animation.clip)
                    {
                        node.speed = Mathf.Abs(speed);
                        animation.Play();
                        return;
                    }
                }
                Log.Error("Animation or clip name is null or empty.");

            }
            else
            {
                var animationState = animation[clipName];
                if (animationState == null)
                {
                    Log.Error("Animation or clip name is null or empty.");
                    return;
                }

                animationState.speed = Mathf.Abs(speed);
                animation.Play(animationState.name);
            }
        }

        public static void PlayReverse(this Animation animation, string clipName = null, float speed = 1.0f)
        {
            if (!animation)
            {
                Log.Error("Animation or clip name is null or empty.");
                return;
            }
            if (string.IsNullOrEmpty(clipName))
            {
                foreach (AnimationState node in animation)
                {
                    if (node.clip == animation.clip)
                    {
                        node.speed = -Mathf.Abs(speed);
                        if (!animation.IsPlaying(clipName))
                        {
                            node.time = node.length;
                            animation.Play();
                        }
                    }
                }
                Log.Error("Animation or clip name is null or empty.");
            }
            else
            {
                var animationState = animation[clipName];
                if (animationState == null)
                {
                    Log.Error("Animation or clip name is null or empty.");
                    return;
                }

                animationState.speed = -Mathf.Abs(speed);
                if (!animation.IsPlaying(clipName))
                {
                    animationState.time = animationState.length;
                    animation.Play(clipName);
                }
            }
        }
    }
}