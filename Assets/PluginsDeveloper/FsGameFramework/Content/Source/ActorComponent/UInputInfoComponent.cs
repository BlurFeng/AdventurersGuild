using FsGameFramework;
using FsGameFramework.InputSystem;

public sealed class UInputInfoComponent : UActorComponent
{
    public DirectionInfo MoveDir { get; private set; }

    public DirectionInfo AimDir { get; private set; }

    public ButtonInfo MainBtn { get; private set; }

    public ButtonInfo JumpBtn { get; private set; }

    public ButtonInfo MouseLeftBtn { get; private set; }

    public ButtonInfo MouseRightBtn { get; private set; }

    public ButtonInfo MouseMiddleBtn { get; private set; }

    public UInputInfoComponent(AActor actor) : base(actor)
    {
        MoveDir = new DirectionInfo();
        AimDir = new DirectionInfo();

        MainBtn = new ButtonInfo();
        JumpBtn = new ButtonInfo();

        MouseLeftBtn = new ButtonInfo();
        MouseRightBtn = new ButtonInfo();
        MouseMiddleBtn = new ButtonInfo();
    }
}