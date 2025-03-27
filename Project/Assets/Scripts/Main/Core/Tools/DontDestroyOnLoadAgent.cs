using Panthea.Common;

namespace EdgeStudio.Tools
{
    public class DontDestroyOnLoadAgent : BetterMonoBehaviour
    {
        void Start()
        {
            DontDestroyOnLoad(CachedGameObject);
        }
    }
}