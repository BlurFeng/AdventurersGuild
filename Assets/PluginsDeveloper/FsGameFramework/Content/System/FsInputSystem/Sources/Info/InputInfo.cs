using UnityEngine;

//用于处理硬件输入信息的一些信息缓存类
namespace FsGameFramework.InputSystem
{
    /// <summary>
    /// 按钮触点输入信息 含bool
    /// </summary>
    public class PointInfo
    {
        public bool Value;
        float timePoint;//时间点记录

        public PointInfo(bool inputValue)
        {
            Value = inputValue;
            TimePointRefresh();
        }

        /// <summary>
        /// 刷新timePoint时间
        /// </summary>
        public void TimePointRefresh()
        {
            timePoint = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// 获取当前timePoint和realtimeSinceStartup的时间差
        /// </summary>
        /// <param name="refreshLater"></param>
        /// <returns></returns>
        public float DeltaTime(bool refreshLater = true)
        {
            float time = Time.realtimeSinceStartup;
            float deltaTime = time - timePoint;
            if (refreshLater) timePoint = time;
            return deltaTime;
        }
    }

    /// <summary>
    /// 轴输入信息 含Vector3
    /// </summary>
    public class AxisInfo
    {
        public Vector3 Direction;
        float timePoint;//时间点记录

        public AxisInfo(Vector3 dir)
        {
            Direction = dir;
            TimePointRefresh();
        }

        /// <summary>
        /// 刷新timePoint时间
        /// </summary>
        public void TimePointRefresh()
        {
            timePoint = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// 获取当前timePoint和realtimeSinceStartup的时间差
        /// </summary>
        /// <param name="refreshLater"></param>
        /// <returns></returns>
        public float DeltaTime(bool refreshLater = true)
        {
            float time = Time.realtimeSinceStartup;
            float deltaTime = time - timePoint;
            if (refreshLater) timePoint = time;
            return deltaTime;
        }
    }
}