using System;
using UnityEngine;

namespace Panthea.CharacterStats
{
    [Serializable]
    public class ModifierType : IComparable<ModifierType>, IEquatable<ModifierType>
    {
        public static ModifierType Add = new(1, nameof(Add));
        public static ModifierType Percent = new(2, nameof(Percent));
        public static ModifierType Multiply = new(3, nameof(Multiply));
        public static ModifierType Reduce = new(4, nameof(Reduce));
        public static ModifierType Subtract = new(5, nameof(Subtract));

        [SerializeField] private string _name;
        [SerializeField] private int _id;

        public string Name => _name;
        public int ID => _id;

        public ModifierType(int id, string name) => (_id, _name) = (id, name);

        public override int GetHashCode() => ID;
        
        public int CompareTo(ModifierType other) => ID.CompareTo(other.ID);
        
        public bool Equals(ModifierType other) => other != null && ID.Equals(other.ID);
    }
}