using System.Collections.Generic;

namespace Panthea.Asset
{
    public class AssetRouter
    {
        private readonly Dictionary<object, ResPak> mLookup = new();

        public void Add(object obj, ResPak ab) => mLookup.TryAdd(obj, ab);

        public void Remove(object obj) => mLookup.Remove(obj);
        
        public ResPak Get(object obj) => mLookup.GetValueOrDefault(obj);
    }
}