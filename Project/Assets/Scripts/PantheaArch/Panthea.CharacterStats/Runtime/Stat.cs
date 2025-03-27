using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
using UnityEngine.Events;

namespace Panthea.CharacterStats
{
    public interface IStat
    {
        float InitialValue { get; }
        float BaseValue { get; set; }
        float Value { get; }    
        string Name { get; }
        IReadOnlyCollection<Modifier> GetModifiers(ModifierType modifierType);
    }
    
    public class Stat<T> : IStat
    {
        public float InitialValue => mInitialValue;

        public float BaseValue 
        { 
            get => mBaseValue + (mParent?.ValueWithoutPost ?? 0);
            set => mBaseValue = value;
        }

        public float Value
        {
            get
            {
                if (mIsDirty || !Mathf.Approximately(mLastBaseValue, BaseValue)) UpdateValues();
                return mValue;
            }
        }

        public string Name => Key.ToString();

        public T Key => mParent == null ? mKey : mParent.Key;

        public Observable<Stat<T>> OnChangeValue => OnChangeValueSubject;
        private readonly Subject<Stat<T>> OnChangeValueSubject = new();
        private readonly CompositeDisposable Disposables = new();
        private float ValueWithoutPost
        {
            get
            {
                if (mIsDirty || !Mathf.Approximately(mLastBaseValue, BaseValue)) UpdateValues();

                return mValueWithoutPost;
            }
        }

        private readonly float mInitialValue;
        private float mBaseValue;
        private readonly Stat<T> mParent;

        private readonly T mKey;
        private bool mIsDirty = true;
        private float mLastBaseValue;
        private float mValue;
        private float mValueWithoutPost;

        private readonly Dictionary<ModifierType, HashSet<Modifier>> mModifiers = new();

        public Stat(T key, float initialValue)
        {
            mInitialValue = initialValue;
            mBaseValue = mInitialValue;
            mKey = key;
        }
        
        public Stat(Stat<T> parent)
        {
            mInitialValue = 0f;
            mBaseValue = mInitialValue;
            mParent = parent;
            
            mParent.OnChangeValue.Subscribe(_ =>
            {
                mIsDirty = true;
                OnChangeValueSubject.OnNext(this);
            }).AddTo(Disposables);
        }

        public Stat(Stat<T> parent, float initialValue)
        {
            mInitialValue = initialValue;
            mBaseValue = mInitialValue;
            mParent = parent;
            
            mParent.OnChangeValue.Subscribe(_ =>
            {
                mIsDirty = true;
                OnChangeValueSubject.OnNext(this);
            }).AddTo(Disposables);
        }

        public void Add(Modifier modifier)
        {
            if (mModifiers.ContainsKey(modifier.Type) == false)
            {
                mModifiers.Add(modifier.Type, new HashSet<Modifier>());
            }

            if (mModifiers[modifier.Type].Add(modifier))
            {
                mIsDirty = true;
                OnChangeValueSubject.OnNext(this);
            }
        }

        public void Remove(Modifier modifier)
        {
            if (mModifiers.ContainsKey(modifier.Type) && 
                mModifiers[modifier.Type].Remove(modifier))
            {
                mIsDirty = true;
                OnChangeValueSubject.OnNext(this);
            }
        }

        public void RemoveByID(string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            var removed = false;
            
            foreach (var modifiers in mModifiers.Values)
            {
                removed |= modifiers.RemoveWhere(modifier => modifier.ID == id) > 0;
            }

            if (removed)
            {
                mIsDirty = true;
                OnChangeValueSubject.OnNext(this);
            }
        }

        public void RemoveBySource(object source)
        {
            if (source == null) return;

            var removed = false;

            foreach (var modifiers in mModifiers.Values)
            {
                removed |= modifiers.RemoveWhere(modifier => modifier.Source == source) > 0;
            }

            if (removed)
            {
                mIsDirty = true;
                OnChangeValueSubject.OnNext(this);
            }
        }
        
        public IReadOnlyCollection<Modifier> GetModifiers(ModifierType modifierType)
        {
            if (mModifiers.TryGetValue(modifierType, out var modifiers))
            {
                return modifiers;
            }

            return Enumerable.Empty<Modifier>().ToList();
        }

        protected virtual float CalculateValue(bool withPostModifier)
        {
            float value = BaseValue;

            value = CalculateAdd(value, withPostModifier);
            value = CalculatePercent(value, withPostModifier);
            value = CalculateMultiply(value, withPostModifier);
            value = CalculateReduce(value, withPostModifier);
            value = CalculateSubtract(value, withPostModifier);

            return value;
        }

        protected float CalculateAdd(float baseValue, bool withPostModifier)
        {
            if (mModifiers.TryGetValue(ModifierType.Add, out var modifiers))
            {
                foreach (var modifier in modifiers)
                {
                    if (modifier.IsPost) continue;

                    baseValue += modifier.Value;
                }
            }

            if (withPostModifier)
            {
                var postModifiers = GetPostModifiers(ModifierType.Add);

                foreach (var modifier in postModifiers)
                {
                    baseValue += modifier.Value;
                }
            }

            return baseValue;
        }

        protected float CalculatePercent(float baseValue, bool withPostModifier)
        {
            var percentAddSum = 0f;
            
            if (mModifiers.TryGetValue(ModifierType.Percent, out var modifiers))
            {
                foreach (var modifier in modifiers)
                {
                    if (modifier.IsPost) continue;

                    percentAddSum += modifier.Value;
                }
            }

            if (withPostModifier)
            {
                var postModifiers = GetPostModifiers(ModifierType.Percent);

                foreach (var modifier in postModifiers)
                {
                    percentAddSum += modifier.Value;
                }
            }

            return baseValue * (1f + percentAddSum);
        }

        protected float CalculateMultiply(float baseValue, bool withPostModifier)
        {
            if (mModifiers.TryGetValue(ModifierType.Multiply, out var modifiers))
            {
                foreach (var modifier in modifiers)
                {
                    if (modifier.IsPost) continue;

                    baseValue *= (1f + modifier.Value);
                }
            }

            if (withPostModifier)
            {
                var postModifiers = GetPostModifiers(ModifierType.Multiply);

                foreach (var modifier in postModifiers)
                {
                    baseValue *= (1f + modifier.Value);
                }
            }

            return baseValue;
        }
        
        protected float CalculateReduce(float baseValue, bool withPostModifier)
        {
            var reduceSum = 0f;
            
            if (mModifiers.TryGetValue(ModifierType.Reduce, out var modifiers))
            {
                foreach (var modifier in modifiers)
                {
                    if (modifier.IsPost) continue;

                    reduceSum += modifier.Value;
                }
            }

            if (withPostModifier)
            {
                var postModifiers = GetPostModifiers(ModifierType.Reduce);

                for (int i = 0; i < postModifiers.Count; i++)
                {
                    reduceSum += postModifiers[i].Value;
                }
            }

            return baseValue * Mathf.Max(0f, 1f - reduceSum);
        }

        protected float CalculateSubtract(float baseValue, bool withPostModifier)
        {
            if (mModifiers.TryGetValue(ModifierType.Subtract, out var modifiers))
            {
                foreach (var modifier in modifiers)
                {
                    if (modifier.IsPost) continue;

                    baseValue -= modifier.Value;
                }
            }

            if (withPostModifier)
            {
                var postModifiers = GetPostModifiers(ModifierType.Subtract);

                foreach (var modifier in postModifiers)
                {
                    baseValue -= modifier.Value;
                }
            }

            return baseValue;
        }
        
        private void UpdateValues()
        {
            mLastBaseValue = BaseValue;
            mValue = CalculateValue(true);
            mValueWithoutPost = CalculateValue(false);
            mIsDirty = false;
        }

        public void Dispose()
        {
            Disposables.Dispose();
            OnChangeValueSubject.Dispose();
        }

        private List<Modifier> GetPostModifiers(ModifierType modifierType)
        {
            var postModifiers = new List<Modifier>();

            if (mParent != null)
            {
                postModifiers.AddRange(mParent.GetPostModifiers(modifierType));
            }

            if (mModifiers.TryGetValue(modifierType, out var modifiers))
            {
                postModifiers.AddRange(modifiers.Where(modifier => modifier.IsPost));
            }

            return postModifiers;
        }
    }
}
