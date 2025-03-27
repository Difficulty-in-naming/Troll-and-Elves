using System.IO;
using System.Linq;
using Panthea.Utils;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;

namespace Panthea.Asset
{
    public class AppendBundleHash : IBuildTask
    {
        public int Version { get { return 1; } }

#pragma warning disable 649
        [InjectContext(ContextUsage.In)]
        IBundleBuildParameters m_Parameters;

        [InjectContext]
        IBundleBuildResults m_Results;
#pragma warning restore 649

        public ReturnCode Run()
        {
            if (!m_Parameters.AppendHash)
                return ReturnCode.SuccessNotRun;

            string[] bundles = m_Results.BundleInfos.Keys.ToArray();
            foreach (string bundle in bundles)
            {
                var details = m_Results.BundleInfos[bundle];
                var oldFileName = details.FileName;
                var newFileName = $"{PathUtils.RemoveFileExtension(details.FileName)}_{details.Hash.ToString()}{AssetsConfig.Suffix}";
                details.FileName = newFileName;
                m_Results.BundleInfos[bundle] = details;
                
                File.Delete(newFileName);
                File.Move(oldFileName, newFileName);
            }

            return ReturnCode.Success;
        }
    }
}