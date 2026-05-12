namespace Tuiget.Framework;

public abstract class ScreenComponent : IWidget, IFocusable
{
    private readonly MainScreen _parent;

    public bool IsFocused { get; set; }

    public ScreenComponent(MainScreen parent)
    {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
    }

    protected void Focus()
    {
        _parent.FocusComponent(this);
    }

    public virtual void Update(FrameInfo frame, IRenderBounds bounds)
    {
    }

    public virtual void OnEvent(ApplicationContext context, ApplicationMessage message)
    {
    }

    public abstract void Render(RenderContext context);
}