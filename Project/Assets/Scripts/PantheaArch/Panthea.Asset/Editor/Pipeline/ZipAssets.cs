using System.Threading.Tasks;
using Panthea.Asset.Define;
using UnityEngine;

namespace Panthea.Asset
{
    public class ZipAssets: AResPipeline
    {
        public override Task Do()
        {
            Debug.Log("等待实现");
            return Task.CompletedTask;
        }
    }
}