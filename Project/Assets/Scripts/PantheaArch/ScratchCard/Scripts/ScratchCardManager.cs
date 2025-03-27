using ScratchCardAsset.Core;
using ScratchCardAsset.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ScratchCardAsset
{
	[ExecuteInEditMode]
	public class ScratchCardManager : MonoBehaviour
	{
		#region General

		public ScratchCard Card;
		public EraseProgress Progress;

		[SerializeField] private Image canvasRendererCard;
		public Image CanvasRendererCard
		{
			get => canvasRendererCard;
			set => canvasRendererCard = value;
		}

		[FormerlySerializedAs("ScratchSurfaceSpriteHasAlpha")] [SerializeField] private bool scratchSurfaceSpriteHasAlpha = true;
		public bool ScratchSurfaceSpriteHasAlpha
		{
			get => scratchSurfaceSpriteHasAlpha;
			set
			{
				scratchSurfaceSpriteHasAlpha = value;
				if (Progress != null)
				{
					Progress.SampleSourceTexture = scratchSurfaceSpriteHasAlpha;
				}
			}
		}

		#endregion

		#region ScratchCard Parameters

		[FormerlySerializedAs("Mode")] [SerializeField] private ScratchMode mode;
		public ScratchMode Mode
		{
			get => mode;
			set
			{
				mode = value;
				if (Card != null)
				{
					Card.Mode = mode;
				}
			}
		}

		[FormerlySerializedAs("MainCamera")] [SerializeField] private Camera mainCamera;
		public Camera MainCamera
		{
			get => mainCamera;
			set
			{
				mainCamera = value;
				if (Card != null && Card.ScratchData != null)
				{
					Card.ScratchData.Camera = mainCamera;
				}
			}
		}

		[FormerlySerializedAs("ScratchSurfaceSprite")] [SerializeField] private Sprite scratchSurfaceSprite;
		public Sprite ScratchSurfaceSprite
		{
			get => scratchSurfaceSprite;
			set
			{
				scratchSurfaceSprite = value;
				if (Card != null)
				{
					if (Application.isPlaying)
					{
						if (initialized)
						{
							UpdateCardSprite(scratchSurfaceSprite);
							Card.SetRenderType(mainCamera);
							Card.Init();
							if (Progress != null)
							{
								Progress.ResetProgress();
								Progress.UpdateProgress();
							}
						}
					}
					else
					{
						InitSurfaceMaterial();
					}
				}
			}
		}
		
		[SerializeField] private ProgressAccuracy progressAccuracy;
		public ProgressAccuracy ProgressAccuracy
		{
			get => progressAccuracy;
			set
			{
				progressAccuracy = value;
				if (Progress != null && Progress.ProgressMaterial != null)
				{
					Progress.ProgressAccuracy = progressAccuracy;
				}
			}
		}

		[FormerlySerializedAs("EraseTexture")] [SerializeField] private Texture brushTexture;
		public Texture BrushTexture
		{
			get => brushTexture;
			set
			{
				brushTexture = value;
				if (Card != null && Card.BrushMaterial != null)
				{
					Card.BrushMaterial.mainTexture = brushTexture;
				}
			}
		}
		
		[FormerlySerializedAs("EraseTextureScale")] [SerializeField] private float brushSize = 1f;
		public float BrushSize
		{
			get => brushSize;
			set
			{
				brushSize = value;
				if (Card != null && Card.Initialized)
				{
					Card.BrushSize = brushSize;
				}
			}
		}
		
		[SerializeField] private float brushOpacity = 1f;
		public float BrushOpacity
		{
			get => brushOpacity;
			set
			{
				brushOpacity = value;
				if (Card != null && Card.BrushMaterial != null)
				{
					Card.BrushMaterial.color = new Color(Card.BrushMaterial.color.r, Card.BrushMaterial.color.g, Card.BrushMaterial.color.b, brushOpacity);
				}
			}
		}

		#endregion

		#region Input
		
		[FormerlySerializedAs("InputEnabled")] [SerializeField] private bool inputEnabled = true;
		public bool InputEnabled
		{
			get => inputEnabled;
			set
			{
				inputEnabled = value;
				Card.enabled = inputEnabled;
			}
		}

		[SerializeField] private bool usePressure;
		public bool UsePressure
		{
			get => usePressure;
			set
			{
				usePressure = value;
				if (Card != null && Card.Initialized)
				{
					Card.Input.UsePressure = usePressure;
				}
			}
		}
		
		[SerializeField] private bool checkCanvasRaycasts = true;
		public bool CheckCanvasRaycasts
		{
			get => checkCanvasRaycasts;
			set
			{
				checkCanvasRaycasts = value;
				if (Card != null && Card.Initialized)
				{
					Card.Input.CheckCanvasRaycasts = checkCanvasRaycasts;
					if (checkCanvasRaycasts)
					{
						Card.Input.InitRaycastsController(Card.SurfaceTransform.gameObject, canvasesForRaycastsBlocking);
					}
				}
			}
		}

		[SerializeField] private Canvas[] canvasesForRaycastsBlocking;
		public Canvas[] CanvasesForRaycastsBlocking
		{
			get => canvasesForRaycastsBlocking;
			set
			{
				canvasesForRaycastsBlocking = value;
				if (Card != null && Card.Initialized)
				{
					Card.Input.InitRaycastsController(Card.SurfaceTransform.gameObject, canvasesForRaycastsBlocking);
				}
			}
		}
		
		#endregion

		#region Shaders
		
		[FormerlySerializedAs("MaskShader")] [SerializeField] private Shader maskShader;
		[FormerlySerializedAs("BrushShader")] [SerializeField] private Shader brushShader;
		[FormerlySerializedAs("MaskProgressShader")] [SerializeField] private Shader maskProgressShader;
		
		#endregion

		private Material surfaceMaterial;
		private Texture2D scratchTexture;
		private Color[] spritePixels;
		private Sprite scratchSprite;
		private MigrationHelper migrationHelper;
		private bool initialized;

		private void Awake()
		{
			if (!Application.isPlaying)
			{
				migrationHelper = new MigrationHelper();
				migrationHelper.StartMigrate(this);
				InitSurfaceMaterial();
			}
		}

		private void Start()
		{
			if (!Application.isPlaying)
			{
				if (migrationHelper == null)
				{
					migrationHelper = new MigrationHelper();
					migrationHelper.StartMigrate(this);
				}
				
				migrationHelper.FinishMigrate();
				migrationHelper = null;
				return;
			}

			if (initialized)
				return;
			
			Init();
		}

		private void OnDestroy()
		{
			if (surfaceMaterial != null)
			{
				if (Application.isPlaying)
				{
					Destroy(surfaceMaterial);
				}
				else
				{
					DestroyImmediate(surfaceMaterial);
				}
				surfaceMaterial = null;
			}
			
			if (!Application.isPlaying)
				return;

			if (Card != null)
			{
				Card.OnInitialized -= OnCardInitialized;
				Card.OnRenderTextureInitialized -= OnCardRenderTextureInitialized;
			}
			ReleaseTexture();
		}

		public void Init()
		{
			if (Card == null)
			{
				Debug.LogError("ScratchCard field is not assigned!");
				return;
			}
			
			if (mainCamera == null)
			{
				mainCamera = mainCamera != null ? mainCamera : Camera.main;
			}

			InitSurfaceMaterial();
			InitBrushMaterial();
			InitProgressMaterial();
			if (TrySelectCard())
			{
				Card.BrushSize = BrushSize;
				Card.Mode = mode;
				Card.SetRenderType(mainCamera);
				Card.OnInitialized -= OnCardInitialized;
				Card.OnInitialized += OnCardInitialized;
				Card.OnRenderTextureInitialized -= OnCardRenderTextureInitialized;
				Card.OnRenderTextureInitialized += OnCardRenderTextureInitialized;
				Card.Init();
			}
			else
			{
				Card.enabled = false;
			}
			
			if (Card.Mode == ScratchMode.Restore)
			{
				Card.Fill(false);
			}
			
			initialized = true;
		}
		
		private void OnCardInitialized(ScratchCard scratchCard)
		{
			scratchCard.Input.UsePressure = usePressure;
			scratchCard.Input.CheckCanvasRaycasts = checkCanvasRaycasts;
			if (checkCanvasRaycasts)
			{
				scratchCard.Input.InitRaycastsController(scratchCard.SurfaceTransform.gameObject, canvasesForRaycastsBlocking);
			}
		}

		private void OnCardRenderTextureInitialized(RenderTexture renderTexture)
		{
			if (Progress != null && Progress.ProgressMaterial != null)
			{
				Progress.ProgressMaterial.mainTexture = renderTexture;
			}
		}
		
		private void ReleaseTexture()
		{
			if (scratchTexture != null)
			{
				Destroy(scratchTexture);
				scratchTexture = null;
			}

			if (scratchSprite != null)
			{
				Destroy(scratchSprite);
				scratchSprite = null;
			}
		}

		public void InitSurfaceMaterial()
		{
			if (Card != null && Card.SurfaceMaterial == null)
			{
				var scratchSurfaceMaterial = new Material(maskShader);
				Card.SurfaceMaterial = scratchSurfaceMaterial;
				surfaceMaterial = scratchSurfaceMaterial;
			}

			UpdateCardSprite(scratchSurfaceSprite);
		}

		private void UpdateCardSprite(Sprite sprite)
		{
			ReleaseTexture();
			var scratchSurfaceMaterial = Card.SurfaceMaterial;
			var isPartOfAtlas = sprite != null && (sprite.texture.width != sprite.rect.size.x || sprite.texture.height != sprite.rect.size.y);
			if (Application.isPlaying)
			{
				if (isPartOfAtlas || scratchSurfaceSpriteHasAlpha)
				{
					if (sprite.texture.isReadable)
					{
						if (sprite.texture.IsCrunched())
						{
							var tempTexture = new Texture2D(sprite.texture.width, sprite.texture.height);
							tempTexture.SetPixels32(sprite.texture.GetPixels32());
							tempTexture.Apply();
							if (sprite.packed)
							{
								spritePixels = tempTexture.GetPixels((int)sprite.textureRect.x, (int)sprite.textureRect.y, (int)sprite.rect.width, (int)sprite.rect.height);
							}
							else
							{
								spritePixels = tempTexture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width, (int)sprite.rect.height);
							}

							Destroy(tempTexture);
						}
						else
						{
							if (sprite.packed)
							{
								spritePixels = sprite.texture.GetPixels((int)sprite.textureRect.x, (int)sprite.textureRect.y, (int)sprite.rect.width, (int)sprite.rect.height);
							}
							else
							{
								spritePixels = sprite.texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width, (int)sprite.rect.height);
							}
						}
					}
					else
					{
						// Debug.LogError("Texture is not readable, please set Read/Write flag in texture settings.");
					}
				}

				if (isPartOfAtlas)
				{
					scratchTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
					scratchTexture.SetPixels(spritePixels);
					scratchTexture.Apply();
					
					if (scratchSurfaceMaterial != null)
					{
						scratchSurfaceMaterial.mainTexture = scratchTexture;
					}

					var croppedRect = new Rect(0, 0, scratchTexture.width, scratchTexture.height);
					var pivot = scratchSurfaceSprite.pivot / croppedRect.size;
					scratchSprite = Sprite.Create(scratchTexture, croppedRect, pivot, Constants.General.PixelsPerUnit);
					sprite = scratchSprite;
				}
				else if (scratchSurfaceMaterial != null && scratchSurfaceSprite != null)
				{
					scratchSurfaceMaterial.mainTexture = scratchSurfaceSprite.texture;
				}
				
				UpdateProgressMaterial();
			}
			else if (Card.SurfaceMaterial != null && scratchSurfaceSprite != null)
			{
				Card.SurfaceMaterial.mainTexture = scratchSurfaceSprite.texture;
			}

			UpdateCardOffset();
			
			if (CanvasRendererCard != null)
			{
				if (Card.SurfaceMaterial != null)
				{
					CanvasRendererCard.material = Card.SurfaceMaterial;
				}
				if (sprite != null)
				{
					CanvasRendererCard.sprite = sprite;
				}
			}
		}

		private void UpdateCardOffset()
		{
			if (Card.SurfaceMaterial != null)
			{
				Card.SurfaceMaterial.SetVector(Constants.MaskShader.Offset, new Vector4(0, 0, 1, 1));
			}
		}

		private void InitBrushMaterial()
		{
			if (Card.BrushMaterial == null)
			{
				Card.BrushMaterial = new Material(brushShader);
			}
			
			Card.BrushMaterial.mainTexture = brushTexture;
			Card.BrushMaterial.color = new Color(1, 1, 1, brushOpacity);
		}

		private void InitProgressMaterial()
		{
			if (Progress == null)
				return;
			
			if (Progress.ProgressMaterial == null)
			{
				var progressMaterial = new Material(maskProgressShader);
				Progress.ProgressMaterial = progressMaterial;
				Progress.SampleSourceTexture = scratchSurfaceSpriteHasAlpha;
			}

			Progress.ProgressAccuracy = progressAccuracy;
			SetProgressSourceTexture();
		}

		private void SetProgressSourceTexture()
		{
			if (scratchSurfaceSpriteHasAlpha)
			{
				if (scratchTexture != null)
				{
					Progress.ProgressMaterial.SetTexture(Constants.ProgressShader.SourceTexture, scratchTexture);
				}
				else if (scratchSurfaceSprite != null)
				{
					Progress.ProgressMaterial.SetTexture(Constants.ProgressShader.SourceTexture, scratchSurfaceSprite.texture);
				}
			}
		}

		private void UpdateProgressMaterial()
		{
			if (Progress != null)
			{
				if (Progress.ProgressMaterial != null)
				{
					SetProgressSourceTexture();
				}

				if (Application.isPlaying && spritePixels != null)
				{
					Progress.SetSpritePixels(spritePixels);
					spritePixels = null;
				}
			}
		}
		
		#region Public Methods

		/// <summary>
		/// Fills RenderTexture with white color (100% scratched surface)
		/// </summary>
		public void FillScratchCard()
		{
			Card.Fill(false);
			if (Progress != null)
			{
				Progress.UpdateProgress();
			}
		}

		/// <summary>
		/// Fills ScratchCard RenderTexture with clear color (0% scratched surface)
		/// </summary>
		public void ClearScratchCard()
		{
			Card.Clear(false);
			if (Progress != null)
			{
				Progress.UpdateProgress();
			}
		}

		public bool TrySelectCard()
		{
			if (canvasRendererCard != null)
			{
				var card = canvasRendererCard;
				card.gameObject.SetActive(true);
				Card.SurfaceTransform = card.transform;
				return true;
			}
			return false;
		}

		public void SetNativeSize()
		{
			if (CanvasRendererCard != null)
			{
				CanvasRendererCard.SetNativeSize();
			}
		}

		#endregion
	}
}