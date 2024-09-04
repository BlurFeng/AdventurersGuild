using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FsGameFramework
{
    /// <summary>
    /// 测试模式
    /// </summary>
    public enum TestMode
    {
        None,
        PlayerControllerSwitchTest,//玩家控制器切换控制对象测试

        Max,
    }

    public class UEditorTestSystem : USystem
    {
        //当前测试模式
        TestMode m_TestMode;
        int m_TestModeIndex;

        public UEditorTestSystem()
        {
            Init();

            //启动时默认测试模式
            SwitchTestMode(TestMode.PlayerControllerSwitchTest);
        }

        public override void Init()
        {
            base.Init();

            PlayerControllerSwitchTestInit();
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            if (Input.GetKeyDown(KeyCode.F12))
            {
                //切换到下个测试模式
                if (m_TestModeIndex < (int)TestMode.Max - 1)
                {
                    m_TestModeIndex++;
                    while (m_TestModeIndex < (int)TestMode.Max && ((TestMode)m_TestModeIndex).GetType() != typeof(TestMode))
                    {
                        m_TestModeIndex++;
                    }
                }
                else
                {
                    m_TestModeIndex = 0;
                }
                SwitchTestMode((TestMode)m_TestModeIndex);
            }

            //执行当前测试模式的Tick
            switch (m_TestMode)
            {
                case TestMode.PlayerControllerSwitchTest:
                    PlayerControllerSwitchTestUpdate();
                    break;
            }
        }

        public void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 300, 50), string.Format("当前测试模式:{0}\n按F12切换测试模式", m_TestMode.ToString()));

            switch (m_TestMode)
            {
                case TestMode.PlayerControllerSwitchTest:
                    GUI.Box(new Rect(10, 100, 300, 300), string.Format("当前模式信息\n当前操作的Pawn对象:{0}\n当前模式操作方式\nF4:切换玩家控制器控制的Pawn",
                        m_PossessPawn == null ? "null" : m_PossessPawn.GameObjectGet.name.ToString()));
                    break;
            }
        }

        /// <summary>
        /// 切换当前测试模式
        /// </summary>
        /// <param name="testMode"></param>
        public void SwitchTestMode(TestMode testMode)
        {
            if (m_TestMode == testMode) return;

            m_TestMode = testMode;
            m_TestModeIndex = (int)testMode;
            switch (m_TestMode)
            {
                case TestMode.PlayerControllerSwitchTest:
                    break;
            }

            Debug.Log("测试模式TestMode切换->" + m_TestMode.ToString());
        }

        /// <summary>
        /// 创建一个简单GameObject 并添加脚本
        /// </summary>
        /// <typeparam name="T">添加到GameObject的类</typeparam>
        /// <returns></returns>
        T SimpleCreat<T>() where T : MonoBehaviour
        {
            GameObject obj = new GameObject();
            obj.transform.position = Vector3.zero;
            obj.transform.rotation = Quaternion.identity;
            T t = obj.AddComponent<T>();
            obj.name = t.GetType().ToString();

            return t;
        }

        #region PlayerControllerSwitchTest 玩家控制器切换控制对象测试
        APlayerController m_PlayerController;//玩家控制器
        APlayerController PlayerController
        {
            get
            {
                if(m_PlayerController == null)
                {
                    var ps = FWorldContainer.GetActors<APlayerController>();
                    if (ps.Count > 0)
                        m_PlayerController = ps[0];
                }

                return m_PlayerController;
            }
        }
        List<APawn> m_APawns;//场景中所有的Pawn
        List<APawn> APawns
        {
            get
            {
                if(m_APawns.Count == 0)
                {
                    var mPawns = FWorldContainer.GetActors<APawn>();
                    if (mPawns != null)
                        m_APawns.AddRange(mPawns);
                }

                return m_APawns;
            }
        }
        APawn m_PossessPawn;//possess的pawn
        int m_PossessPawnIndex;

        //初始化
        void PlayerControllerSwitchTestInit()
        {
            m_APawns = new List<APawn>();
        }

        void PlayerControllerSwitchTestUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F4))
            {
                SwitchPlayerControllerPossessPawn();
            }
        }

        /// <summary>
        /// 切换PlayerController的控制对象Pawn
        /// </summary>
        /// <param name="testMode"></param>
        void SwitchPlayerControllerPossessPawn()
        {
            if (PlayerController == null || APawns.Count == 0) return;

            if (!PlayerController.IsPossess)
            {
                m_PossessPawnIndex = 0;
                m_PossessPawn = APawns[m_PossessPawnIndex];
                PlayerController.Possess(m_PossessPawn);
            }
            else
            {
                if (m_PossessPawnIndex < APawns.Count - 1)
                    m_PossessPawnIndex++;
                else
                    m_PossessPawnIndex = 0;

                m_PossessPawn = APawns[m_PossessPawnIndex];
                PlayerController.Possess(m_PossessPawn);
            }

            //if(mPossessPawn is ARoverPawn)
            //{
            //    //摄像机转换到手动模式 设置跟随目标为空
            //    CameraSystemManager.Instance.SwitchCameraMode(CameraMode.ManualMode);
            //    CameraSystemManager.Instance.SwitchFollowTarget(null);
            //}
            //else
            //{
            //    //摄像机转换到自动模式 跟随目标为控制的Pawn
            //    CameraSystemManager.Instance.SwitchCameraMode(CameraMode.AutoMode);
            //    CameraSystemManager.Instance.SwitchFollowTarget(mPossessPawn.GetCameraFollowTarget());
            //}
        }
        #endregion
    }
}
