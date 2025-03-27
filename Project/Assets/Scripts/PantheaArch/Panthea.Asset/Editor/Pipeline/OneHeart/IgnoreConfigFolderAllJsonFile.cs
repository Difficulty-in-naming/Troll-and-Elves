using System.Collections.Generic;
using System.Threading.Tasks;
using Panthea.Asset.Define;

namespace Panthea.Asset.OneHeart
{
    public class IgnoreConfigFolderAllJsonFile : AResPipeline
    {
        public HashSet<string> BuildFiles;

        public IgnoreConfigFolderAllJsonFile(HashSet<string> buildFiles)
        {
            BuildFiles = buildFiles;
        }
        public override Task Do()
        {
            HashSet<string> files = new HashSet<string>(BuildFiles);
            foreach (var node in files)
            {
                if (node.StartsWith("config/"))
                {
                    if (node.EndsWith(".json"))
                    {
                        BuildFiles.Remove(node);
                    }
                }

                if (node.EndsWith(".skel"))
                {
                    BuildFiles.Remove(node);
                }
            }

            return Task.CompletedTask;
        }
    }
}