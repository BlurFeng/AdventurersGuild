using System;

namespace FsStoryIncident
{
    public struct StoryTypeParamKey
    {
        /// <summary>
        /// 类型
        /// </summary>
        public Type type;

        /// <summary>
        /// 参数
        /// </summary>
        public string param;

        public StoryTypeParamKey(Type type, string param)
        {
            this.type = type;
            this.param = param;
        }

        public static bool operator ==(StoryTypeParamKey a, StoryTypeParamKey b)
        {
            return !(a != b);
        }

        public static bool operator !=(StoryTypeParamKey a, StoryTypeParamKey b)
        {
            return a.type != b.type || !a.param.Equals(b.param);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is not StoryTypeParamKey) return false;

            return this == (StoryTypeParamKey)obj;
        }

        public override int GetHashCode()
        {
            return type.GetHashCode() * param.GetHashCode();
        }

        public override string ToString()
        {
            return $"{(type != null ? type.ToString() : "Null Type")} {(param != null ? param : "Null param")}";
        }
    }
}