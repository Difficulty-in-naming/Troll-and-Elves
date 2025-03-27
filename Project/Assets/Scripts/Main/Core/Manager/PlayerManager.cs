using System;
using EdgeStudio.DB;
using Panthea.Common;
using R3;

namespace EdgeStudio.Manager
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        private readonly Data_Player data;
      
        public ReactiveProperty<string> Name { get; private set; }

        public string Avatar
        {
            get => data.Avatar;
            set => data.Avatar = value;
        }

        public PlayerManager()
        {
            data = DBManager.Inst.Query<Data_Player>();
            DBManager.Inst.PreUpdate.Subscribe(OnSaveChanged);
            Name = string.IsNullOrEmpty(data.Name) ? new ReactiveProperty<string>("士兵") : new ReactiveProperty<string>(data.Name);
            Name.Subscribe(t1 => data.Name = t1);
        }

        void OnSaveChanged(Type t) => data.UpdateTime = RealTimeManager.Inst.GetCurrentUtcTimeStamp();
    }
}