using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Panthea.Common;
using Panthea.Utils;
using R3;
using UnityEngine;

namespace EdgeStudio.Manager
{
    public class RealTimeManager : Singleton<RealTimeManager>
    {
        public Observable<Unit> DayChanged => DayChangedSubject;
        private readonly Subject<Unit> DayChangedSubject = new();
        private DateTimeOffset nowTime;
        private DateTimeOffset nowDay;
        public RealTimeManager()
        {
            if (!Application.isPlaying)
                return;
            nowTime = TimeUtils.GetUtcDateTime();
            nowDay = nowTime.Date;
            Observable.Interval(TimeSpan.FromSeconds(1)).Where(_ => Inst.nowDay != Inst.nowTime.Date).Subscribe(_ => FireDayChanged());
            ObservableMono.OnApplicationFocusAsObservable().SubscribeAwait(OnApplicationFocus,AwaitOperation.Drop);
            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(CountTime);
        }

        public void Reset()
        {
            nowTime = TimeUtils.GetUtcDateTime();
            nowDay = nowTime.Date;
        }
        
        private void CountTime(Unit unit)
        {
            nowTime = nowTime.AddSeconds(1);
        }
        
        private async ValueTask OnApplicationFocus(bool b, CancellationToken cancellationToken)
        {
            if (b)
            {
                await TimeUtils.FetchUtcTime();
                nowTime = TimeUtils.GetUtcDateTime();
            }
        }

        private void FireDayChanged()
        {
            nowDay = nowTime.Date;
            DayChangedSubject.OnNext(Unit.Default);
        }

        /// <summary>
        /// 获取当前日期
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime GetUtcDate()
        {
            return nowTime.UtcDateTime;
        }

        /// <summary>
        /// 获取当前时间
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime GetLocalDate()
        {
            return nowTime.LocalDateTime;
        }

        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCurrentTimeStamp()
        {
            return (uint)TimeUtils.ConvertDateTimeToStamp(GetUtcDate());
        }

        /// <summary>
        /// 获取当前UTC时间戳
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCurrentUtcTimeStamp()
        {
            return (uint)TimeUtils.ConvertDateTimeToStamp(GetLocalDate());
        }
    }
}