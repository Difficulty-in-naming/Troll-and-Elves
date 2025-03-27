namespace EdgeStudio
{
    public class CharacterStates 
    {
        public enum CharacterConditions
        {
            Normal,
            ControlledMovement,
            Frozen,
            Paused,
            Dead,
            Stunned
        }

        public enum MovementStates 
        {
            Idle,
            Walking,
            Running,
            Dashing,
        }
    }
}