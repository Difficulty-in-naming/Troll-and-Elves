using LitMotion;
using LitMotion.Extensions;
using Panthea.Common;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EdgeStudio.GUI
{
    public class ParabolicCurve : BetterMonoBehaviour
    {
        public RectTransform startPoint;
        public RectTransform endPoint;
        public float height = 100f;
        [Range(0f, 1f)] public float progress = 0f;

        public int resolution = 50;

        private Vector2[] positions;

        void Awake() => UpdateCurve();

        public void SetToEnd()
        {
            if (positions == null)
            {
                UpdateCurve();
            }

            progress = 1;
            UpdateImagePosition();
        }
        
        public void SetToStart()
        {
            if (positions == null)
            {
                UpdateCurve();
            }

            progress = 0;
            UpdateImagePosition();
        }

        public void UpdateCurve()
        {
            if (startPoint == null || endPoint == null)
                return;
            positions = new Vector2[resolution + 1];
            Vector2 start = startPoint.anchoredPosition;
            Vector2 end = endPoint.anchoredPosition;

            for (int i = 0; i <= resolution; i++)
            {
                float t = i / (float)resolution;
                positions[i] = CalculateParabolicPoint(start, end, height, t);
            }

            RectTransform.anchoredPosition = (start + end) / 2;
            UpdateImagePosition();
        }
        
        private void UpdateImagePosition()
        {
            if (positions is { Length: > 1 })
            {
                int index = Mathf.FloorToInt(progress * (positions.Length - 1));
                RectTransform.anchoredPosition = positions[index];
            }
        }
        
        private Vector2 CalculateParabolicPoint(Vector2 start, Vector2 end, float height, float t)
        {
            float parabolicT = t * 2 - 1;
            Vector2 travelVector = end - start;
            Vector2 result = start + t * travelVector;
            result.y += (-parabolicT * parabolicT + 1) * height;
            return result;
        }

        public void Drop()
        {
            LMotion.Create(0f, 1f, 0.4f).Bind(t1 => progress = t1).AddTo(this);
            LMotion.Create(0f, 360f, 0.06f).WithLoops(5).BindToEulerAnglesZ(RectTransform).AddTo(this);
        }
    }
}