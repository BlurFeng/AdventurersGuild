using System;

namespace FsStoryIncident
{
    public interface IConfigCommonData
    {
        /// <summary>
        /// Guid
        /// 全局唯一Id
        /// </summary>
        /// <returns></returns>
        public Guid Guid();

        /// <summary>
        /// 包含Tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool ContainsTag(string tag);

        /// <summary>
        /// 名称
        /// </summary>
        /// <returns></returns>
        public string Name();

#if UNITY_EDITOR
        public string NameEditorShow();
#endif
    }
}