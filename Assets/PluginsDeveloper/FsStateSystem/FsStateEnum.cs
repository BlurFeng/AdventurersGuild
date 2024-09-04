namespace FsStateSystem
{
    public enum State
    {
        //修改状态表表格时值含义
        //status with status 状态添加时和其他状态处理关系
        // 0 不加入 新状态无法添加
        // 1 覆盖 新状态可添加且移除其他需要被覆盖的状态
        // 2 叠加 新状态可添加 且不会移除可叠加的状态


        None = 0,


        //100段 基础状态 不共存
        Normal = 100,//正常
        Dead,//死亡

        //200段 行为状态
        Idle = 200,//站立
        Walk,//走 
        Run,//跑
        Jump,//跳跃

        //300段  描述状态
        Grounded = 300,//着地

        //400段 其他状态


        MAX_VALUE,//最大状态数
    }
}

