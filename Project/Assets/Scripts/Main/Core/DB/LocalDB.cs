using System;
using System.Collections.Generic;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using R3;

namespace EdgeStudio.DB
{
    public class LocalDB : ILocalDB
    {
        public static readonly string SaveKey = "Saves";
        public static readonly string SavePath = GameSettings.PersistentDataPath + "/DB/Local/";
        public const string Extname = ".data";
        public bool NeedUpdate = false;
        private RemoteDataCollection mRemoteDataCollection;
        private IDisposable updateObserver;
        public List<DBDefine> UpdateTarget = new List<DBDefine>();
        public void Init()
        {
#if !UNITY_EDITOR
        mRemoteDataCollection = SaveLoadUtils.Load<RemoteDataCollection>(SaveKey) ?? new RemoteDataCollection();
        mRemoteDataCollection.Init();
        mRemoteDataCollection.IfNullNewData();
#else
            mRemoteDataCollection = SaveLoadUtils.Load<RemoteDataCollection>(GetPath(SaveKey)) ?? new RemoteDataCollection();
            mRemoteDataCollection.Init();
            mRemoteDataCollection.IfNullNewData();
#endif
            updateObserver = Observable.EveryUpdate().Subscribe(Update);
        }
    
        public bool IsVaild() => true;

        /// <summary>
        /// 仅调用DBManager.UpdateLocal的时候触发.告诉系统这个文件需要同步到服务器
        /// </summary>
        public HashSet<Type> NeedSyncToServer { get; } = new();

        public T Query<T>() where T : DBDefine => (T) Query(typeof (T));

        private string GetPath(string name)
        {
            using var sb = ZString.CreateStringBuilder();
            sb.Append(SavePath);
            sb.Append(GameSettings.PlayerId);
            sb.Append("/");
            sb.Append(name);
            sb.Append(Extname);
            return sb.ToString();
        }
    
        public DBDefine Query(Type t)
        {
            var instance = mRemoteDataCollection.Get(t);
            if (mRemoteDataCollection.Get(t) != null)
                return instance;
            throw new Exception("加载存档逻辑错误");
        }

        public UniTask Update<T>(T value, bool force = false) where T : DBDefine
        {
            if (GameSettings.IsLoadingComplete == false)
            {
                throw new Exception("加载还未结束不能上传存档");
            }
            if (!typeof(T).IsDefined(typeof(DBCacheOnlyAttribute), false))
            {
                NeedUpdate = true;
                UpdateTarget.Add(value);
                if (force)
                    Update(Unit.Default);
            }
            return UniTask.CompletedTask;
        }

        public UniTask UpdateAll()
        {
            SaveLoadUtils.Save(GetPath(SaveKey),mRemoteDataCollection);
            return UniTask.CompletedTask;
        }
    
        void Update(Unit unit)
        {
            if (NeedUpdate)
            {
                UpdateAll();
            }

            NeedUpdate = false;
        }

        public RemoteDataCollection GetDataCollection() => mRemoteDataCollection;
    
        public void Dispose() => updateObserver.Dispose();
    }
}