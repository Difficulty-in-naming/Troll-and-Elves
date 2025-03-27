using LitMotion;
using LitMotion.Extensions;
using Panthea.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EdgeStudio.GUI
{
	[AddComponentMenu("TopDown Engine/GUI/Button Prompt")]
	public class ButtonPrompt : BetterMonoBehaviour
	{
		[Tooltip("用作提示边框的图像"),BindField]
		public Image Frame;
		[BindField, Tooltip("用作背景的图像")]
		public Image Background;
		[BindField, Tooltip("提示容器的 CanvasGroup 组件")]
		public CanvasGroup Container;
		[BindField, Tooltip("提示文本组件")]
		public TextMeshProUGUI Text;

		[Header("持续时间")]
		[Tooltip("淡入持续时间（秒）")]
		public float FadeInDuration = 0.2f;
		[Tooltip("淡出持续时间（秒）")]
		public float FadeOutDuration = 0.2f;

		private MotionHandle mHandle;
		public virtual void Initialization() => Container.alpha = 0f;

		public virtual void SetText(string newText) => Text.text = newText;

		public virtual void SetBackgroundColor(Color newColor) => Background.color = newColor;

		public virtual void SetTextColor(Color newColor) => Text.color = newColor;

		public virtual void Show()
		{
			CachedGameObject.SetActive(true);
			if (mHandle.IsActive())
			{
				mHandle.Cancel();
			}
			Container.alpha = 0f;
			mHandle = LMotion.Create(0f, 1f, FadeInDuration).BindToAlpha(Container).AddTo(this);
		}

		public virtual void Hide()
		{
			if (!CachedGameObject.activeInHierarchy)
			{
				return;
			}
			Container.alpha = 1f;
			mHandle = LMotion.Create(1f, 0f, FadeOutDuration).WithOnComplete(() => CachedGameObject.SetActive(false)).BindToAlpha(Container).AddTo(this);
		}
	}
}