namespace EdgeStudio.DB
{
    public class DBDefine
    {
        public virtual void NewData() { }
        public virtual void Init() { }

        public virtual void PreUpdate() { }

        public virtual void PostUpdate() { }
    
        public virtual void Dispose() { }
    }
}
