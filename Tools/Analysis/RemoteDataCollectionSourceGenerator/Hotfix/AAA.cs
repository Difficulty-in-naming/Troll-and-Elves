namespace EdgeStudio.DB
{
    [DBIndex((int)DBIndexMap.Player)]
    public class AAA : DBDefine
    {
        void XXX()
        {
            var x = RemoteDataCollection.AllType;
        }
    }

    [DBCacheOnly]
    public class BBB : DBDefine
    {
        void XXX()
        {
            var x = RemoteDataCollection.AllType;
        }
    }

}
