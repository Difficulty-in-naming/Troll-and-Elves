using System;
using EdgeStudio.Manager.Audio;
using Panthea.Asset;
using Panthea.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EdgeStudio.UI.Component
{
    public class UIClickSound : BetterMonoBehaviour, IPointerClickHandler
    {
        public AssetReference<AudioClip> audioClip;
        public async void OnPointerClick(PointerEventData eventData)
        {
            try
            {
                var clip = await audioClip.Load(destroyCancellationToken);
                if (clip && this)
                    AudioManager.Inst.PlaySound(clip, max: 1);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
