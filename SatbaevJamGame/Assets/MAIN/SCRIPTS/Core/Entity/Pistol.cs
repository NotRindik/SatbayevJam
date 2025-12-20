using UnityEngine;

public class Pistol : MonoBehaviour
{
    [SerializeField] Transform shotPos;
    [SerializeField] PistolComponent ps;
    [SerializeField] public Vector3 targetPoint;
    float lastShootTime;
    public bool isPlayer;
    public void Shoot()
    {
        if (!ps.bc.unscaleUpdate)
        {
            if (Time.time < lastShootTime + ps.delay)
                return;

            lastShootTime = Time.time;
        }
        else
        {
            if (Time.unscaledTime < lastShootTime + ps.delay)
                return;

            lastShootTime = Time.unscaledTime;
        }

        Vector2 screenPoint = InputManager.inputActions.Player.Point.ReadValue<Vector2>();

        Ray ray = Camera.main.ScreenPointToRay(screenPoint);

        // Если попали во что-то — стреляем туда
        Plane plane = new Plane(Vector3.up, shotPos.position.y);

        // 2. Пересечение луча с плоскостью
        if (plane.Raycast(ray, out float enter))
        {
            targetPoint = ray.GetPoint(enter);
        }
        else
        {
            // fallback, очень редко нужен
            targetPoint = shotPos.position + transform.forward * 10f;
        }
        targetPoint = new Vector3(targetPoint.x, shotPos.position.y, targetPoint.z);
        Vector3 dir = (targetPoint - shotPos.position).normalized;
        if(isPlayer == false)
        {
            dir = (shotPos.position - transform.position ).normalized;
            dir.y = 0;
        }
        var inst = Instantiate(ps.BulletPrefab, shotPos.position, Quaternion.LookRotation(dir));
        var temp = inst.bc;
        temp.DamageLayer = ps.bc.DamageLayer;
        temp.DestroyLayer = ps.bc.DestroyLayer;
        temp.unscaleUpdate = ps.bc.unscaleUpdate;
        inst.bc = temp;
    }

}
[System.Serializable]
public struct PistolComponent
{
    public float delay;
    public  Bullet BulletPrefab;
    public BulletComponent bc;
}
