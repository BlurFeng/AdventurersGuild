using System.Collections.Generic;
using UnityEngine;
using FsGameFramework;

namespace FInteractionSystem
{
    /// <summary>
    /// 定义筛选方法要如何获得被筛选对象数组中的位置 这给使用者提供了优化此部分代码的空间
    /// 比如所有的GameObject对象上的Actor类都继承自某一个基类且此基类有中心位置的缓存
    /// </summary>
    /// <param name="targetComponent"></param>
    /// <returns></returns>
    public delegate Vector3 GetTargetPosition(Component targetComponent);

    /// <summary>
    /// F交互系统方法库
    /// </summary>
    public class FInteractionFunctionLibrary
    {
        #region 方位 Orientation
        //20210509 Winhoo
        //此处方法仅需要满足工具即可 不用提升到某个Utility类中供整个项目使用
        //这会使这部分代码和Utility中的重复 但保证了插件的独立性
        //但之后可能按需提升到FGameFramework命名空间下的Utility类中

        /// <summary>
        /// 确认前后方位
        /// </summary>
        /// <param name="originTs">中心方位信息</param>
        /// <param name="compareTs">比较方位信息</param>
        /// <returns>true=在正面 false=在反面</returns>
        static bool ConfirmOrientationFrontOrBack(Transform originTs, Transform compareTs)
        {
            Vector3 originFront = originTs.forward;
            Vector3 compareDir = (compareTs.position - originTs.position).normalized;
            float diffVal = Vector3.Dot(compareDir, originFront);

            if (diffVal > 0)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region Detection Object 探测对象功能

        /// <summary>
        /// 探测对象
        /// </summary>
        /// <param name="findComponent">最符合的对象</param>
        /// <param name="originPosition">探测源点 位置</param>
        /// <param name="originReferenceDir">探测源点 参照方向</param>
        /// <param name="layerMask">检测层级layer</param>
        /// <param name="radiusLimit">探测范围 半径</param>
        /// <param name="angleLimit">探测范围 角度</param>
        /// <param name="toleranceRange">容许范围 当两个对象的距离差小于此值时，优先获取更接近参照方向的对象</param>
        /// <param name="filteredClass">目标类型筛选 如果为空则不筛选</param>
        /// <param name="containsSubclass">筛选时包含此类及其子类</param>
        /// <param name="ignoreColliders">探测时忽略的Actor 一般为组件所有者</param>
        /// <param name="getTargetPositionFunc">获取探测目标中心点的方法 可设置更优化的方法</param>
        /// <param name="debugDraw">是否在场景中绘制探测范围</param>
        /// <returns>是否找到了有效目标</returns>
        public static bool DetectionObject(
            out Component findComponent,
            Vector3 originPosition, Vector3 originReferenceDir, [UnityEngine.Internal.DefaultValue("AllLayers")] int layerMask = 0,
            float radiusLimit = 10f, float angleLimit = 90f, float toleranceRange = 1f, System.Type filteredClass = null, bool containsSubclass = true,
            Collider[] ignoreColliders = null,
            GetTargetPosition getTargetPositionFunc = null, bool debugDraw = false)
        {
            OverlapSphereOutComponents(originPosition, radiusLimit, layerMask, ignoreColliders, out Component[] componentArray);

            if (componentArray.Length == 0)
            {
                findComponent = null;
                return false;
            }

            return DetectionObjectByObjectArray(
                componentArray, out findComponent, originPosition, originReferenceDir,
                radiusLimit, angleLimit, toleranceRange, filteredClass, containsSubclass,
                getTargetPositionFunc, debugDraw);
        }

        /// <summary>
        /// 探测对象 根据提供的对象数组
        /// </summary>
        /// <param name="components">被探测对象数组</param>
        /// <param name="findComponent">找到的符合条件的目标类型对象</param>
        /// <param name="originPosition">探测源点 位置</param>
        /// <param name="originReferenceDir">探测源点 参照方向</param>
        /// <param name="radiusLimit">探测范围 半径</param>
        /// <param name="angleLimit">探测范围 角度</param>
        /// <param name="toleranceRange">容许范围 当两个对象的距离差小于此值时，优先获取更接近参照方向的对象</param>
        /// <param name="filteredClass">目标类型筛选 如果为空则不筛选</param>
        /// <param name="containsSubclass">筛选时包含此类及其子类</param>
        /// <param name="getTargetPositionFunc">探测时忽略的Actor 一般为组件所有者</param>
        /// <param name="debugDraw">是否在场景中绘制探测范围</param>
        /// <returns>是否找到了有效目标</returns>
        public static bool DetectionObjectByObjectArray(
            Component[] components, out Component findComponent,
            Vector3 originPosition, Vector3 originReferenceDir,
            float radiusLimit = 10f, float angleLimit = 90f, float toleranceRange = 1f, System.Type filteredClass = null, bool containsSubclass = true,
            GetTargetPosition getTargetPositionFunc = null, bool debugDraw = false)
        {
            if (debugDraw)
            {
                //绘制探测返范围方法
            }

            findComponent = null;

            if (components.Length == 0) return false;

            float maxDiff = -2f;
            float minDis = radiusLimit + 1f;

            float angleLimitRad = angleLimit * Mathf.Deg2Rad;

            //筛选出最符合要求的对象
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];

                //有筛选指定类 确认是否符合
                if(null != filteredClass)
                {
                    if (containsSubclass)
                    {
                        if (!component.GetType().IsSubclassOf(filteredClass))
                            continue;
                    }
                    else if (component.GetType() != filteredClass)
                            continue;
                }

                //获取信息
                Vector3 componentOrigin;//获取中心
                if (getTargetPositionFunc == null)
                    componentOrigin = GetTargetPosition(component);
                else
                    componentOrigin = getTargetPositionFunc(component);

                //比较component是否符合要求
                if (CompareByLimit(
                    componentOrigin, originPosition, originReferenceDir, radiusLimit, angleLimit, toleranceRange,
                    ref minDis, ref maxDiff, out bool goodDirOrDis, out bool goodCompareOrLimit)
                    && goodCompareOrLimit)
                {
                    findComponent = component;
                }
            }

            return findComponent != null;
        }

        /// <summary>
        /// 对比目标位置是否在限制内
        /// </summary>
        /// <param name="targetOriginPos">目标原位置</param>
        /// <param name="originPos">对比原点位置</param>
        /// <param name="originDir">对比原点方向</param>
        /// <param name="radiusLimit">范围半径限制</param>
        /// <param name="angleLimit">角度限制 0-360</param>
        /// <param name="toleranceRange">容许范围 当和对比距离的差小于此值时，优先获取更接近参照方向的对象</param>
        /// <param name="minDis">对比最小距离</param>
        /// <param name="maxDotDiff">对比最大DotDiff</param>
        /// <param name="goodDirOrDis">符合要求的方式 true=方向符合 false=距离符合 </param>
        /// <param name="goodCompareOrLimit">通过了二次比较还是只通过了基本限制 true=通过比较 false=通过了基本限制Limit</param>
        /// <returns></returns>
        public static bool CompareByLimit(
            Vector3 targetOriginPos, Vector3 originPos, Vector3 originDir, 
            float radiusLimit, float angleLimit, float toleranceRange, ref float minDis, ref float maxDotDiff, out bool goodDirOrDis, out bool goodCompareOrLimit)
        {
            goodDirOrDis = goodCompareOrLimit = false;

            Vector3 targetDir = targetOriginPos - originPos;
            float targetDis = targetDir.magnitude;

            if (targetDis > radiusLimit) return false;//是否在限制距离内

            float diffVal = Vector3.Dot(targetDir.normalized, originDir);
            bool goodAngle = angleLimit >= 360f || Mathf.Acos(diffVal) * Mathf.Rad2Deg <= angleLimit / 2f;

            if (!goodAngle) return false;//是否在限制角度内

            //距离相差在容许范围内时 对比哪个目标更接近参照向量方向
            if (Mathf.Abs(targetDis - minDis) <= toleranceRange)
            {
                if (diffVal > maxDotDiff)
                {
                    maxDotDiff = diffVal;
                    goodDirOrDis = false;
                    goodCompareOrLimit = true;
                    return true;
                }
            }
            //对比距离
            else if (targetDis < minDis)
            {
                minDis = targetDis;
                maxDotDiff = diffVal;
                goodDirOrDis = true;
                goodCompareOrLimit = true;
                return true;
            }

            goodCompareOrLimit = false;
            return true;
        }

        /// <summary>
        /// 球形范围检测 返回探测到的目标类型对象数组
        /// </summary>
        /// <param name="origin">球形中心点</param>
        /// <param name="radius">球形半径</param>
        /// <param name="layerMask">检测层级layer</param>
        /// <param name="outComponentArray">检测到的目标对象数组</param>
        /// <returns>是否找到了目标</returns>
        public static bool OverlapSphereOutComponents(Vector3 origin, float radius, int layerMask, Collider[] ignoreColliders, out Component[] outComponentArray)
        {
            Collider[] colliders;
            
            //球形范围检测
            if (layerMask > 0)
                colliders = Physics.OverlapSphere(origin, radius, layerMask);
            else
                colliders = Physics.OverlapSphere(origin, radius);//未输入有效的layerMask

            //排除忽略的collider
            for (int i = 0; i < ignoreColliders.Length; i++)
            {
                Collider compareCo = ignoreColliders[i];
                for (int j = 0; j < colliders.Length; j++)
                {
                    if (colliders[j].Equals(compareCo))
                        colliders[j] = null;
                }
            }

            return GetComponentByColliderArray(colliders, out outComponentArray);
        }

        /// <summary>
        /// 从提供的Collider数组中 获得所有指定类型
        /// </summary>
        /// <typeparam name="T">获取目标类型</typeparam>
        /// <param name="colliders">用于筛选的Collider数组</param>
        /// <param name="outComponentArray">返回筛选出的目标类型数组</param>
        /// <returns>是否有获得目标类型</returns>
        static bool GetComponentByColliderArray<T>(Collider[] colliders, out T[] outComponentArray) where T : Component
        {
            outComponentArray = null;

            if (colliders.Length == 0) return false;

            //筛选出AActor对象
            List<T> components = new List<T>();
            for (int i = 0; i < colliders.Length; i++)
            {
                if (null == colliders[i]) continue;

                var co = colliders[i];
                T component = co.GetComponentInParent<T>();
                if (component)
                    components.Add(component);
            }

            //返回目标对象数组
            if (components.Count > 0)
            {
                outComponentArray = components.ToArray();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 默认的获取组件位置信息的方法
        /// 如果项目有对此进行优化 可以在Detection时传入优化的获取位置方法
        /// </summary>
        /// <param name="targetComponent"></param>
        /// <returns></returns>
        public static Vector3 GetTargetPosition(Component targetComponent)
        {
            var collider = targetComponent.gameObject.GetComponentInChildren<Collider>();
            if (collider)
            {
                return collider.bounds.center;
            }
            else
            {
                return targetComponent.transform.position;
            }
        }
        #endregion
    }
}