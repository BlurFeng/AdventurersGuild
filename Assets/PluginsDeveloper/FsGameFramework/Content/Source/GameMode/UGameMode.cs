using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FsGameFramework
{
    /// <summary>
    /// 游戏模式 一个World对应一个
    /// 游戏模式规定了这个
    /// </summary>
    public class UGameMode : UObject
    {
        private UGameState m_GameState;
        /// <summary>
        /// 游戏模式对应的数据类
        /// </summary>
        public UGameState GameState
        {
            get
            {
                return m_GameState;
            }

            set
            {
                m_GameState = value;
            }
        }
    }
}
