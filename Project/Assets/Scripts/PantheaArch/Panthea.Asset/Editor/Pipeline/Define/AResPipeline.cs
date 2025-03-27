using System.Threading.Tasks;

namespace Panthea.Asset.Define
{
    public abstract class AResPipeline
    {
        public abstract Task Do();

        public virtual Task Rollback()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnComplete()
        {
            return Task.CompletedTask;
        }
    }
}