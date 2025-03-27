// Unity
using UnityEngine;

// GUPS
using GUPS.EasyPooling.Decorator;

namespace GUPS.EasyPooling.Tests.Mock
{
    /// <summary>
    /// A decorator for testing purposes.
    /// </summary>
    public class MyDecorator : IDecorator
    {
        public bool OnCreateOnly => false;

        public void OnDecorate(GameObject _GameObject)
        {
            // Add a MyBehaviourC component.
            MyBehaviourC var_MyBehaviourC = _GameObject.AddComponent<MyBehaviourC>();

            // Set the values of the MyBehaviourC component.
            var_MyBehaviourC.valueDouble = 123;
            var_MyBehaviourC.valueString = "Nice!";
        }
    }
}
