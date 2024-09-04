using EntrustSystem;
using System.Collections.Generic;

public class CCD_Entrust
{
    /// <summary>
    /// 委托项目操作器
    /// </summary>
    public EntrustItemHandler EntrustItemHandler { get; private set; }

    /// <summary>
    /// 通过的冒险者信息
    /// 如果一个条件认为某个冒险者不通过，将他从这个列表中移除
    /// 之后下一个条件会继续对数组进行筛选，直到结束
    /// </summary>
    public List<VenturerInfo> PassVenturerInfo { get; private set; }

    /// <summary>
    /// 队伍是否通过
    /// 默认为通过，如果某个条件认为不通过，将此值设置为false
    /// </summary>
    public bool isPassTeam;

    /// <summary>
    /// 未通过，不用再进行条件确认了
    /// </summary>
    public bool NotPass => !isPassTeam || PassVenturerInfo.Count == 0;

    public CCD_Entrust(EntrustItemHandler entrustItemHandler)
    {
        this.EntrustItemHandler = entrustItemHandler;
        isPassTeam = true;

        PassVenturerInfo = new List<VenturerInfo>();

        int[] venturerTeam = EntrustItemHandler.GetVenturerTeam();
        for (int i = 0; i < venturerTeam.Length; i++)
        {
            VenturerInfo venturerInfo = VenturerModel.Instance.GetVenturerInfo(venturerTeam[i]);
            if (venturerInfo == null) continue;
            PassVenturerInfo.Add(venturerInfo);
        }
    }
}
