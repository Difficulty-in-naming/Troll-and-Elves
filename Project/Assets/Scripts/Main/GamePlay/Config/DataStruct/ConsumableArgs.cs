using System;

namespace EdgeStudio.Config
{
    [Serializable]
    public class ConsumableArgs
    {
        public float Hunger;
        public float Thirst;
        public float Radiation;
        public float Health;
        public float Fatigue;
        public float Duration;
        public float MoveSpeed;
        public float Bleed;
        public bool BleedImmune;
        public float Stamina;
        public float MaxStamina;
        public float MaxHp;
        public float HungerSpeed;
        public float ThirstSpeed;
        public float RadiationDefense;
    }
}