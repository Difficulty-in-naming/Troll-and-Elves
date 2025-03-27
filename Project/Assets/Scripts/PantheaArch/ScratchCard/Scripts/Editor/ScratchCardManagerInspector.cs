using ScratchCardAsset.Core;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

namespace ScratchCardAsset.Editor
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(ScratchCardManager))]
	public class ScratchCardManagerInspector : UnityEditor.Editor
	{
		#region Fields

		private SerializedProperty card;
		private SerializedProperty progress;
		private SerializedProperty imageCard;
		private SerializedProperty hasAlpha;
		private SerializedProperty mode;
		private SerializedProperty camera;
		private SerializedProperty scratchSprite;
		private SerializedProperty progressAccuracy;
		private SerializedProperty brushTexture;
		private SerializedProperty brushSize;
		private SerializedProperty brushOpacity;
		private SerializedProperty inputEnabled;
		private SerializedProperty usePressure;
		private SerializedProperty checkCanvasRaycasts;
		private SerializedProperty canvasesForRaycastsBlocking;
		private SerializedProperty maskShader;
		private SerializedProperty brushShader;
		private SerializedProperty maskProgressShader;
		private ScratchCardManager cardManager;
		private EraseProgress eraseProgress;
		private bool inputBlockShow;
		private bool cardParametersBlockShow;

		#endregion

		private const string InputBlockPrefsKey = "ScratchCardManager.InputBlock";
		private const string CardParametersBlockPrefsKey = "ScratchCardManager.CardParametersBlock";

		void OnEnable()
		{
			//general
			card = serializedObject.FindProperty("Card");
			progress = serializedObject.FindProperty("Progress");
			imageCard = serializedObject.FindProperty("canvasRendererCard");
			hasAlpha = serializedObject.FindProperty("scratchSurfaceSpriteHasAlpha");

			//scratch card parameters
			mode = serializedObject.FindProperty("mode");
			camera = serializedObject.FindProperty("mainCamera");
			scratchSprite = serializedObject.FindProperty("scratchSurfaceSprite");
			progressAccuracy = serializedObject.FindProperty("progressAccuracy");
			brushTexture = serializedObject.FindProperty("brushTexture");
			brushSize = serializedObject.FindProperty("brushSize");
			brushOpacity = serializedObject.FindProperty("brushOpacity");
			
			//input
			inputEnabled = serializedObject.FindProperty("inputEnabled");
			usePressure = serializedObject.FindProperty("usePressure");
			checkCanvasRaycasts = serializedObject.FindProperty("checkCanvasRaycasts");
			canvasesForRaycastsBlocking = serializedObject.FindProperty("canvasesForRaycastsBlocking");

			//shaders
			maskShader = serializedObject.FindProperty("maskShader");
			brushShader = serializedObject.FindProperty("brushShader");
			maskProgressShader = serializedObject.FindProperty("maskProgressShader");

			inputBlockShow = EditorPrefs.GetBool(InputBlockPrefsKey, true);
			cardParametersBlockShow = EditorPrefs.GetBool(CardParametersBlockPrefsKey, true);
		}

		public override bool RequiresConstantRepaint()
		{
			return cardManager != null && cardManager.Card != null && cardManager.Card.RenderTexture != null && cardManager.Card.IsScratched;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			cardManager = (ScratchCardManager)target;
			
			#region ScratchCardManager Parameters
			
			EditorGUI.BeginDisabledGroup(Application.isPlaying);
			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.PropertyField(card, new GUIContent("Scratch Card"));
			if (card.objectReferenceValue == null)
			{
				EditorGUILayout.HelpBox("Scratch Card is null, please set reference to Scratch Card", MessageType.Warning);
			}
			
			EditorGUILayout.PropertyField(progress, new GUIContent("Erase Progress"));
			EditorGUI.BeginChangeCheck();
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(cardManager, "Change Render Type");
				if (cardManager.TrySelectCard())
				{
					cardManager.InitSurfaceMaterial();
				}
			}
			
			#region Card Type

			var canSetNativeSize = true;
			var cardManagerChanged = false;
			
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(imageCard, new GUIContent("Image Card"));
			var changed = EditorGUI.EndChangeCheck();
			cardManagerChanged |= changed;
			if (changed && imageCard.objectReferenceValue != null)
			{
				serializedObject.ApplyModifiedProperties();
				cardManager.InitSurfaceMaterial();
			}

			if (imageCard.objectReferenceValue == null)
			{
				EditorGUILayout.HelpBox("Image Card is null, please set reference to Image Card", MessageType.Warning);
				canSetNativeSize = false;
			}

			EditorGUI.EndDisabledGroup();

			#endregion

			#region Camera and Sprite

			EditorGUILayout.PropertyField(camera, new GUIContent("Main Camera"));
			
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(scratchSprite, new GUIContent("Sprite"));
			var spriteChanged = EditorGUI.EndChangeCheck();
			if (spriteChanged)
			{
				cardManagerChanged = true;
				cardManager.ScratchSurfaceSprite = (Sprite)scratchSprite.objectReferenceValue;
			}
			
			if (scratchSprite.objectReferenceValue == null)
			{
				EditorGUILayout.HelpBox("Sprite is null, please set reference to Sprite", MessageType.Warning);
			}
			else if (scratchSprite.objectReferenceValue is Sprite sprite)
			{
				var spritePath = AssetDatabase.GetAssetPath(sprite);
				var textureImporter = AssetImporter.GetAtPath(spritePath) as TextureImporter;
				if (textureImporter != null && (sprite.packed || !sprite.texture.isReadable))
				{
					var textureImporterSettings = new TextureImporterSettings();
					textureImporter.ReadTextureSettings(textureImporterSettings);
					if (!textureImporterSettings.readable && textureImporterSettings.spriteMode == (int)SpriteImportMode.Multiple)
					{
						EditorGUILayout.HelpBox("Enable the “Read/Write” option in the texture settings to make the texture readable.", MessageType.Warning);
						if (GUILayout.Button(new GUIContent("Enable the “Read/Write” flag"), GUILayout.ExpandWidth(true)))
						{
							textureImporterSettings.readable = true;
							textureImporter.SetTextureSettings(textureImporterSettings);
							AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);
							AssetDatabase.Refresh();
						}
					}
				}
			}
			
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(progressAccuracy, new GUIContent("Progress Accuracy"));
			if (EditorGUI.EndChangeCheck())
			{
				cardManagerChanged = true;
				cardManager.ProgressAccuracy = (ProgressAccuracy)progressAccuracy.enumValueIndex;
			}

			#endregion

			#region Shaders

			EditorGUI.BeginDisabledGroup(Application.isPlaying);

			if (maskShader.objectReferenceValue == null)
			{
				EditorGUILayout.PropertyField(maskShader, new GUIContent("Mask Shader"));
			}

			if (brushShader.objectReferenceValue == null)
			{
				EditorGUILayout.PropertyField(brushShader, new GUIContent("Brush Shader"));
			}

			if (maskProgressShader.objectReferenceValue == null)
			{
				EditorGUILayout.PropertyField(maskProgressShader, new GUIContent("Mask Progress Shader"));
			}

			#endregion
			
			EditorGUI.EndDisabledGroup();
			
			#endregion

			DrawHorizontalLine();

			#region Input
			
			EditorGUI.BeginChangeCheck();
			inputBlockShow = EditorGUILayout.Foldout(inputBlockShow, "Input Parameters");
			if (EditorGUI.EndChangeCheck())
			{
				EditorPrefs.SetBool(InputBlockPrefsKey, inputBlockShow);
			}
			var inputEnableChanged = false;
			var usePressureChanged = false;
			var checkCanvasRaycastsChanged = false;
			var canvasesForRaycastsBlockingChanged = false;
			if (inputBlockShow)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(inputEnabled, new GUIContent("Input Enabled"));
				inputEnableChanged = EditorGUI.EndChangeCheck();
				
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(usePressure, new GUIContent("Use Pressure"));
				usePressureChanged = EditorGUI.EndChangeCheck();

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(checkCanvasRaycasts, new GUIContent("Check Canvas Raycasts"));
				checkCanvasRaycastsChanged = EditorGUI.EndChangeCheck();
				
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(canvasesForRaycastsBlocking, new GUIContent("Canvases For Raycasts Blocking"));
				canvasesForRaycastsBlockingChanged = EditorGUI.EndChangeCheck();
			}
			
			#endregion
			
			DrawHorizontalLine();
			
			#region ScartchCard Parameters

			var hasScratchCardReference = card.objectReferenceValue == null;
			EditorGUI.BeginDisabledGroup(hasScratchCardReference);
			EditorGUI.BeginChangeCheck();
			cardParametersBlockShow = EditorGUILayout.Foldout(cardParametersBlockShow, "Scratch Card Parameters");
			if (EditorGUI.EndChangeCheck())
			{
				EditorPrefs.SetBool(CardParametersBlockPrefsKey, cardParametersBlockShow);
			}
			var brushTextureChanged = false;
			var brushOpacityChanged = false;
			var brushSizeChanged = false;
			var scratchModeChanged = false;
			if (cardParametersBlockShow)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(brushTexture, new GUIContent("Brush Texture"));
				brushTextureChanged = EditorGUI.EndChangeCheck();
			
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.Slider(brushOpacity, 0f, 1f, new GUIContent("Brush Opacity"));
				brushOpacityChanged = EditorGUI.EndChangeCheck();
			
				if (brushTexture.objectReferenceValue != null)
				{
					if (cardManager.Card != null)
					{
						brushSize.floatValue = cardManager.BrushSize;
					}
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.Slider(brushSize, 0.01f, 4f, new GUIContent("Brush Size"));
					brushSizeChanged = EditorGUI.EndChangeCheck();
				}
			
				if (cardManager.Card != null)
				{
					mode.enumValueIndex = (int) cardManager.Card.Mode;
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(mode, new GUIContent("Scratch Mode"));
				scratchModeChanged = EditorGUI.EndChangeCheck();
			
				EditorGUI.EndDisabledGroup();
			}

			#endregion

			DrawHorizontalLine();

			#region Buttons

			EditorGUI.BeginDisabledGroup(!canSetNativeSize);
			if (GUILayout.Button("Set Native Size"))
			{
				var imageComponent = imageCard.objectReferenceValue as Image;
				if (imageComponent != null)
				{
					Undo.RecordObject(imageComponent.rectTransform, "Set Native Size");
					cardManager.SetNativeSize();
				}
			}
			EditorGUI.EndDisabledGroup();
			
			#endregion

			#region CheckForAlpha
			
			if (scratchSprite.objectReferenceValue != null && spriteChanged)
			{
				var path = AssetDatabase.GetAssetPath(scratchSprite.objectReferenceValue);
				var importer = AssetImporter.GetAtPath(path) as TextureImporter;
				if (importer != null)
				{
					hasAlpha.boolValue = importer.DoesSourceTextureHaveAlpha();
					cardManager.ScratchSurfaceSpriteHasAlpha = hasAlpha.boolValue;
				}
			}
			
			#endregion

			#region Apply

			if (card.objectReferenceValue != null)
			{
				if (card.objectReferenceValue != null)
				{
					var scratchCardChanged = false;
					if (brushTextureChanged && cardManager.Card.BrushMaterial != null)
					{
						cardManager.Card.BrushMaterial.mainTexture = brushTexture.objectReferenceValue as Texture2D;
						scratchCardChanged = true;
					}

					if (brushOpacityChanged)
					{
						Undo.RecordObject(cardManager, "Set Brush Opacity");
						cardManager.BrushOpacity = brushOpacity.floatValue;
						scratchCardChanged = true;
					}
					
					if (brushSizeChanged)
					{
						Undo.RecordObject(cardManager.Card, "Set Brush Size");
						cardManager.BrushSize = brushSize.floatValue;
						scratchCardChanged = true;
					}

					if (inputEnableChanged)
					{
						scratchCardChanged = true;
						cardManager.InputEnabled = inputEnabled.boolValue;
					}

					if (usePressureChanged)
					{
						cardManager.UsePressure = usePressure.boolValue;
					}

					if (checkCanvasRaycastsChanged)
					{
						cardManager.CheckCanvasRaycasts = checkCanvasRaycasts.boolValue;
					}

					if (canvasesForRaycastsBlockingChanged)
					{
						serializedObject.ApplyModifiedProperties();
						var data = cardManager.CanvasesForRaycastsBlocking;
						cardManager.CanvasesForRaycastsBlocking = data;
					}

					if (scratchModeChanged)
					{
						cardManager.Mode = (ScratchMode)mode.enumValueIndex;
						scratchCardChanged = true;
					}

					if (cardManager.Card.RenderTexture != null)
					{
						DrawHorizontalLine();
						var rect = GUILayoutUtility.GetRect(160, 120, GUILayout.ExpandWidth(true));
						GUI.DrawTexture(rect, cardManager.Card.RenderTexture, ScaleMode.ScaleToFit);
						DrawHorizontalLine();

						if (Application.isPlaying)
						{
							if (eraseProgress == null)
							{
								eraseProgress = progress.objectReferenceValue as EraseProgress;
							}

							if (eraseProgress != null)
							{
								EditorGUILayout.LabelField($"Erase progress: {eraseProgress.GetProgress()}");
							}

							if (GUILayout.Button("Clear"))
							{
								cardManager.ClearScratchCard();
							}

							if (GUILayout.Button("Fill"))
							{
								cardManager.FillScratchCard();
							}
						}
					}

					if (cardManagerChanged)
					{
						MarkAsDirty(target);
					}

					if (scratchCardChanged)
					{
						MarkAsDirty(cardManager.Card);
					}
				}
			}
			serializedObject.ApplyModifiedProperties();
			
			#endregion
		}

		private void MarkAsDirty(Object objectTarget, bool markScene = true)
		{
			if (Application.isPlaying)
				return;

			EditorUtility.SetDirty(objectTarget);
			if (markScene)
			{
				var component = objectTarget as Component;
				if (component != null)
				{
					EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
				}
			}
		}

		private void DrawHorizontalLine()
		{
			GUILayout.Space(5f);
			EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2f), Color.gray);
			GUILayout.Space(5f);
		}
	}
}