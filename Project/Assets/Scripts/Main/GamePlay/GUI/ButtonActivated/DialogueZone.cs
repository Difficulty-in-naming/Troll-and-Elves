using System;
using EdgeStudio.Environments;
using UnityEngine;
using Cysharp.Threading.Tasks;
using EdgeStudio.Odin;
using System.Linq;

namespace EdgeStudio.GUI
{
    namespace MoreMountains.TopDownEngine
    {
        [Serializable]
        public class DialogueSequence
        {
            [Range(0, 100)]
            public float Weight = 1f;
            public DialogueElement[] Elements;
        }

        [Serializable]
        public class DialogueElement
        {
            [Multiline] 
            public string DialogueLine;
        }

        /// <summary>
        /// 将此类添加到空组件上。它需要一个设置为"is trigger"的Collider或Collider2D。
        /// 然后，您可以通过检查器自定义对话区域。
        /// </summary>
        [AddComponentMenu("TopDown Engine/GUI/Dialogue Zone")]
        public class DialogueZone : ButtonActivated
        {
            [ColorFoldout("对话外观", 18), Tooltip("用于显示对话的预制体")]
            public DialogueBox DialogueBoxPrefab;
            
            [ColorFoldout("对话速度（以秒为单位）", 19), Tooltip("淡入淡出的持续时间")]
            public float FadeDuration = 0.2f;
            [ColorFoldout("对话速度（以秒为单位）", 19), Tooltip("两个对话之间的时间")]
            public float TransitionTime = 0.2f;

            [ColorFoldout("对话位置", 20), Tooltip("对话框应该出现在碰撞器顶部的距离")]
            public Vector3 Offset = Vector3.zero;

            [ColorFoldout("玩家移动", 21), Tooltip("如果设置为true，角色将能够在对话进行时移动")]
            public bool CanMoveWhileTalking = true;

            [ColorFoldout("按按钮从一条消息转到下一条？", 22), Tooltip("此区域是否由按钮处理")]
            public bool ButtonHandled = true;

            [ColorFoldout("按按钮从一条消息转到下一条？", 22), Header("仅当对话不是由按钮处理时："), Range(1, 100), Tooltip("消息应显示的持续时间，以秒为单位。仅在对话框不是由按钮处理时考虑")]
            public float MessageDuration = 3f;

            [ColorFoldout("激活", 23), Tooltip("如果可以激活多次则为true")]
            public bool ActivableMoreThanOnce = true;

            [ColorFoldout("激活", 23), Range(1, 100), Tooltip("如果区域可以激活多次，它应该在两次激活之间保持不活动多长时间？")] 
            public float InactiveTime = 2f;

            [ColorFoldout("对话序列")]
            public DialogueSequence[] DialogueSequences;

            protected DialogueBox mDialogueBox;
            protected bool mActivated;
            protected bool mPlaying;
            protected int mCurrentIndex;
            protected bool mActivable = true;
            protected DialogueElement[] mCurrentDialogue;

            protected override void OnEnable()
            {
                base.OnEnable();
                mCurrentIndex = 0;
            }

            protected virtual DialogueElement[] SelectRandomDialogueSequence()
            {
                if (DialogueSequences == null || DialogueSequences.Length == 0)
                    return new DialogueElement[0];

                float totalWeight = DialogueSequences.Sum(seq => seq.Weight);
                float randomPoint = UnityEngine.Random.Range(0f, totalWeight);
                float currentWeight = 0f;

                foreach (var sequence in DialogueSequences)
                {
                    currentWeight += sequence.Weight;
                    if (randomPoint <= currentWeight)
                        return sequence.Elements;
                }

                return DialogueSequences[0].Elements;
            }

            /// <summary>
            /// 当按下按钮时，我们开始对话
            /// </summary>
            public override void TriggerButtonAction()
            {
                if (!CheckNumberOfUses())
                    return;

                if (mPlaying && !ButtonHandled)
                    return;

                base.TriggerButtonAction();
                StartDialogue().Forget();
            }

            /// <summary>
            /// 当触发时，无论是通过按钮按下还是简单地进入区域，都开始对话
            /// </summary>
            public virtual async UniTaskVoid StartDialogue()
            {
                if (mCollider2D == null || (mActivated && !ActivableMoreThanOnce) || !mActivable)
                    return;

                if (!CanMoveWhileTalking)
                {
                    if (ShouldUpdateState && (mCharacterButtonActivation != null))
                    {
                        mCharacterButtonActivation.GetComponentInParent<Character>().MovementState.ChangeState(CharacterStates.MovementStates.Idle);
                    }
                }

                if (!mPlaying)
                {
                    mDialogueBox = Instantiate(DialogueBoxPrefab, this.gameObject.transform, true);
                    mDialogueBox.transform.localPosition = Vector3.zero + Offset;
                    mPlaying = true;
                    mCurrentDialogue = SelectRandomDialogueSequence();
                }

                await PlayNextDialogue();
            }

            protected override void OnTriggerExit2D(Collider2D collider)
            {
                base.OnTriggerExit2D(collider);
                
                if (mDialogueBox != null)
                {
                    CleanupDialogue();
                }
            }

            protected virtual void CleanupDialogue()
            {
                if (mDialogueBox != null)
                {
                    Destroy(mDialogueBox.gameObject);
                    mDialogueBox = null;
                }

                mPlaying = false;
                mCurrentIndex = 0;

                if (mCharacterButtonActivation != null)
                {
                    mCharacterButtonActivation.InButtonActivatedZone = false;
                    mCharacterButtonActivation.ButtonActivatedZone = null;
                }
            }

            /// <summary>
            /// 打开或关闭碰撞器
            /// </summary>
            protected virtual void EnableCollider(bool status)
            {
                if (mCollider2D != null)
                {
                    mCollider2D.enabled = status;
                }
            }

            /// <summary>
            /// 播放队列中的下一个对话
            /// </summary>
            protected virtual async UniTask PlayNextDialogue()
            {
                if (mDialogueBox == null)
                    return;

                if (mCurrentIndex != 0)
                {
                    mDialogueBox.FadeOut(FadeDuration);
                    await UniTask.Delay(TimeSpan.FromSeconds(TransitionTime));
                }

                if (mCurrentIndex >= mCurrentDialogue.Length)
                {
                    mCurrentIndex = 0;
                    CleanupDialogue();
                    mActivated = true;
                    
                    if (ActivableMoreThanOnce)
                    {
                        mActivable = false;
                        await Reactivate();
                    }
                    else
                    {
                        gameObject.SetActive(false);
                    }
                    return;
                }

                if (mDialogueBox != null && mDialogueBox.DialogueText != null)
                {
                    mDialogueBox.FadeIn(FadeDuration);
                    mDialogueBox.DialogueText.text = mCurrentDialogue[mCurrentIndex].DialogueLine;
                }

                mCurrentIndex++;

                if (!ButtonHandled)
                {
                    await AutoNextDialogue();
                }
            }

            /// <summary>
            /// 自动转到下一行对话
            /// </summary>
            protected virtual async UniTask AutoNextDialogue()
            {
                await UniTask.Delay(TimeSpan.FromSeconds(MessageDuration));
                await PlayNextDialogue();
            }

            /// <summary>
            /// 重新激活对话区域
            /// </summary>
            protected virtual async UniTask Reactivate()
            {
                await UniTask.Delay(TimeSpan.FromSeconds(InactiveTime));
                EnableCollider(true);
                mActivable = true;
                mPlaying = false;
                mCurrentIndex = 0;
                mPromptHiddenForever = false;

                if (AlwaysShowPrompt)
                {
                    ShowPrompt();
                }
            }
        }
    }
}

