﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Panthea.Asset.Define;
using Panthea.Utils;
using UnityEngine;

namespace Panthea.Asset
{
    public class CollectAllAssets: AResPipeline
    {
        public string PackPath;
        private Dictionary<string, object> mInject;

        public CollectAllAssets(string packPath, Dictionary<string, object> inject)
        {
            PackPath = packPath;
            mInject = inject;
        }

        public override Task Do()
        {
            var files = Directory.GetFiles(Application.dataPath + "/" + PackPath, "*.*", SearchOption.AllDirectories).ToList();

            for (var index = files.Count - 1; index >= 0; index--)
            {
                var path = files[index];
                var ext = Path.GetExtension(path);
                var fileName = Path.GetFileName(path);
                if (ext == ".meta")
                {
                    files.RemoveAt(index);
                    continue;
                }

                if (fileName.StartsWith("."))
                {
                    files.RemoveAt(index);
                    continue;
                }

                files[index] = PathUtils.FullPathToAssetbundlePath(files[index],Application.dataPath + "/" + PackPath);
            }

#if DEBUG_ADDRESSABLE
        foreach (var node in files)
        {
            Debug.Log("this file need to build:" + node);
        }
#endif

            mInject.Add("files", new HashSet<string>(files));
            return Task.CompletedTask;
        }
    }
}