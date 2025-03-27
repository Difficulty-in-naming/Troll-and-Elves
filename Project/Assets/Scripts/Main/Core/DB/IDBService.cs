using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace EdgeStudio.DB
{
    public interface IDBService
    {
        void Init();
        bool IsVaild();
        T Query<T>() where T : DBDefine;
        DBDefine Query(Type type);
        UniTask Update<T>(T value,bool force = false) where T : DBDefine;
        UniTask UpdateAll();
        RemoteDataCollection GetDataCollection();
    }

    public interface ILocalDB : IDBService
    {
        HashSet<Type> NeedSyncToServer { get; }
    }
}