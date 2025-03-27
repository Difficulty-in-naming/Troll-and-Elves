using System;
using EdgeStudio.Config;

using Panthea.Common;
#if USE_MEMORYPACK
[MemoryPack.MemoryPackable(SerializeLayout.Explicit)]
#endif
namespace EdgeStudio.DB
{
    public partial class Preference
    {
        private static readonly string mSavePath = GameSettings.PersistentDataPath + "/DB/Local/Preference.data";
        private bool Dirty { get; set; } = false;
        private float mSoundVolume = 1;
        private float mMusicVolume = 1;
        private bool mEnableVibrate = true;
        private bool mLowGraphics = false;
        private int mFailedTipsIndex = 0;
        private long mUserId = 0;
        private int mUserAcquisitionSource;
#if UNITY_EDITOR
        private int mLang = 2;//2代表简体中文
#else
    private int mLang;
#endif

#if USE_MEMORYPACK
    [MemoryPack.MemoryPackOrder(0)]
#endif
        public float SoundVolume
        {
            get => mSoundVolume;
            set
            {
                mSoundVolume = value;
                Dirty = true;
            }
        }

#if USE_MEMORYPACK
    [MemoryPack.MemoryPackOrder(1)]
#endif
        public float MusicVolume
        {
            get => mMusicVolume;
            set
            {
                mMusicVolume = value;
                Dirty = true;
            }
        }

#if USE_MEMORYPACK
    [MemoryPack.MemoryPackOrder(2)]
#endif
        public long UserId
        {
            get => mUserId;
            set
            {
                mUserId = value;
                Dirty = true;
                //Crashlytics.SetUserId(value.ToString());
            }
        }

#if USE_MEMORYPACK
    [MemoryPack.MemoryPackOrder(3)]
#endif
        public bool EnableVibrate
        {
            get => mEnableVibrate;
            set
            {
                mEnableVibrate = value;
                Dirty = true;
            } 
        }

#if USE_MEMORYPACK
    [MemoryPack.MemoryPackOrder(4)]
#endif
        public bool LowGraphics
        {
            get => mLowGraphics;
            set
            {
                mLowGraphics = value;
                Dirty = true;
            } 
        }

#if USE_MEMORYPACK
    [MemoryPack.MemoryPackOrder(5)]
#endif
        public int Lang
        {
            get => mLang;
            set
            {
                mLang = value;
                Language.CurrentLanguage = Language.Map(mLang);
                Dirty = true;
            }
        }

#if USE_MEMORYPACK
    [MemoryPack.MemoryPackOrder(6)]
#endif
        public int FailedTipsIndex
        {
            get => mFailedTipsIndex;
            set
            {
                mFailedTipsIndex = value;
                Dirty = true;
            }
        }
    
        /// <summary>
        /// 归因渠道.因为平台登录的时候必须立刻保存此时不能马上拉起玩家数据(因为要从服务器中获取存档等操作)，所以要先放在这里并立即保存
        /// </summary>
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackOrder(7)]
#endif
        public int UserAcquisitionSource
        {
            get => mUserAcquisitionSource;
            set
            {
                mUserAcquisitionSource = value;
                Dirty = true;
            }
        }
        
        //////////////////////////////////////
        /// 以下为自定义Preference
        //////////////////////////////////////

        private int mScriptSortType;

        public int ScriptSortType
        {
            get => mScriptSortType;
            set
            {
                mScriptSortType = value;
                Dirty = true;
            }
        }
        
        private long mZhuanYunDate;
        /// <summary> 转运日期 </summary>
        public long ZhuanYunDate
        {
            get => mZhuanYunDate;
            set
            {
                mZhuanYunDate = value;
                Dirty = true;
            }
        }


        private static Preference mInst;

        public static Preference Inst
        {
            get
            {
                if (mInst == null)
                {
                    try
                    {
                        mInst = SaveLoadUtils.Load<Preference>(mSavePath);
                        if (mInst == null)
                        {
                            mInst = new Preference();
                            SaveLoadUtils.Save(mSavePath, mInst);
                        }
                    }
                    catch(Exception ex)
                    {
                        Log.Error(ex);
                        mInst = new Preference();
                        SaveLoadUtils.Save(mSavePath, mInst);
                    }
                }

                return mInst;
            }
        }

        public void MarkDirty()
        {
            Inst.Dirty = true;
        }
    
        public static void Save()
        {
            if (Inst.Dirty == false)
            {
                return;
            }

            Inst.Dirty = false;
            SaveLoadUtils.Save(mSavePath, mInst);
        }
    }
}