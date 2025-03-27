using EdgeStudio.Event;

namespace EdgeStudio.AAA
{
    /// <summary>
    /// CharacterEvents 用于补充由角色状态机触发的事件，以信号表示发生的事情，这些事情不一定与状态变化相关。
    /// </summary>
    [GameEvent]
    public partial struct CharacterEvent
    {
        public int TargetCharacter; // 目标角色
    }
}
