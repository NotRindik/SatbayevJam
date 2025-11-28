using Systems;
using UnityEngine;

public class RotateFaceTo : BaseSystem
{
    private MoveComponent moveComponent;
    public override void Initialize(Entity owner)
    {
        base.Initialize(owner);
        moveComponent = owner.GetControllerComponent<MoveComponent>();

        owner.OnUpdate += Update;
    }

    public override void OnUpdate()
    {
        Vector3 dir = moveComponent.dir;

        if (dir.sqrMagnitude < 0.0001f)
            return;

        owner.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }
}
