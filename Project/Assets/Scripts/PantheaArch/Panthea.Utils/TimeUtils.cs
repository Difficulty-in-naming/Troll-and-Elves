using System;
using System.Globalization;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Panthea.Common;
using R3;
using UnityEngine;
using UnityEngine.Networking;

namespace Panthea.Utils
{
    public static class TimeUtils
    {
        private static DateTimeOffset UtcDateTime { get; set; }
        private static long UtcTimeStamp { get; set; }
        private static DateTime Epoch { get; } = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static bool Fetching { get; private set; }
        private static Subject<DateTimeOffset> FetchTimeSubject = new();
        public static Observable<DateTimeOffset> FetchTimeObservable => FetchTimeSubject.AsObservable();
        public static async UniTask Init()
        {
            if (UtcTimeStamp == 0)
            {
                UtcDateTime = new DateTimeOffset(DateTime.UtcNow);
                UtcTimeStamp = UtcDateTime.ToUnixTimeSeconds();
            }

            await FetchUtcTime();
        }

        public static float DeltaTime() => Time.deltaTime;

        public static async UniTask UseDangerTime()
        {
            UtcDateTime = DateTimeOffset.UtcNow;
            UtcTimeStamp = UtcDateTime.ToUnixTimeSeconds();
            await UniTask.CompletedTask;
        }

        ///系统会不停的校对时间.直到成功完成校对为止
        public static async UniTask FetchUtcTime()
        {
            if (Fetching)
            {
                while (Fetching)
                {
                    await UniTask.NextFrame();
                }

                return;
            }

            Fetching = true;
            while (Fetching)
            {
                try
                {
                    using var request = UnityWebRequest.Head("https://www.baidu.com");
                    using var response = await request.SendWebRequest();
                    string todaysDates = response.GetResponseHeader("date");
                    UtcDateTime = DateTime.ParseExact(todaysDates,
                        "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                        CultureInfo.InvariantCulture.DateTimeFormat,
                        DateTimeStyles.AssumeUniversal).ToUniversalTime();
                    UtcTimeStamp = UtcDateTime.ToUnixTimeSeconds();
                    Fetching = false;
                    FetchTimeSubject.OnNext(UtcDateTime);
                }
                catch(Exception ex)
                {
                    Log.Error(ex);
                    await Task.Delay(5000);//每5秒执行一次
                }
            }
        }

        /// <summary> 返回当前UTC时间(全球) </summary>
        public static long GetUtcTimeStamp() => UtcTimeStamp;

        /// <summary> 将DateTime转换为UTC时间(全球) </summary>
        public static long GetUtcTimeStamp(DateTime dt) => ConvertDateTimeToStamp(dt);

        /// <summary> 返回当前UTC时间(全球) </summary>
        public static DateTime GetUtcDateTime() => DateTimeOffset.FromUnixTimeSeconds(GetUtcTimeStamp()).UtcDateTime;

        /// <summary> 返回当前UTC时间(全球) </summary>
        public static DateTime GetUtcDateTime(DateTime dt) => DateTimeOffset.FromUnixTimeSeconds(GetUtcTimeStamp(dt)).UtcDateTime;

        /// <summary> 返回当前UTC时间(本地) </summary>
        public static long GetLocalTimeStamp() => ConvertDateTimeToStamp(DateTimeOffset.FromUnixTimeSeconds(GetUtcTimeStamp()).LocalDateTime);

        /// <summary> 返回当前UTC时间(本地) </summary>
        public static DateTime GetLocalDateTime() => DateTimeOffset.FromUnixTimeSeconds(GetUtcTimeStamp()).LocalDateTime;

        /// <summary>将DateTime转换为UTC时间</summary>
        public static long ConvertDateTimeToStamp(DateTimeOffset time) => (long) (time - Epoch).TotalSeconds;
        
        /// <summary>将DateTime转换为UTC时间</summary>
        public static long ConvertDateTimeToStamp(DateTime time) => (long) (time - Epoch).TotalSeconds;

        /// <summary>获取两个UTC时间之间的差值</summary>
        public static TimeSpan BetweenTime(long fromUtcTime, long toUtcTime) => DateTimeOffset.FromUnixTimeSeconds(toUtcTime) - DateTimeOffset.FromUnixTimeSeconds(fromUtcTime);

        /// <summary>
        /// 将时间戳转换为DateTime对象
        /// </summary>
        /// <param name="timestamp">时间戳（秒或毫秒）</param>
        /// <param name="isMilliseconds">是否为毫秒级时间戳，默认为false（秒级）</param>
        /// <returns>转换后的DateTime对象</returns>
        public static DateTime ConvertToDateTime(long timestamp, bool isMilliseconds = false)
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            if (isMilliseconds)
            {
                timestamp /= 1000;
            }
            return unixEpoch.AddSeconds(timestamp);
        }
        
        /// <summary>
        /// 给定一个数字和一个转换格式如mm:ss
        /// 比如 ConvertNumberToTimeString(90,@"mm\:ss");
        /// 则返回结果 01:30
        /// </summary>
        public static string ConvertNumberToTimeString(double seconds, string format)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            string text = time.ToString(format);
            return text;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Purge() => FetchTimeSubject = new Subject<DateTimeOffset>();
    }
}