namespace EdgeStudio.Server
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class ProtoRequest : System.Attribute
    {
        public string Prefix;
        public System.Type ResponseType;

        public ProtoRequest(string prefix, System.Type responseType)
        {
            Prefix = prefix;
            ResponseType = responseType;
        }
    }
}