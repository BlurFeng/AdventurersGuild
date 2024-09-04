using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FsGameFramework;
using FsPostProcessSystem;

public class GameInstance : FsGameFramework.FGameInstance
{
    /// <summary>
    /// 正在进行游戏
    /// </summary>
    public static bool IsPlayingGame = false;

    protected override void Awake()
    {
        IsPlayingGame = Application.isPlaying;

        //初始化游戏使用到的组件或系统
        Transform trans = transform;

        //摄像机组
        Camera cameraUI = null;
        m_ProcedureSystem.AddNode(new ProcedureNode((procedureSystem) =>
        {
            string addr = AssetAddressUtil.GetPrefabGamePlayAddress("CameraGroup");
            AssetSystem.Instance.LoadPrefab(addr, (prefab) =>
            {
                //获取摄像机
                var cameraMain = prefab.transform.Find("MainCamera").GetComponent<Camera>();
                CameraModel.Instance.SetCameraMain(cameraMain);
                cameraUI = prefab.transform.Find("UICamera").GetComponent<Camera>();
                CameraModel.Instance.SetCameraUI(cameraUI);

                //打开后处理 视口变换
                PostProcessSystem.Instance.OpenEffect<ViewportTransEffect>();

                procedureSystem.OnContinueNode();
            }, trans);
        }));

        //UI界面系统
        m_ProcedureSystem.AddNode(new ProcedureNode((procedureSystem) =>
        {
            string addr = AssetAddressUtil.GetPrefabGamePlayAddress("WindowSystem");
            AssetSystem.Instance.LoadPrefab(addr, (prefab) =>
            {
                Canvas canvas = prefab.GetComponent<Canvas>();
                canvas.worldCamera = cameraUI;
                canvas.planeDistance = 1f;

                procedureSystem.OnContinueNode();
            }, trans);
        }));

        //音频系统
        m_ProcedureSystem.AddNode(new ProcedureNode((procedureSystem) =>
        {
            string addr = AssetAddressUtil.GetPrefabGamePlayAddress("AudioSystem");
            AssetSystem.Instance.LoadPrefab(addr, (prefab) =>
            {
                procedureSystem.OnContinueNode();
            }, trans);
        }));

        //配置系统
        m_ProcedureSystem.AddNode(new ProcedureNode((procedureSystem) =>
        {
            ConfigSystem.Instance.Init(() =>
            {
                //模块创建
                AssetTemplateSystem.Instance.Create(); //预制体模板
                WeatherModel.Instance.Create(); //气象系统

                //UI界面系统 初始化
                WindowSystem.Instance.InitWindowGroup(); 

                procedureSystem.OnContinueNode();
            });
        }));

        //状态系统初始化
        m_ProcedureSystem.AddNode(new ProcedureNode((procedureSystem) =>
        {
            string addr = "Assets/ProductAssets/ConfigJson/ActorState.json";
            AssetSystem.Instance.LoadJson(addr, (textAsset) =>
            {
                List<Dictionary<string, string>> stateDicList = new List<Dictionary<string, string>>();
                stateDicList = LitJson.JsonMapper.ToObject<List<Dictionary<string, string>>>(textAsset.text);
                FsStateSystem.FsStateSystemManager.Instance.Init(stateDicList, "KeyState");

                procedureSystem.OnContinueNode();
            });
        }));

        //打开 主菜单界面（最后执行）
        m_ProcedureSystem.AddNode(new ProcedureNode((procedureSystem) =>
        {
            //进入 开发者展示界面
            WindowSystem.Instance.OpenWindow(WindowEnum.DeveloperWindow);

            procedureSystem.OnContinueNode();
        }));

        base.Awake();
    }

#if UNITY_EDITOR
    #region Gizmos调试
    [Header("Gizmos调试")]
    [SerializeField] private bool m_GizmosAreaInfo;

    private void OnDrawGizmos()
    {
        //网格系统-区域
        if (m_GizmosAreaInfo)
        {
            foreach (var areaGroupInfo in GuildGridModel.Instance.GridCellSystemManager.DicAreaGroupInfo.Values)
            {
                //区域项目
                foreach (var areaItem in areaGroupInfo.DicAreaInfo.Values)
                {
                    Gizmos.color = Color.white;
                    var centerPos = GuildGridModel.Instance.GetWorldPosition(areaItem.GridCoord + areaItem.Size * 0.5f);
                    Gizmos.DrawWireCube(centerPos, GuildGridModel.Instance.GetWorldPosition(areaItem.Size));
                }

                //区域组边界
                Gizmos.color = Color.blue;
                var centerPos1 = GuildGridModel.Instance.GetWorldPosition(areaGroupInfo.BoundGridCoord + areaGroupInfo.BoundSize * 0.5f);
                Gizmos.DrawWireCube(centerPos1, GuildGridModel.Instance.GetWorldPosition(areaGroupInfo.BoundSize));
            }

            //当前玩家所处区域项目
            if (GuildGridModel.Instance.PlayerAreaInfoCur != null)
            {
                Gizmos.color = Color.green;
                var centerPos = GuildGridModel.Instance.GetWorldPosition(GuildGridModel.Instance.PlayerAreaInfoCur.GridCoord + GuildGridModel.Instance.PlayerAreaInfoCur.Size * 0.5f);
                Gizmos.DrawWireCube(centerPos, GuildGridModel.Instance.GetWorldPosition(GuildGridModel.Instance.PlayerAreaInfoCur.Size));
            }
        }
    }
    #endregion
#endif
}
