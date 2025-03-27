using System.Collections;
using Cysharp.Threading.Tasks;
using EdgeStudio.DB;
using Panthea.Common;

namespace EdgeStudio.Manager
{
    public class GuideManager : Singleton<GuideManager>
    {
        private Data_Player Player => DBManager.Inst.Query<Data_Player>();
        // private BitArray GuideState => Player.Guide;
        
        public GuideId ExecutingGuide { get; private set; }

        // public bool HasFinished(GuideId id) => GuideState[(int)id];
        
        public async void Start(GuideId id)
        {
            UniTask task = default;
            ExecutingGuide = id;
            await task;
        }
    }
}