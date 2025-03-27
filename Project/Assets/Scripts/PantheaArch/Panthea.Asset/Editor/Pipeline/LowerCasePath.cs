using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Panthea.Asset.Define;

namespace Panthea.Asset
{
    public class LowerCasePath: AResPipeline
    {
        public HashSet<string> BuildFiles;
        private Dictionary<string, object> mInject;

        public LowerCasePath(HashSet<string> buildFiles, Dictionary<string, object> inject)
        {
            BuildFiles = buildFiles;
            mInject = inject;
        }

        public override Task Do()
        {
            mInject["files"] = new HashSet<string>(BuildFiles.Select(x => x.ToLower()));
            return Task.CompletedTask;
        }
    }
}