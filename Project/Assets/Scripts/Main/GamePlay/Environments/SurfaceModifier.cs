using Sirenix.OdinInspector;
using UnityEngine;

namespace EdgeStudio.Environments
{
    /// <summary>
    /// 将此组件添加到平台，并定义其新的摩擦力或力，这些力将应用于任何行走在其上的 TopDownController。
    /// TODO：仍在开发中。
    /// </summary>
    [AddComponentMenu("Edge Studio/Environment/Surface Modifier")]
    public class SurfaceModifier : TopDownMonoBehaviour 
    {
        [Header("摩擦力")]
        [InfoBox("设置一个介于 0.01 和 0.99 之间的摩擦力以获得滑溜的表面（接近 0 非常滑，接近 1 较不滑）。\n或将其设置为大于 1 以获得粘性表面。值越大，表面越粘。")]
        [Tooltip("应用于行走在此表面上的 TopDownController 的摩擦力大小")]
        public float Friction;

        [Header("力")]
        [InfoBox("使用这些选项为任何在此表面上接地的 TopDownController 添加 X 或 Y（或两者）方向的力。添加 X 方向的力将创建一个跑步机（负值 > 向左移动的跑步机，正值 > 向右移动的跑步机）。正值 Y 方向力将创建一个蹦床、弹性表面或跳跃器。")]
        [Tooltip("应用于行走在此表面上的 TopDownController 的附加力大小")]
        public Vector3 AddedForce = Vector3.zero;
    }
}