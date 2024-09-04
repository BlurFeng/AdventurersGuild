using System;
using Deploy;
using FsStoryIncident;
using com.ootii.Messages;

/// <summary>
/// 冒险模块
/// 主要处理委托过程中冒险者(或玩家)触发事件并推进故事
/// </summary>
public class AdventureModel : Singleton<AdventureModel>, IDestroy, ISaveData
{
    public override void Init()
    {
        base.Init();

        StoryIncidentManager.Instance.Init(ConfigSystem.Instance.StoryIncidentInitConfigContainer.Get("SIIC_Default"));

        MessageDispatcher.AddListener(TimeModelMsgType.TIMEMODEL_DAYCOUNT_CHANGE, OnExecuteFinishTurn);
    }

    public override void Destroy()
    {
        base.Destroy();

        MessageDispatcher.RemoveListener(TimeModelMsgType.TIMEMODEL_DAYCOUNT_CHANGE, OnExecuteFinishTurn);
    }

    private void OnExecuteFinishTurn(IMessage rMessage)
    {
        
    }

    #region ISaveData
    public void SaveData(ES3File saveData)
    {
        
    }

    public void LoadData(ES3File saveData)
    {
        
    }
    #endregion

    /// <summary>
    /// 发生一个事件
    /// </summary>
    /// <param name="incidentPackGuids"></param>
    /// <param name="incidentHandler"></param>
    /// <returns></returns>
    public bool IncidentHappen(Guid[] incidentPackGuids, out IncidentHandler incidentHandler, object customData = null)
    {
        return StoryIncidentManager.RandomIncidentHandler(incidentPackGuids, out incidentHandler, customData);
    }

    /// <summary>
    /// 完成一个事件
    /// </summary>
    /// <param name="incidentHandler"></param>
    public void IncidentDone(IncidentHandler incidentHandler)
    {
        StoryIncidentManager.DestroyIncidentHandler(incidentHandler);
    }
}
