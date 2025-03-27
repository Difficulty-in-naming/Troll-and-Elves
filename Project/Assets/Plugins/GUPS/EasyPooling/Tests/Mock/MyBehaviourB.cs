// Unity
using UnityEngine;

namespace GUPS.EasyPooling.Tests.Mock
{
    /// <summary>
    /// A MonoBehaviour for testing purposes.
    /// </summary>
    public class MyBehaviourB : MonoBehaviour
    {
#pragma warning disable
        [SerializeField]
        private float valueFloat = 4;

        public double valueDouble = 7;
#pragma warning restore
    }
}
