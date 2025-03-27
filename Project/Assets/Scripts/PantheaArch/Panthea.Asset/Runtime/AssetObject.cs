using UnityEngine;

namespace Panthea.Asset
{
    public class AssetObject
    {
        private GameObject GameObject;
        private ResPak ResPack;

        public AssetObject(GameObject go, ResPak resPack)
        {
            GameObject = go;
            ResPack = resPack;
        }

        public GameObject GetGameObject() => GameObject;
#if HAS_PANTHEA_ASSET
        public UnityObject Instantiate(Transform parent)
        {
            var go = Object.Instantiate(GameObject,parent);
            var uo = new UnityObject(go,ResPack, o =>
            {
                AssetsKit.Inst.ReleaseInstance(this);
            });
            return uo;
        }
#endif
    }
}