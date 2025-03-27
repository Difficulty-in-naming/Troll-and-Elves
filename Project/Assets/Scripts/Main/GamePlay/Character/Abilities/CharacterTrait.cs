using System.Collections.Generic;
using EdgeStudio.Odin;
using EdgeStudio.Traits;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
namespace EdgeStudio.Abilities
{


    [AddComponentMenu("Edge Studio/Character/Abilities/Character Trait Manager")]
    public class CharacterTrait : CharacterAbility
    {
        [ColorFoldout("特性管理"), Tooltip("角色的天生特性")]
        public List<Trait> InnateTraits = new();
        
        [ColorFoldout("特性管理"), Tooltip("游戏中可解锁的所有特性")]
        public List<Trait> AllAvailableTraits = new();
        
        [ColorFoldout("特性管理"), ReadOnly, Tooltip("角色当前激活的特性")]
        private readonly List<Trait> mActiveTraits = new();
        
        [ColorFoldout("特性管理"), ReadOnly, Tooltip("角色在游戏中获得的特性")]
        private readonly List<Trait> mAcquiredTraits = new();

        // 统计数据跟踪
        private int mEnemiesKilled = 0;
        private readonly Dictionary<string, int> mSpecificEnemiesKilled = new();
        private float mDamageDealt = 0;
        private float mDamageTaken = 0;
        
        // 特性解锁事件
        private readonly Subject<Trait> mOnTraitUnlocked = new();
        public Observable<Trait> OnTraitUnlocked => mOnTraitUnlocked;
        
        // 特性添加事件
        private readonly Subject<Trait> mOnTraitAdded = new();
        public Observable<Trait> OnTraitAdded => mOnTraitAdded;
        
        // 特性移除事件
        private readonly Subject<Trait> mOnTraitRemoved = new();
        public Observable<Trait> OnTraitRemoved => mOnTraitRemoved;
        
        // R3事件订阅的处理
        private readonly CompositeDisposable mDisposables = new();

        protected override void Initialization()
        {
            base.Initialization();
            
            // 添加天生特性
            foreach (var trait in InnateTraits)
            {
                AddTrait(trait, false);
            }
            
            // 订阅事件
            EnemyKilledEvent.Event.Subscribe(HandleEnemyKilled).AddTo(mDisposables);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            mDisposables.Clear();
        }

        public override void ProcessAbility()
        {
            base.ProcessAbility();
            
            foreach (var trait in mActiveTraits)
            {
                trait.OnUpdate();
            }
        }

        // 添加特性
        public void AddTrait(Trait trait, bool isAcquired = true)
        {
            Trait newTrait = Instantiate(trait);
            newTrait.Initialize(Character, this);
            
            if (isAcquired)
            {
                mAcquiredTraits.Add(newTrait);
            }
            
            mActiveTraits.Add(newTrait);
            newTrait.OnTraitAdded();
            
            // 触发事件
            mOnTraitAdded.OnNext(newTrait);
        }

        // 移除特性
        public void RemoveTrait(Trait trait)
        {
            Trait traitToRemove = mActiveTraits.Find(t => t.TraitID == trait.TraitID);
            
            if (traitToRemove != null)
            {
                traitToRemove.OnTraitRemoved();
                mActiveTraits.Remove(traitToRemove);
                mAcquiredTraits.Remove(traitToRemove);
                
                mOnTraitRemoved.OnNext(traitToRemove);
                
                Destroy(traitToRemove);
            }
        }

        // 事件处理
        private void HandleEnemyKilled(EnemyKilledEvent enemy)
        {
            mEnemiesKilled++;
            
            /*
            if (!mSpecificEnemiesKilled.ContainsKey(enemyType))
            {
                mSpecificEnemiesKilled[enemyType] = 0;
            }
            mSpecificEnemiesKilled[enemyType]++;
            
            // 通知所有特性
            foreach (var trait in mActiveTraits)
            {
                trait.OnEnemyKilled(enemy, enemyType);
            }*/
        }

        private void HandleDamageDealt(GameObject target, float damage)
        {
            mDamageDealt += damage;
            
            foreach (var trait in mActiveTraits)
            {
                trait.OnDamageDealt(target, damage);
            }
        }

        private void HandleDamageTaken(GameObject source, float damage)
        {
            mDamageTaken += damage;
            
            foreach (var trait in mActiveTraits)
            {
                trait.OnDamageTaken(source, damage);
            }
        }
        
        // 获取特性数据用于检查条件
        public int GetEnemiesKilled() => mEnemiesKilled;
        public int GetSpecificEnemiesKilled(string enemyType) => mSpecificEnemiesKilled.ContainsKey(enemyType) ? mSpecificEnemiesKilled[enemyType] : 0;
        public float GetDamageDealt() => mDamageDealt;
        public float GetDamageTaken() => mDamageTaken;
        
        //======== 从TraitUnlockSystem整合的方法 ========//
        
        // 解锁一个随机特性
        [Button("解锁随机特性")]
        public void UnlockRandomTrait()
        {
            List<Trait> availableTraits = new List<Trait>();
            
            foreach (var trait in AllAvailableTraits)
            {
                if (!mAcquiredTraits.Exists(t => t.TraitID == trait.TraitID))
                {
                    availableTraits.Add(trait);
                }
            }
            
            if (availableTraits.Count > 0)
            {
                int randomIndex = Random.Range(0, availableTraits.Count);
                UnlockTrait(availableTraits[randomIndex]);
            }
        }
        
        // 解锁特定特性
        public void UnlockTrait(Trait trait)
        {
            if (!mAcquiredTraits.Exists(t => t.TraitID == trait.TraitID))
            {
                AddTrait(trait);
                
                // 触发解锁事件
                mOnTraitUnlocked.OnNext(trait);
            }
        }
        
        // 获取随机特性选择（如升级时提供选择）
        public List<Trait> GetRandomTraitSelection(int count)
        {
            List<Trait> selection = new List<Trait>();
            List<Trait> availableTraits = new List<Trait>();
            
            foreach (var trait in AllAvailableTraits)
            {
                if (!mAcquiredTraits.Exists(t => t.TraitID == trait.TraitID))
                {
                    availableTraits.Add(trait);
                }
            }
            
            int selectionCount = Mathf.Min(count, availableTraits.Count);
            
            for (int i = 0; i < selectionCount; i++)
            {
                if (availableTraits.Count == 0) break;
                
                int randomIndex = Random.Range(0, availableTraits.Count);
                selection.Add(availableTraits[randomIndex]);
                availableTraits.RemoveAt(randomIndex);
            }
            
            return selection;
        }
        
        // 判断特性是否已解锁
        public bool IsTraitUnlocked(string traitID)
        {
            return mAcquiredTraits.Exists(t => t.TraitID == traitID);
        }
    }
}
