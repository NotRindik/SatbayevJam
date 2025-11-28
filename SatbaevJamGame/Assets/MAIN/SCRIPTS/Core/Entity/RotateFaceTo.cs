using DG.Tweening;
using Systems;
using UnityEngine;

public class RotateFaceTo : BaseSystem
{
    private MoveComponent moveComponent;
    private Vector3 temp;
    private Tween rotateTween;
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
        if (temp != dir)
        {
            rotateTween?.Kill();
            rotateTween = owner.transform.DORotateQuaternion(Quaternion.LookRotation(dir, Vector3.up), 0.1f);
            temp = dir;
        }
    }
}
