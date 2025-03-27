using System;
using Panthea.Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace EdgeStudio
{
    [Serializable]
    public class GamePlayData
    {
        [FormerlySerializedAs("PlayerCreateRect")] [EditableRect] public Rect PlayerRect;
        [FormerlySerializedAs("EnemyCreateRect")] [EditableRect] public Rect EnemyRect;
        public CharacterTeam PlayerTeam;
        public CharacterTeam EnemyTeam;
    }
    
    /// <summary>
    /// 游戏中管理器.挂在GameScene中.如果在游戏外就会找不到这个Manager.
    /// </summary>
    public class InGameStartManager : MonoSingleton<InGameStartManager>
    {
        public GamePlayData Left;
        public GamePlayData Right;
        #region 测试数据
        public GameObject Newbie;
        #endregion
        void Awake()
        {
            //先创建玩家选择的角色
            // InstantiatePlayer();
        }

        async void InstantiatePlayer()
        {
            var x = await InstantiateAsync(Newbie,1);
        }
    }
}