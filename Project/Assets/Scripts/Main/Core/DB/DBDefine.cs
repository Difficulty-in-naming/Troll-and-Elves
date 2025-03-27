#if USE_MEMORYPACK
[MemoryPack.MemoryPackable]
#endif
namespace EdgeStudio.DB
{
    public partial class DBDefine
    {
        public virtual void NewData() { }
    
        public virtual void Init() { }

        public virtual void PreUpdate() { }

        public virtual void PostUpdate() { }
    
        public virtual void Dispose() { }
    }
}