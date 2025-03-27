using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace EdgeStudio.UI.Component
{
    public class BgRollingComponent : MonoBehaviour
    {
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        public Image Image;
        private Material Material;
        void Start()
        {
            Material = Image.material = new Material(Image.material);
            LMotion.Create(Vector2.zero, new Vector2(1, 1), 30).WithLoops(-1).Bind(Material, (vector2, material) =>
            {
                material.SetTextureOffset(MainTex, vector2);
            });
        }
    }
}
