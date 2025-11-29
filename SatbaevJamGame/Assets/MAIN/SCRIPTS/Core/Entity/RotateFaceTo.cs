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

        // целевая ротация
        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);

        // скорость поворота (в градусах в секунду)
        float speed = 720f;

        owner.transform.rotation =
            Quaternion.RotateTowards(owner.transform.rotation, targetRot, speed * Time.deltaTime);
    }
}
