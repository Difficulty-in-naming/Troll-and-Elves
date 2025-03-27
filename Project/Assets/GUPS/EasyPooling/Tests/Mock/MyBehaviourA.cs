// Unity
using UnityEngine;

namespace GUPS.EasyPooling.Tests.Mock
{
    /// <summary>
    /// A MonoBehaviour for testing purposes.
    /// </summary>
    public class MyBehaviourA : MonoBehaviour
    {
#pragma warning disable
        [SerializeField]
        private int valueInt = 42;

        public string valueString = "Hello World!";
#pragma warning restore
    }
}
