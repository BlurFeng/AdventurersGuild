using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FsGameFramework
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "NewGameModeConfig", menuName = "FsGameFramework/GameModeConfig", order = 22)]
    public class UGameModeConfig : ScriptableObject
    {
        [Header("游戏模式类")]
        [Tooltip("游戏模式规定了游戏的玩法逻辑")]
        public string GameModeClass = "FsGameFramework.UGameMode";

        [Header("开始游戏时默认的PlayerController")]
        [Tooltip("游戏开始后生成此玩家控制器,并Possess到默认的Pawn。")]
        public GameObject PlayerController;

        [Header("开始游戏时默认的Pawn")]
        [Tooltip("无论场景中是否存在其他的Pawn，开始游戏时PlayerController都会生成并Possess此Pawn")]
        public GameObject DefaultPawn ;

        [Header("游戏模式对应的数据类")]
        //[Tooltip("")]
        public string GameStateClass = "FsGameFramework.UGameState";

        [Header("玩家对应的数据类")]
        //[Tooltip("")]
        public string PlayerStateClass = "FsGameFramework.UPlayerState";

        //[Header("观众类")]
        //[Tooltip("")]
        //public string SpectatorClass;

        //public string HUDClass;
    }
}
