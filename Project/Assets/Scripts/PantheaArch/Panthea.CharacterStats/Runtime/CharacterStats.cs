using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
using UnityEngine.Events;

namespace Panthea.CharacterStats
{
    public interface ICharacterStats : IDisposable
    {
        string Name { get; }
        IReadOnlyList<IStat> Stats { get; }
    }
    
    [System.Serializable]
    public class CharacterStats<T> : ICharacterStats
    {
        public string Name { get;}
        public IReadOnlyList<IStat> Stats => mStats.Values.Cast<IStat>().ToList();
        public IReadOnlyDictionary<T, Stat<T>> All => mStats;
        public UnityEvent<CharacterStats<T>, Stat<T>> OnChangeStat { get; } = new();

        public Stat<T> this[T key]
        {
            get
            {
                if (mStats.TryGetValue(key, out var item))
                {
                    return item;
                }

                Debug.LogErrorFormat("[CharacterStats] Can't found stat - {0}", key);
                return null;
            }
        }
        
        public CharacterStats<T> Parent { get; protected set; }

        protected readonly Dictionary<T, Stat<T>> mStats = new();

        public CharacterStats()
        {
            CharacterStatsManager.Add(this);
        }

        public CharacterStats(string name)
        {
            Name = name;
            
            CharacterStatsManager.Add(this);
        }
        
        public CharacterStats(string name, CharacterStats<T> parent)
        {
            Name = name;
            Parent = parent;

            foreach (var item in Parent.All)
            {
                var stat = CreateStat(item.Value);

                stat.OnChangeValue.Subscribe(OnChangeValue);
                
                mStats.Add(item.Key, stat);
            }

            CharacterStatsManager.Add(this);
        }

        public virtual Stat<T> CreateStat(Stat<T> parent) => new Stat<T>(parent);

        public void Dispose() => CharacterStatsManager.Remove(this);

        public bool Contains(T key) => mStats.ContainsKey(key);

        public bool AddStat(T key, float initialValue)
        {
            if (mStats.ContainsKey(key))
            {
                Debug.LogWarningFormat("[CharacterStats] AddStat : Already added - {0}", key);
                return false;
            }

            var stat = new Stat<T>(key, initialValue);

            stat.OnChangeValue.Subscribe(OnChangeValue);

            mStats.Add(key, stat);

            return true;
        }

        public bool AddModifier(T key, Modifier modifier)
        {
            if (mStats.ContainsKey(key))
            {
                mStats[key].Add(modifier);
                return true;
            }
            else
            {
                Debug.LogWarningFormat("[CharacterStats] AddModifier : Can't found stat - {0}", key);
                return false;
            }
        }

        public void RemoveModifier(T key, Modifier modifier)
        {
            if (mStats.ContainsKey(key))
            {
                mStats[key].Remove(modifier);
            }
            else
            {
                Debug.LogErrorFormat("[CharacterStats] RemoveModifier : Can't found stat - {0}", key);
            }
        }
        
        public void RemoveModifierByID(string id)
        {
            foreach (var stat in mStats.Values)
            {
                stat.RemoveByID(id);
            }
        }
        
        public void RemoveModifierBySource(object source)
        {
            foreach (var stat in mStats.Values)
            {
                stat.RemoveBySource(source);
            }
        }

        private void OnChangeValue(Stat<T> stat)
        {
            OnChangeStat.Invoke(this, stat);
        }
    }
}