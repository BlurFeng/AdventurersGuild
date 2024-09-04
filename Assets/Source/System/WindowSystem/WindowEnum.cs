
public enum WindowEnum
{
    Undefined = 0,

	#region 通用界面（1~99）
	/// <summary>
	/// 开发者展示界面
	/// </summary>
	DeveloperWindow = 1, 

	/// <summary>
	/// 异步加载界面（主菜单→主游戏 进度条）
	/// </summary>
	AsyncLoadWindow = 2, 

	/// <summary>
	/// 通知弹窗
	/// </summary>
	NotificationWindow = 3,

	/// <summary>
	/// GM窗口
	/// </summary>
	GMWindow = 4,

	/// <summary>
	/// 信息弹窗
	/// </summary>
	InfoWindow = 5,

	/// <summary>
	/// 光标窗口
	/// </summary>
	CursorWindow = 6,

	/// <summary>
	/// Esc菜单
	/// </summary>
	EscMenuWindow = 7,
	#endregion

	#region 功能界面，每个大功能预留100供子功能使用（例如卡牌界面500，卡牌相关的可以用501-599）
	/// <summary>
	/// 主菜单
	/// </summary>
	MainMenuWindow = 100,

	/// <summary>
	/// 存档菜单
	/// </summary>
	MenuLoadWindow = 110,

	/// <summary>
	/// 设置菜单
	/// </summary>
	MenuSettingWindow = 120,

	/// <summary>
	/// 主游戏
	/// </summary>
	MainGameWindow = 200,

	/// <summary>
	/// 公会建造
	/// </summary>
	GuildBuildWindow = 300,

	/// <summary>
	/// 建筑信息
	/// </summary>
	BuildingInfoWindow = 310,

	/// <summary>
	/// 建筑区域设定
	/// </summary>
	BuildingAreaWindow = 320,

	/// <summary>
	/// 冒险者列表
	/// </summary>
	VenturerListWindow = 400,

	/// <summary>
	/// 冒险者信息
	/// </summary>
	VenturerInfoWindow = 410,

	/// <summary>
	/// 委托界面
	/// </summary>
	EntrustWindow = 500,

	/// <summary>
	/// 角色编辑界面
	/// </summary>
	RoleEditorWindow = 600,

	/// <summary>
	/// 笔记本窗口
	/// </summary>
	NotebookWindow = 700,
	#endregion
}
