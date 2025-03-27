using UnityEngine;
using UnityEngine.UI;

namespace EdgeStudio.UI.Common
{
    [UIWidget(Dynamic = true,Pool = true)]
    public class BackgroundUI : UIBase
    {
        void Start()
        {
            Reset();
            MakeFullScreen();
            var image = CachedGameObject.AddComponent<Image>();
            image.color = new Color32(0, 0, 0, 230);
            CachedGameObject.AddComponent<Button>();
            CachedTransform.localPosition = Vector3.zero;
            CachedTransform.localScale = Vector3.one;
            CachedTransform.localEulerAngles = Vector3.zero;
        }
    }
}
