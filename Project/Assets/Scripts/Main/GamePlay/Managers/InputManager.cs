using System.Collections.Generic;
using Panthea.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace EdgeStudio
{
    public class InputManager : MonoPersistentSingleton<InputManager>
    {
        [SerializeField] private InputActionAsset InputActions;
        public InputActionProperty Run;
        [FormerlySerializedAs("DashAction")] public InputActionProperty Dash;

        public Vector2 Threshold = new(0.1f, 0.4f);

        [SerializeField] private InputActionProperty PrimaryMovement;
        [SerializeField] private InputActionProperty SecondaryMovement;
        public Vector2 PrimaryMovementInput;
        public Vector2 SecondaryMovementInput;
        /// 主要移动值（用于移动角色）
        public Vector2 LastNonNullPrimaryMovement { get; set; }

        /// 次要移动（通常是手柄上的右摇杆），用于瞄准
        public Vector2 LastNonNullSecondaryMovement { get; set; }

        public Vector2 MousePosition => Mouse.current.position.ReadValue();

        private void Awake()
        {
            PrimaryMovement.action.performed += context => PrimaryMovementInput = context.ReadValue<Vector2>();
            PrimaryMovement.action.canceled += context => PrimaryMovementInput = context.ReadValue<Vector2>();
            SecondaryMovement.action.performed += context => SecondaryMovementInput = context.ReadValue<Vector2>();
            SecondaryMovement.action.canceled += context => SecondaryMovementInput = context.ReadValue<Vector2>();
        }

        private void Update() => GetLastNonNullValues();

        /// <summary>
        /// 获取主要和次要轴的最后非空值
        /// </summary>
        protected virtual void GetLastNonNullValues()
        {
            if (PrimaryMovementInput.magnitude > Threshold.x)
            {
                LastNonNullPrimaryMovement = PrimaryMovementInput;
            }

            if (SecondaryMovementInput.magnitude > Threshold.x)
            {
                LastNonNullSecondaryMovement = SecondaryMovementInput;
            }
        }
        
        /// <summary>
        /// 应用鼠标位置偏移量
        /// </summary>
        /// <param name="offset">要应用的偏移量</param>
        public void ApplyMouseOffset(Vector2 offset)
        {
            Vector2 newPosition = MousePosition + offset;
            Mouse.current.WarpCursorPosition(newPosition);
        }
    }
}