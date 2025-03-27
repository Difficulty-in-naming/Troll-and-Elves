using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EdgeStudio.Abilities;
using EdgeStudio.GUI;
using EdgeStudio.Odin;
using Panthea.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace EdgeStudio.Environments
{
    /// <summary>
    ///     扩展此类以在特定区域按下按钮时激活某些功能
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Button Activated")]
    public class ButtonActivated : TopDownMonoBehaviour
    {
        public enum ButtonActivatedRequirements
        {
            Character,
            ButtonActivator,
            Either
        }

        [ColorFoldout("Requirements", 10)] [InfoBox("<size=11>在这里你可以指定与此区域交互所需的条件。是否需要ButtonActivation角色能力？是否只能由玩家交互？</size>")] [Tooltip("如果为true，具有ButtonActivator类的对象将能够与此区域交互")]
        public ButtonActivatedRequirements ButtonActivatedRequirement = ButtonActivatedRequirements.Either;

        [ColorFoldout("Requirements", 10)] [Tooltip("如果为true，此区域只能由玩家角色激活")]
        public bool RequiresPlayerType = true;

        [ColorFoldout("Requirements", 10)] [Tooltip("如果为true，此区域只能在角色具有所需能力时激活")]
        public bool RequiresButtonActivationAbility = true;

        [ColorFoldout("Activation Conditions", 11)] [InfoBox("<size=11>在这里你可以指定如何与该区域交互。你可以让它自动激活，只在地面上激活，或完全阻止其激活。</size>")] [Tooltip("如果为false，该区域将无法激活")]
        public bool Activable = true;

        [ColorFoldout("Activation Conditions", 11)] [Tooltip("如果为true，无论是否按下按钮，该区域都会激活")]
        public bool AutoActivation;

        [ColorFoldout("Activation Conditions", 11)] [ShowIf("AutoActivation")] [Tooltip("角色必须在区域内停留多长时间（以秒为单位）才能激活它")]
        public float AutoActivationDelay;


        [ColorFoldout("Activation Conditions", 11)] [Tooltip("如果你希望CharacterBehaviorState被通知玩家进入区域，请将此设置为true。")]
        public bool ShouldUpdateState = true;

        [ColorFoldout("Activation Conditions", 11)] [Tooltip("如果为true，当另一个对象进入时不会重新触发enter，只有当最后一个对象退出时才会触发exit")]
        public bool OnlyOneActivationAtOnce = true;

        [ColorFoldout("Activation Conditions", 11)] [Tooltip("包含所有可以与此特定按钮激活区域交互的图层的图层掩码")]
        public LayerMask TargetLayerMask = ~0;

        [ColorFoldout("Number of Activations", 12)] [InfoBox("<size=11>你可以决定让该区域永远可交互，或只能交互有限次数，并可以指定使用之间的延迟（以秒为单位）。</size>")] [Tooltip("如果设置为false，你的激活次数将为MaxNumberOfActivations")]
        public bool UnlimitedActivations = true;

        [ColorFoldout("Number of Activations", 12)] [Tooltip("该区域可以交互的次数")]
        public int MaxNumberOfActivations;

        [ColorFoldout("Number of Activations", 12)] [Tooltip("此区域剩余的激活次数")] [ReadOnly]
        public int NumberOfActivationsLeft;

        [ColorFoldout("Number of Activations", 12)] [Tooltip("激活后无法再次激活的延迟时间（以秒为单位）")]
        public float DelayBetweenUses;

        [ColorFoldout("Number of Activations", 12)] [Tooltip("如果为true，该区域将在最后一次使用后禁用自己（永久或直到你手动重新激活它）")]
        public bool DisableAfterUse;

        [ColorFoldout("Input", 13)] public InputActionProperty InputSystemAction;

        [ColorFoldout("Visual Prompt", 15)] [InfoBox("<size=11>你可以让这个区域显示一个视觉提示，以向玩家表明它是可交互的。</size>")] [Tooltip("如果为true，将显示一个正确设置的提示")]
        public bool UseVisualPrompt = true;

        [ColorFoldout("Visual Prompt", 15)] [ShowIf("UseVisualPrompt")] [Tooltip("用于显示提示的游戏对象预制体")]
        public ButtonPrompt ButtonPromptPrefab;

        [ColorFoldout("Visual Prompt", 15)] [ShowIf("UseVisualPrompt")] [Tooltip("如果为true，无论玩家是否在区域内，'buttonA'提示都会一直显示。")]
        public bool AlwaysShowPrompt = true;

        [ColorFoldout("Visual Prompt", 15)] [ShowIf("UseVisualPrompt")] [Tooltip("如果为true，当玩家与区域发生碰撞时会显示'buttonA'提示")]
        public bool ShowPromptWhenColliding = true;

        [ColorFoldout("Visual Prompt", 15)] [ShowIf("UseVisualPrompt")] [Tooltip("如果为true，使用后提示将隐藏")]
        public bool HidePromptAfterUse;

        [ColorFoldout("Visual Prompt", 15)] [ShowIf("UseVisualPrompt")] [Tooltip("实际buttonA提示相对于对象中心的位置")] [OnValueChanged("OnPromptRelativePositionChanged")]
        public Vector3 PromptRelativePosition = Vector3.zero;

        [ColorFoldout("Actions", 17)] [Tooltip("该区域激活时触发的UnityEvent")]
        public UnityEvent OnActivation;

        [ColorFoldout("Actions", 17)] [Tooltip("离开该区域时触发的UnityEvent")]
        public UnityEvent OnExit;

        [ColorFoldout("Actions", 17)] [Tooltip("角色在区域内时触发的UnityEvent")]
        public UnityEvent OnStay;

        protected Animation mButtonPromptAnimator;
        protected ButtonPrompt mButtonPrompt;
        protected Collider2D mCollider2D;
        protected bool mPromptHiddenForever;
        protected CharacterButtonActivation mCharacterButtonActivation;
        protected float mLastActivationTimestamp;
        protected List<GameObject> mCollidingObjects;
        protected bool mStaying;

        public virtual bool AutoActivationInProgress { get; set; }
        public virtual float AutoActivationStartedAt { get; set; }
        public bool InputActionPerformed => InputSystemAction.action.WasPressedThisFrame();

        protected virtual void OnEnable() => Initialization();

        private void OnPromptRelativePositionChanged()
        {
            if (mButtonPrompt != null)
            {
                var promptTrans = mButtonPrompt.transform;
                if (mCollider2D != null) promptTrans.position = mCollider2D.bounds.center + PromptRelativePosition;
            }
        }

        public virtual void Initialization()
        {
            mCollider2D = gameObject.GetComponent<Collider2D>();
            NumberOfActivationsLeft = MaxNumberOfActivations;
            mCollidingObjects = new List<GameObject>();

            if (AlwaysShowPrompt) ShowPrompt();

            InputSystemAction.action.Enable();
        }

        protected virtual void OnDisable() => InputSystemAction.action.Disable();

        protected virtual async UniTask TriggerButtonActionAsync()
        {
            if (!(AutoActivationDelay <= 0f))
            {
                AutoActivationInProgress = true;
                AutoActivationStartedAt = Time.time;
                await UniTask.Delay(TimeSpan.FromSeconds(AutoActivationDelay));
                AutoActivationInProgress = false;
            }

            TriggerButtonAction();
        }

        /// <summary>
        ///     当按下输入按钮时，我们检查区域是否可以被激活，如果可以，触发ZoneActivated
        /// </summary>
        public virtual void TriggerButtonAction()
        {
            if (!CheckNumberOfUses())
            {
                PromptError();
                return;
            }

            mStaying = true;
            ActivateZone();
        }

        public virtual void TriggerExitAction(GameObject arg)
        {
            mStaying = false;
            OnExit?.Invoke();
        }

        /// <summary>
        ///     使区域可激活
        /// </summary>
        public virtual void MakeActivable() => Activable = true;

        /// <summary>
        ///     使区域不可激活
        /// </summary>
        public virtual void MakeUnactivable() => Activable = false;

        /// <summary>
        ///     如果区域不可激活则使其可激活，如果可激活则使其不可激活。
        /// </summary>
        public virtual void ToggleActivable() => Activable = !Activable;

        protected virtual void Update()
        {
            if (mStaying) OnStay?.Invoke();
        }

        /// <summary>
        ///     激活区域
        /// </summary>
        protected virtual void ActivateZone()
        {
            OnActivation?.Invoke();

            mLastActivationTimestamp = Time.time;

            if (HidePromptAfterUse)
            {
                mPromptHiddenForever = true;
                HidePrompt();
            }

            NumberOfActivationsLeft--;

            if (DisableAfterUse && NumberOfActivationsLeft <= 0) DisableZone();
        }

        /// <summary>
        ///     触发错误
        /// </summary>
        public virtual void PromptError()
        {
            if (mButtonPromptAnimator != null) mButtonPromptAnimator.Play("Error");
        }

        /// <summary>
        ///     显示按钮A提示。
        /// </summary>
        public virtual void ShowPrompt()
        {
            if (!UseVisualPrompt || mPromptHiddenForever || ButtonPromptPrefab == null) return;

            // 我们在区域顶部添加一个闪烁的A提示
            if (mButtonPrompt == null)
            {
                mButtonPrompt = Instantiate(ButtonPromptPrefab);
                mButtonPrompt.Initialization();
                mButtonPromptAnimator = mButtonPrompt.gameObject.GetComponentNoAlloc<Animation>();
            }

            if (mButtonPrompt != null)
            {
                var promptTrans = mButtonPrompt.transform;
                if (mCollider2D != null) promptTrans.position = mCollider2D.bounds.center + PromptRelativePosition;
                promptTrans.parent = CachedTransform;
                mButtonPrompt.Show();
            }
        }

        /// <summary>
        ///     隐藏按钮A提示。
        /// </summary>
        public virtual void HidePrompt()
        {
            if (mButtonPrompt != null) mButtonPrompt.Hide();
        }

        /// <summary>
        ///     禁用按钮激活区域
        /// </summary>
        public virtual void DisableZone()
        {
            Activable = false;

            if (mCollider2D != null) mCollider2D.enabled = false;

            if (ShouldUpdateState && mCharacterButtonActivation != null)
            {
                mCharacterButtonActivation.InButtonActivatedZone = false;
                mCharacterButtonActivation.ButtonActivatedZone = null;
            }
        }

        /// <summary>
        ///     启用按钮激活区域
        /// </summary>
        public virtual void EnableZone()
        {
            Activable = true;

            if (mCollider2D != null) mCollider2D.enabled = true;
        }

        protected virtual void OnTriggerEnter2D(Collider2D collidingObject) => TriggerEnter(collidingObject.gameObject);

        protected virtual void OnTriggerExit2D(Collider2D collidingObject) => TriggerExit(collidingObject.gameObject);

        /// <summary>
        ///     当有物体与按钮激活区域发生碰撞时触发
        /// </summary>
        protected virtual void TriggerEnter(GameObject arg)
        {
            if (!CheckConditions(arg)) return;

            // 此时物体正在碰撞并且已授权，我们将其添加到我们的列表中
            mCollidingObjects.Add(arg);
            if (!TestForLastObject(arg)) return;

            if (ShouldUpdateState)
            {
                mCharacterButtonActivation = arg.GetComponent<Character>()?.FindAbility<CharacterButtonActivation>();
                if (mCharacterButtonActivation != null)
                {
                    mCharacterButtonActivation.InButtonActivatedZone = true;
                    mCharacterButtonActivation.ButtonActivatedZone = this;
                    mCharacterButtonActivation.InButtonAutoActivatedZone = AutoActivation;
                }
            }

            if (AutoActivation) TriggerButtonActionAsync().Forget();

            // 如果我们还没有显示提示并且区域可以被激活，我们就显示它
            if (ShowPromptWhenColliding) ShowPrompt();
        }

        /// <summary>
        ///     当有物体离开时触发
        /// </summary>
        protected virtual void TriggerExit(GameObject arg)
        {
            if (!CheckConditions(arg)) return;

            mCollidingObjects.Remove(arg);
            if (!TestForLastObject(arg)) return;

            AutoActivationInProgress = false;

            if (ShouldUpdateState)
            {
                mCharacterButtonActivation = arg.GetComponent<Character>()?.FindAbility<CharacterButtonActivation>();
                if (mCharacterButtonActivation != null)
                {
                    mCharacterButtonActivation.InButtonActivatedZone = false;
                    mCharacterButtonActivation.ButtonActivatedZone = null;
                }
            }

            if (mButtonPrompt != null && !AlwaysShowPrompt) HidePrompt();

            TriggerExitAction(arg);
        }

        /// <summary>
        ///     测试离开我们区域的物体是否是最后一个剩余的物体
        /// </summary>
        protected virtual bool TestForLastObject(GameObject arg)
        {
            if (OnlyOneActivationAtOnce)
            {
                if (mCollidingObjects.Count > 0)
                {
                    var lastObject = true;
                    foreach (var obj in mCollidingObjects)
                    {
                        if (obj != null && obj != arg)
                            lastObject = false;
                    }

                    return lastObject;
                }
            }

            return true;
        }

        /// <summary>
        ///     检查剩余的使用次数和可能的使用间隔，如果区域可以被激活则返回true。
        /// </summary>
        /// <returns><c>true</c>，如果检查了使用次数，否则为<c>false</c>。</returns>
        public virtual bool CheckNumberOfUses()
        {
            if (!Activable) return false;

            if (Time.time - mLastActivationTimestamp < DelayBetweenUses) return false;

            if (UnlimitedActivations) return true;

            if (NumberOfActivationsLeft == 0) return false;

            return NumberOfActivationsLeft > 0;
        }

        /// <summary>
        ///     确定是否应该激活此区域
        /// </summary>
        /// <returns><c>true</c>，如果检查了条件，否则为<c>false</c>。</returns>
        protected virtual bool CheckConditions(GameObject arg)
        {
            if (!LayerManager.LayerInLayerMask(arg.layer, TargetLayerMask)) return false;

            var character = arg.gameObject.GetComponent<Character>();

            switch (ButtonActivatedRequirement)
            {
                case ButtonActivatedRequirements.Character:
                    if (character == null) return false;
                    break;

                case ButtonActivatedRequirements.ButtonActivator:
                    if (arg.gameObject.GetComponent<ButtonActivator>() == null) return false;
                    break;

                case ButtonActivatedRequirements.Either:
                    if (character == null && arg.gameObject.GetComponent<ButtonActivator>() == null) return false;
                    break;
            }

            if (RequiresPlayerType)
            {
                if (character == null) return false;
            }

            if (RequiresButtonActivationAbility)
            {
                var characterButtonActivation = arg.gameObject.GetComponent<Character>()?.FindAbility<CharacterButtonActivation>();
                // 我们检查与水碰撞的对象实际上是一个TopDown控制器和一个角色
                if (characterButtonActivation == null) return false;

                if (!characterButtonActivation.AbilityAuthorized) return false;
            }

            return true;
        }
    }
}