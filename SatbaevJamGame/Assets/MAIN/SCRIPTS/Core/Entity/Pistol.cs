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
        if (Time.time < lastShootTime + ps.delay)
            return;

        lastShootTime = Time.time;

        Vector2 screenPoint = InputManager.inputActions.UI.Point.ReadValue<Vector2>();

        Ray ray = Camera.main.ScreenPointToRay(screenPoint);

        // Если попали во что-то — стреляем туда
        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
            targetPoint = hit.point;
        }
        else
        {
            // Если нет коллизии — берём точку далеко по лучу
            targetPoint = ray.GetPoint(100f);
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
