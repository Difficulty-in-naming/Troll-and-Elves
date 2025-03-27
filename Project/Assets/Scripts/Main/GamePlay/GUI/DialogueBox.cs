using EdgeStudio.Odin;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EdgeStudio.GUI
{
	/// <summary>
	/// 对话框类。不要直接添加到游戏中，请使用 DialogueZone 代替。
	/// </summary>
	public sealed class DialogueBox : TopDownMonoBehaviour
	{
		[ColorFoldout("对话框"), Tooltip("文本面板背景")]
		public CanvasGroup TextPanelCanvasGroup;
		[ColorFoldout("对话框"), Tooltip("要显示的文本")]
		public TextMeshProUGUI DialogueText;
		[ColorFoldout("对话框"), Tooltip("按钮A提示")]
		public CanvasGroup Prompt;
		[ColorFoldout("对话框"), Tooltip("需要着色的图片列表")]
		public Image[] ColorImages;

		public void ChangeText(string newText) => DialogueText.text = newText;

		public void ButtonActive(bool state) => Prompt.gameObject.SetActive(state);

		/// <summary>
		/// 淡入对话框
		/// </summary>
		public void FadeIn(float duration)
		{
			if (TextPanelCanvasGroup != null) LMotion.Create(0f, 1f, duration).BindToAlpha(TextPanelCanvasGroup).AddTo(this);
			if (DialogueText != null) LMotion.Create(DialogueText.color, Color.white, duration).BindToColor(DialogueText).AddTo(this);
			if (Prompt != null) LMotion.Create(0f, 1f, duration).BindToAlpha(Prompt).AddTo(this);
		}

		/// <summary>
		/// 淡出对话框
		/// </summary>
		public void FadeOut(float duration)
		{
			if (TextPanelCanvasGroup != null) LMotion.Create(1f, 0f, duration).BindToAlpha(TextPanelCanvasGroup).AddTo(this);
			if (DialogueText != null) LMotion.Create(DialogueText.color, Color.clear, duration).BindToColor(DialogueText).AddTo(this);
			if (Prompt != null) LMotion.Create(1f, 0f, duration).BindToAlpha(Prompt).AddTo(this);
		}
	}
}