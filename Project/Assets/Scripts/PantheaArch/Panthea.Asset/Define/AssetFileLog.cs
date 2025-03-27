using System;
using UnityEngine.Scripting;

namespace Panthea.Asset
{
    public enum FileState : sbyte
    {
        Include = 0,
        Downloaded = 1,
        NotExists = 2
    }
    ///这里的结构未来需要优化减少部分字段
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    public partial class AssetFileLog
    {
        public uint Crc;
        public long Version;
        public string Path;
        public string[] Dependencies;
        public string[] Files;
		public FileState State = FileState.NotExists;
        public long Size;
        public string ShortBundlePath;
        public string RealPath;
#if USE_MEMORYPACK
        [MemoryPack.MemoryPackConstructor]
#elif USEJSON
        [Newtonsoft.Json.JsonConstructor]
#endif
        [Preserve]
        public AssetFileLog()
        {
        }
#if UNITY_EDITOR
        public AssetFileLog(uint crc, long version, string path, string[] files, string[] dependencies,FileState state,long size,string shortPath,string realPath)
        {
            Crc = crc;
            Version = version;
            Path = path;
            Files = files;
            Dependencies = dependencies;
            State = state;
            Size = size;
            ShortBundlePath = shortPath;
            RealPath = realPath;
        }
#endif
        public void CopyFrom(AssetFileLog other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other), "Cannot copy from a null AssetFileLog object.");
            }

            Crc = other.Crc;
            Version = other.Version;
            Path = other.Path;
            Dependencies = other.Dependencies;
            Files = other.Files;
            State = other.State;
            Size = other.Size;
            ShortBundlePath = other.ShortBundlePath;
            RealPath = other.RealPath;
        }
        
        public override string ToString()
        {
            return $"FileInfo:\n" +
                   $"  Crc: {Crc}\n" +
                   $"  Version: {Version}\n" +
                   $"  Path: {Path}\n" +
                   $"  Dependencies: [{string.Join(", ", Dependencies ?? Array.Empty<string>())}\n" +
                   $"  Files: [{string.Join(", ", Files ?? Array.Empty<string>())}\n" +
                   $"  State: {State}\n" +
                   $"  Size: {Size}\n" +
                   $"  ShortBundlePath: {ShortBundlePath}\n" +
                   $"  RealPath: {RealPath}";
        }
    }
}