using System;
using Cysharp.Threading.Tasks;
using EdgeStudio.Manager.Panthea.UI;
using Panthea.Common;
using UnityEngine.InputSystem;

namespace EdgeStudio.GUI
{
    public class ShortCutManager : BetterMonoBehaviour
    {
        public InputActionReference OpenBagPack;

        async void Update()
        {
            if (OpenBagPack.action.triggered)
            {
            }
        }
    }
}