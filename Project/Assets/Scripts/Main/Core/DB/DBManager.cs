using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Panthea.Utils;
using R3;
using UnityEngine;
using UnityEngine.Scripting;

namespace EdgeStudio.DB
{
    public partial class DBManager: IDBService
    {
        private static DBManager _inst;
        public static DBManager Inst
        {
            get
            {
                if (_inst == null)
                {
                    _inst = new DBManager();
                    _inst.Init();
                }

                return _inst;
            }
        }

        private Subject<Type> preUpdateSubject = new Subject<Type>();
        private Subject<Type> postUpdateSubject = new Subject<Type>();
        public Observable<Type> PreUpdate => preUpdateSubject;
        public Observable<Type> PostUpdate => postUpdateSubject;

        private List<IDBService> mDBServices = new();

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Purge() => _inst = null;
#endif
        /// <summary>
        /// 我们不能使用PlayerManager.Inst.Id,因为PlayerManager有部分内容依赖了DB.使用PlayerManager.Id可能会引发无限循环的Bug
        /// </summary>
        public long UserId { get; set; } = Preference.Inst.UserId;
        public bool IsVaild() => true;

        [Preserve]
        public void Init()
        {
            //TODO 这里加入对服务器的判断.然后添加加载DB设置
            mDBServices.Add(new LocalDB());
            // if (GameSettings.EnableNetwork)
            // {
            // mDBServices.Add(new ServerDB());
            // }

            foreach (var node in mDBServices)
            {
                node.Init();
            }
        }

        public T Query<T>() where T : DBDefine => (T) Query(typeof (T));

        public DBDefine Query(Type type)
        {
            for (int i = mDBServices.Count - 1; i >= 0; i--)
            {
                var result = mDBServices[i].Query(type);
                if (result != null)
                    return result;
            }
            return null;
        }

        public void UpdateLocal<T>(T value,bool syncToServer = true) where T : DBDefine
        {
            this.preUpdateSubject.OnNext(typeof(T));
            var db = GetLocalService();
            db.Update(value);
            if(syncToServer)
                db.NeedSyncToServer.Add(value.GetType());
            this.postUpdateSubject.OnNext(typeof(T));
        }
    
        public void UpdateLocal<T>(bool syncToServer = true) where T : DBDefine
        {
            this.preUpdateSubject.OnNext(typeof(T));
            var db = GetLocalService();
            var value = db.Query<T>();
            db.Update(value);
            if(syncToServer)
                db.NeedSyncToServer.Add(value.GetType());
            this.postUpdateSubject.OnNext(typeof(T));
        }

        public async UniTask Update<T>(T value, bool force = false) where T : DBDefine
        {
            this.preUpdateSubject.OnNext(typeof(T));
            var tasks = ListPool<UniTask>.Create();
            for (int i = 0; i < mDBServices.Count; i++)
            {
                var node = mDBServices[i];
                if (node.IsVaild())
                {
                    tasks.Add(node.Update(value,force));
                }
            }
            await tasks;
            tasks.Dispose();
            this.postUpdateSubject.OnNext(typeof(T));
        }

        public RemoteDataCollection GetDataCollection() => GetLocalService().GetDataCollection();

        public void Update<T>(bool force = false) where T : DBDefine
        {
            this.preUpdateSubject.OnNext(typeof(T));
            var value = Query<T>();
            for (int i = 0; i < mDBServices.Count; i++)
            {
                var node = mDBServices[i];
                if(node.IsVaild())
                    node.Update(value,force);
            }
            this.postUpdateSubject.OnNext(typeof(T));
        }
    
        public async UniTask Update()
        {
            this.preUpdateSubject.OnNext(null);
            var db = GetLocalService();
            var tasks = ListPool<UniTask>.Create();
            foreach (var node in db.NeedSyncToServer)
            {
                var result = db.Query(node);
                tasks.Add(Update(result));
            }
            await tasks;
            tasks.Dispose();
            this.postUpdateSubject.OnNext(null);
        }
    
        public async UniTask UpdateAll()
        {
            this.preUpdateSubject.OnNext(null);
            var db = GetLocalService();
            var tasks = ListPool<UniTask>.Create();
            foreach (var node in mDBServices)
            {
                tasks.Add(node.UpdateAll());
            }
            await tasks;
            tasks.Dispose();
            this.postUpdateSubject.OnNext(null);
        }
    

        public ILocalDB GetLocalService()
        {
            for (int i = 0; i < mDBServices.Count; i++)
            {
                var node = mDBServices[i];
                if (node is ILocalDB db)
                {
                    return db;
                }
            }
            return null;
        }
    }
}