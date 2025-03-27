using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using EdgeStudio.DB;
using LitMotion;
using LitMotion.Extensions;
using Panthea.Asset;
using Panthea.Common;
using Panthea.Utils;
using R3;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace EdgeStudio.Manager.Audio
{
    /// <summary>
    /// 多媒体管理器
    /// 负责视频的播放
    /// 或者声音的播放
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        public class Callback
        {
            public int Handle;
        }

        private class TempClip
        {
            public AudioSource Source;
            public string PlayMediaPath; //播放媒体得文件路径
            public OnEnd EndCallback;
            public AudioClip Clip;
            public TempClip(AudioSource source)
            {
                Source = source;
            }
        }

        public delegate void OnEnd(Callback data);

        public string ActiveMusicPath { get; private set; }
        private Preference Prefs => Preference.Inst;

        /// <summary>
        /// 缓存使用过的声效
        /// </summary>
        private Dictionary<string, AudioClip> SoundCache;

        /// <summary>
        /// 缓存使用过的音乐
        /// </summary>
        private Dictionary<string, AudioClip> MusicCache;

        private Dictionary<AudioSource, MotionHandle> MotionMap { get; } = new Dictionary<AudioSource, MotionHandle>();
        
        /// <summary>
        /// 因为这个声效可能正在播放中.如果这时候我们调整了音效的音量.我们无法从池里面拿到所有的音效
        /// </summary>
        private List<TempClip> TempSoundPool;

        private List<TempClip> TempMusicPool;

        private Dictionary<string, int> AliasMap;

        private float mSoundVolume;
        private AudioMixer Mixer;
        private AudioMixerGroup SoundGroup;
        private AudioMixerGroup MusicGroup;
        public float SoundVolume
        {
            get => mSoundVolume;
            set
            {
                ControlSound(value);
                mSoundVolume = value;
                Prefs.SoundVolume = value;
                //DeepSeek请修改这段代码.value现在的取值范围为0-1,而SoundVol的取值范围为-80到20
                SoundGroup.audioMixer.SetFloat("SoundVol", Mathf.Log10(Mathf.Clamp(value / 1, 0.0001f, 1)) * 20);
            }
        }

        private float mMusicVolume;

        public float MusicVolume
        {
            get => mMusicVolume;
            set
            {
                ControlMusic(value);
                mMusicVolume = value;
                Prefs.MusicVolume = value;
                MusicGroup.audioMixer.SetFloat("MusicVol", Mathf.Log10(Mathf.Clamp(value / 1, 0.0001f, 1)) * 20);
            }
        }

        private float mSoundPlayPitch;

        public float GetSoundPlayPitch() => mSoundPlayPitch;

        public void SetSoundPlayPitch(float value, bool force)
        {
            if (!Mathf.Approximately(mSoundPlayPitch, value))
            {
                mSoundPlayPitch = value;
                SoundPool.Inst.Apply(source => { source.pitch = value; });
                foreach (TempClip node in TempSoundPool)
                {
                    node.Source.pitch = value;
                }
            }
        }

        private float mMusicPlayPitch;

        public float MusicPlayPitch
        {
            get => mMusicPlayPitch;
            set
            {
                mMusicPlayPitch = value;
                foreach (var node in TempMusicPool)
                {
                    node.Source.pitch = value;
                }

                MusicPool.Inst.Apply(source => { source.pitch = value; });
            }
        }

        public string MuiscName { get; set; }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Purge() => Inst = null;
#endif

        public override void OnCreate() //
        {
            Mixer = Resources.Load<AudioMixer>("AudioMixer");
            SoundGroup = Mixer.FindMatchingGroups("Master/Sound")[0];
            MusicGroup = Mixer.FindMatchingGroups("Master/Music")[0];
#if UNITY_EDITOR
            if (!Mixer)
            {
                throw new Exception("无法在Resources目录下找到AudioMixer,请创建一个AudioMixer并在Group中添加Sound和Music并将音量变量公开");
            }
#endif
            MusicCache = new Dictionary<string, AudioClip>(8);
            TempSoundPool = new List<TempClip>(32);
            TempMusicPool = new List<TempClip>(4);
            AliasMap = new Dictionary<string, int>(16);
            mSoundPlayPitch = 1;
            mMusicPlayPitch = 1;
            SoundCache = new Dictionary<string, AudioClip>(32);
            mMusicVolume = Prefs.MusicVolume;
            mSoundVolume = Prefs.SoundVolume;
            //注册Update用于清理回收池
            Observable.EveryUpdate().Subscribe(Update);
        }

        private void Update(Unit unit)
        {
            for (var index = Inst.TempSoundPool.Count - 1; index >= 0; index--)
            {
                var node = Inst.TempSoundPool[index];
                if (node.Source && !node.Source.isPlaying && !node.Source.loop)
                {
                    Inst.SoundReturnToPool(node.Source);
                }
            }
        }

        /// <summary>
        /// 为返回的HashId 设置别名.通过别名进行查找
        /// 使用完后需要自己手动移除映射.
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="id"></param>
        public void SetAlias(string alias, int id)
        {
            if (id != 0 && !string.IsNullOrEmpty(alias))
            {
                AliasMap[alias] = id;
            }
        }

        public int GetIdByAlias(string alias)
        {
            return string.IsNullOrEmpty(alias) ? 0 : AliasMap.GetValueOrDefault(alias, 0);
        }

        public void RemoveMap(string alias)
        {
            if (AliasMap.ContainsKey(alias)) AliasMap.Remove(alias);
        }

        //==================================================================================================================
        // 用文件名播放音效
        public async UniTask<int> PlaySound([MustLower] string name, bool loop = false, float delay = 0, OnEnd end = null,int max = 0,
            CancellationToken cancellationToken = default)
        {
            if (Prefs.SoundVolume == 0 || cancellationToken.IsCancellationRequested) return 0;
            
            if (string.IsNullOrEmpty(name))
            {
                Log.Error("播放声音使用的路径不能为空");
                return 0;
            }
            
            if (max > 0)
            {
                int count = 0;
                foreach (var node in TempSoundPool)
                {
                    if (node.PlayMediaPath == name)
                    {
                        count++;
                        if (count >= max)
                        {
                            Log.Debug("播放声音" + name + "超过上限.无法播放");
                            return 0;
                        }
                    }
                }
            }

            var path = name;
            bool temp3 = SoundCache.TryGetValue(path, out var clip);
            AudioSource source = SoundPool.Inst.Rent();
            
            var temp = new TempClip(source) { EndCallback = end, Source = source, PlayMediaPath = name };
            TempSoundPool.Add(temp);
            
            if (!temp3 || !clip)
            {
                try
                {
                    clip = await AssetsKit.Inst.Load<AudioClip>("audios/sound/" + path);
                    if (!clip || cancellationToken.IsCancellationRequested)
                    {
                        SoundPool.Inst.Return(source);
                        TempSoundPool.Remove(temp);
                        return 0;
                    }
                }
                catch
                {
                    Log.Warning(path + "没找到");
                    SoundPool.Inst.Return(source);
                    TempSoundPool.Remove(temp);
                    return 0;
                }

                SoundCache[path] = clip;
            }

            source.clip = clip;

            if (delay > 0)
                DelayPlaySound(source, delay, cancellationToken);
            else
                source.Play();
            source.volume = SoundVolume;
            source.loop = loop;
            source.pitch = GetSoundPlayPitch();
            int code = source.GetHashCode();
            return code;
        }

        public async void DelayPlaySound(AudioSource source, float time, CancellationToken token = default)
        {
            await UniTask.WaitForSeconds(time, cancellationToken: token);
            if (Prefs.SoundVolume == 0)
                return;
            source.Play();
        }

        //用组件播放音效
        public int PlaySound(AudioClip clip, bool loop = false, float delay = 0, OnEnd end = null,int max = 0,CancellationToken cancellationToken = default)
        {
            if (!clip)
            {
                Log.Error("播放声音的引用不能为空");
                return 0;
            }
            
            if (max > 0)
            {
                int count = 0;
                foreach (var node in TempSoundPool)
                {
                    if (node.Clip == clip)
                    {
                        count++;
                        if (count >= max)
                        {
                            Log.Debug("播放声音" + clip.name + "超过上限.无法播放");
                            return 0;
                        }
                    }
                }
            }
            var source = SoundPool.Inst.Rent();
            var temp = new TempClip(source) { EndCallback = end, Source = source, PlayMediaPath = clip.name, Clip = clip };
            TempSoundPool.Add(temp);

            source.clip = clip;
            source.Play();
            source.loop = loop;
            source.pitch = GetSoundPlayPitch();
            int code = source.GetHashCode();

            return code;
        }

        //暂停指定音效
        public void PauseSound(int id)
        {
            for (int i = 0; i < TempSoundPool.Count; i++)
            {
                if (TempSoundPool[i].Source.GetHashCode() == id)
                {
                    TempSoundPool[i].Source.Pause();
                }
            }
        }

        //继续指定音效
        public void ResumeSound(int id)
        {
            if (Prefs.SoundVolume == 0)
                return;
            for (int i = 0; i < TempSoundPool.Count; i++)
            {
                if (TempSoundPool[i].Source.GetHashCode() == id)
                {
                    TempSoundPool[i].Source.UnPause();
                }
            }
        }

        //继续指定音效
        public void ResumeSound(string fileName)
        {
            if (Prefs.SoundVolume == 0)
                return;
            for (int i = 0; i < TempSoundPool.Count; i++)
            {
                if (string.Equals(TempSoundPool[i].PlayMediaPath, fileName))
                {
                    TempSoundPool[i].Source.UnPause();
                }
            }
        }

        //停止指定音效
        public void StopSound(int id)
        {
            for (int i = 0; i < TempSoundPool.Count; i++)
            {
                if (TempSoundPool[i].Source.GetHashCode() == id)
                {
                    var source = TempSoundPool[i];
                    SoundReturnToPool(source.Source);
                }
            }
        }

        public void StopSound(string name)
        {
            for (int i = 0; i < TempSoundPool.Count; i++)
            {
                if (TempSoundPool[i].PlayMediaPath == name)
                {
                    var source = TempSoundPool[i];
                    SoundReturnToPool(source.Source);
                }
            }
        }
        
        public void StopSound(AudioClip clip)
        {
            for (int i = 0; i < TempSoundPool.Count; i++)
            {
                if (TempSoundPool[i].Clip == clip)
                {
                    var source = TempSoundPool[i];
                    SoundReturnToPool(source.Source);
                }
            }
        }

        //暂停所有音效
        public void PauseAllSound()
        {
            for (int i = 0; i < TempSoundPool.Count; i++)
            {
                TempSoundPool[i].Source.Pause();
            }
        }

        //继续所有音效
        public void ResumeAllSound()
        {
            if (Prefs.SoundVolume == 0)
                return;
            for (int i = 0; i < TempSoundPool.Count; i++)
            {
                TempSoundPool[i].Source.UnPause();
            }
        }

        //停止所有音效
        public void StopAllSound()
        {
            for (int i = 0; i < TempSoundPool.Count; i++)
            {
                var source = TempSoundPool[i];
                SoundPool.Inst.Return(source.Source);
                source.Source.Stop();
                source.EndCallback?.Invoke(new Callback { Handle = source.Source.GetHashCode() });
            }

            TempSoundPool.Clear();
        }

        //==================================================================================================================
        //用文件名播放音乐文件
        public async UniTask<int> PlayMusic([MustLower] string name, bool loop = true, float duration = 0.3f, float delay = 0)
        {
            MuiscName = name;
            if (string.IsNullOrEmpty(name)) return 0;
            Debug.Log(name);
            AudioSource source = MusicPool.Inst.Rent();
            source.enabled = true;
            var temp = new TempClip(source) { Source = source, PlayMediaPath = name };
            TempMusicPool.Add(temp);
            if (!MusicCache.TryGetValue(name, out var clip) || !clip)
            {
                Log.Print("加载音乐资源:" + name);
                try
                {
                    clip = await AssetsKit.Inst.Load<AudioClip>("audios/music/" + name);
                    if (!clip)
                    {
                        MusicPool.Inst.Return(source);
                        return 0;
                    }
                }
                catch
                {
                    Log.Warning(name + "没找到");
                    MusicPool.Inst.Return(source);
                    return 0;
                }

                MusicCache[name] = clip;
            }

            if (!source.enabled)
                return 0;
            source.mute = false;
            source.pitch = 1.0f;
            ActiveMusicPath = name;
            source.clip = clip;
            source.loop = loop;
            source.volume = 0;
            source.pitch = MusicPlayPitch;
            if (MotionMap.TryGetValue(source, out var motion))
                motion.Complete();
            if (duration > 0)
            {
                MotionMap[source] = LMotion.Create(source.volume, MusicVolume, duration).WithUnscaleTime().BindToVolume(source);
            }
            else
                source.volume = MusicVolume;
            if (delay > 0)
                source.PlayDelayed(delay);
            else
                source.Play();
            return source.GetHashCode();
        }

        //暂停音乐
        public void PauseMusic(int id)
        {
            for (int i = 0; i < TempMusicPool.Count; i++)
            {
                var source = TempMusicPool[i].Source;
                if (source.GetHashCode() == id)
                {
                    if (MotionMap.TryGetValue(source, out var motion))
                        motion.Complete();
                    MotionMap[source] = LMotion.Create(source.volume, mMusicVolume, 0.3f).WithUnscaleTime().WithOnComplete(source.Pause).BindToVolume(source);
                }
            }
        }

        public void PauseMusicByFileName(string fileName)
        {
            if (Prefs.MusicVolume == 0)
                return;
            for (int i = 0; i < TempMusicPool.Count; i++)
            {
                var source = TempMusicPool[i].Source;
                if (source.clip.name == fileName)
                {
                    if (MotionMap.TryGetValue(source, out var motion))
                        motion.Complete();
                    MotionMap[source] = LMotion.Create(source.volume, mMusicVolume, 0.3f).WithUnscaleTime().WithOnComplete(source.Pause).BindToVolume(source);
                }
            }
        }

        //继续音乐
        public void ResumeMusic(int id)
        {
            if (Prefs.MusicVolume == 0)
                return;
            for (int i = 0; i < TempMusicPool.Count; i++)
            {
                var source = TempMusicPool[i].Source;
                if (source.GetHashCode() == id)
                {
                    if (MotionMap.TryGetValue(source, out var motion))
                        motion.Complete();
                    source.UnPause();
                    MotionMap[source] = LMotion.Create(0, mMusicVolume, 0.3f).WithUnscaleTime().BindToVolume(source);
                }
            }
        }

        public void ResumeMusicByFileName(string fileName)
        {
            if (Prefs.MusicVolume == 0)
                return;
            for (int i = 0; i < TempMusicPool.Count; i++)
            {
                var source = TempMusicPool[i].Source;
                if (source && source.clip.name == fileName)
                {
                    if (MotionMap.TryGetValue(source, out var motion))
                        motion.Complete();
                    source.UnPause();
                    MotionMap[source] = LMotion.Create(0, mMusicVolume, 0.3f).WithUnscaleTime().BindToVolume(source);
                }
            }
        }

        //停止音乐
        public async UniTask StopMusic(int id, float duration = 0.3f)
        {
            for (int i = 0; i < TempMusicPool.Count; i++)
            {
                var source = TempMusicPool[i].Source;
                if (source && source.GetHashCode() == id)
                {
                    if (MotionMap.TryGetValue(source, out var motion))
                        motion.Complete();
                    var tween = LMotion.Create(source.volume,MusicVolume, 0.3f).WithEase(Ease.InOutSine).WithUnscaleTime().WithOnComplete(
                        () =>
                        {
                            var instance = source;
                            MusicPool.Inst.Return(instance);
                            TempMusicPool.RemoveAt(i);
                            source.mute = true;
                        }).BindToVolume(source);
                    MotionMap[source] = tween;
                    await tween;
                    return;
                }
            }
        }

        public void StopAllMusic(float duration = 0.3f)
        {
            Debug.Log("StopAllMusic");
            for (int i = 0; i < TempMusicPool.Count; i++)
            {
                var node = TempMusicPool[i];
                var source = node.Source;
                if (source)
                {
                    source.enabled = false;
                    source.clip = null;
                    
                    if (MotionMap.TryGetValue(source, out var motion))
                    {
                        if(motion.IsActive())
                            motion.Complete();
                    }

                    MotionMap[source] = LMotion.Create(source.volume, 0, 0.3f).WithEase(Ease.InOutSine).WithUnscaleTime().WithOnComplete(
                        () =>
                        {
                            MusicPool.Inst.Return(source);
                            TempMusicPool.Remove(node);
                        }).BindToVolume(source);
                }
            }
        }

        public void StopMusicByPath(string path)
        {
            for (int i = 0; i < TempMusicPool.Count; i++)
            {
                var source = TempMusicPool[i].Source;
                if (source && source.clip.name == path)
                {
                    if (MotionMap.TryGetValue(source, out var motion))
                    {
                        if(motion.IsActive())
                            motion.Complete();
                    }

                    MotionMap[source] = LMotion.Create(source.volume, 0, 0.3f).WithEase(Ease.InOutSine).WithUnscaleTime().WithOnComplete(
                        () =>
                        {
                            var instance = source;
                            MusicPool.Inst.Return(instance);
                            TempMusicPool.RemoveAt(i);
                            source.mute = true;
                        }).BindToVolume(source);
                    return;
                }
            }
        }

        //=====================================================================================================================    
        //私有成员
        private string FindSource(string[] searchPath, string name)
        {
            for (int i = 0; i < searchPath.Length; i++)
            {
                var path = searchPath[i] + name;
                if (File.Exists(path))
                {
                    Log.Print("查找到文件！" + "    " + path);
                    return path;
                }
            }

            Log.Error("未查找到视频文件！" + "    " + name);
            return string.Empty;
        }

        private void SoundReturnToPool(AudioSource source)
        {
            //        Log.Print("回收声音:" + source.clip.name);
            if (!source) return;

            SoundPool.Inst.Return(source);
            for (int i = 0; i < TempSoundPool.Count; i++)
            {
                if (TempSoundPool[i].Source == source)
                {
                    var tempClip = TempSoundPool[i];
                    tempClip.Source.Stop();
                    tempClip.EndCallback?.Invoke(new Callback { Handle = tempClip.Source.GetHashCode() });
                    TempSoundPool.RemoveAt(i);
                    return;
                }
            }
        }

        private void ControlSound(float value)
        {
            var tempVal = mSoundVolume;
            SoundPool.Inst.Apply(source =>
            {
                if (MotionMap.TryGetValue(source, out var motion))
                {
                    if(motion.IsActive())
                        motion.Complete();
                }

                MotionMap[source] = LMotion.Create(tempVal, value, 0.5f).WithUnscaleTime().BindToVolume(source);
            });
            foreach (TempClip node in TempSoundPool)
            {
                var source = node.Source;
                if (MotionMap.TryGetValue(source, out var motion))
                {
                    if(motion.IsActive())
                        motion.Complete();
                }

                MotionMap[source] = LMotion.Create(tempVal, value, 0.5f).WithUnscaleTime().BindToVolume(source);
            }
        }

        public void ControlMusic(float value, float duration = 2)
        {
            var tempVal = mMusicVolume;
            MusicPool.Inst.Apply(source =>
            {
                if (MotionMap.TryGetValue(source, out var motion))
                {
                    if(motion.IsActive())
                        motion.Complete();
                }

                MotionMap[source] = LMotion.Create(tempVal, value, 0.5f).WithUnscaleTime().BindToVolume(source);
            });
            foreach (TempClip node in TempMusicPool)
            {
                var source = node.Source;
                if (MotionMap.TryGetValue(source, out var motion))
                {
                    if(motion.IsActive())
                        motion.Complete();
                }

                MotionMap[source] = LMotion.Create(tempVal, value, 0.5f).WithUnscaleTime().BindToVolume(source);
            }
        }
    }

    public class SoundPool : UnityObjectPool<AudioSource>
    {
        private static SoundPool mInst;

        private static GameObject mContainer { get; set; }

        public static SoundPool Inst
        {
            get
            {
                if (mInst == null)
                {
                    mContainer = new GameObject("Sound");
                    Object.DontDestroyOnLoad(mContainer);
                    mInst = new SoundPool();
                }

                return mInst;
            }
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Purge()
        {
            mInst = null;
            mContainer = null;
        }
#endif

        protected override void OnBeforeRent(AudioSource i)
        {
            i.enabled = true;
        }

        protected override void OnBeforeReturn(AudioSource i)
        {
            i.enabled = false;
        }

        public void Apply(Action<AudioSource> method)
        {
            if (Queue != null)
            {
                foreach (var node in Queue)
                {
                    method(node);
                }
            }
        }

        protected override AudioSource CreateInstance()
        {
            var source = mContainer.AddComponent<AudioSource>();
            source.volume = AudioManager.Inst.MusicVolume / 100f;
            return source;
        }
    }

    public class MusicPool : UnityObjectPool<AudioSource>
    {
        private static MusicPool mInst;

        private static GameObject mContainer { get; set; }

        public static MusicPool Inst
        {
            get
            {
                if (mInst == null)
                {
                    mContainer = new GameObject("Music");
                    Object.DontDestroyOnLoad(mContainer);
                    mInst = new MusicPool();
                }

                return mInst;
            }
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Purge()
        {
            mInst = null;
            mContainer = null;
        }
#endif

        protected override void OnBeforeRent(AudioSource i)
        {
            i.enabled = true;
        }

        protected override void OnBeforeReturn(AudioSource i)
        {
            i.enabled = false;
        }

        public void Apply(Action<AudioSource> method)
        {
            if (Queue != null)
            {
                foreach (var node in Queue)
                {
                    method(node);
                }
            }
        }

        protected override AudioSource CreateInstance()
        {
            var source = mContainer.AddComponent<AudioSource>();
            source.volume = AudioManager.Inst.MusicVolume / 100f;
            return source;
        }
    }
}
