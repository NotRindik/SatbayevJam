using Systems;

public class PlayerEntity : SavingEntity
{
    private AnimationComponent animationComponent;
    public MeshTrail_Script meshTrail;
    private IInputProvider input;
    public override void Start()
    {
        base.Start();
        input = GetControllerSystem<IInputProvider>();
        animationComponent = GetControllerComponent<AnimationComponent>();

        input.GetState().Move.started += c =>
        {
            animationComponent.CrossFade("Walk", 0.3f);
        };

        input.GetState().Move.canceled += c =>
        {
            animationComponent.CrossFade("Idle", 0.2f);
        };

        input.GetState().Dash.started += c =>
        {
            meshTrail.Activate(0.2f);
        };

        animationComponent.CrossFade("Idle", 0);
    }
}
