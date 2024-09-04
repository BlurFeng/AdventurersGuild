
namespace FsStoryIncident
{
    public class StoryTaskExample : IStoryTask
    {
        private int mP1;
        private float mP2;

        public void Parse(string param)
        {
            //解析参数只进行一次，缓存参数使用，提高效率。
            if (StoryIncidentLibrary.SplitParams(param, StoryIncidentLibrary.SepType.Sep1, out string[] paramAry))
            {
                mP1 = 1; if (paramAry.Length > 0) mP1 = int.Parse(paramAry[0]);
                mP2 = 2f; if (paramAry.Length > 1) mP2 = float.Parse(paramAry[1]);
            }
        }

        public bool ExecuteTask(object customData)
        {
            //进行业务操作
            var num = mP1 + mP2;

            if(num - 1 >= 0)
            {
                return true;
            }

            return false;
        }

#if UNITY_EDITOR
        public string GetParamsComment()
        {
            return "示例方法；参1：int；参2：float";
        }
#endif
    }
}