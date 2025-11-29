using UnityEngine;

public class Pistol : MonoBehaviour
{
    [SerializeField] Transform shotPos;
    [SerializeField] PistolComponent ps;

    float lastShootTime;
    public void Shoot()
    {
        // ��������
        if (Time.time < lastShootTime + ps.delay)
            return;

        lastShootTime = Time.time;

        // �������� ������� ������� � ����
        Vector3 mouse = InputManager.inputActions.UI.Point.ReadValue<Vector2>();
        mouse.z = 10f; // ��������� �� ������
        Vector3 worldMouse = Camera.main.ScreenToWorldPoint(mouse);

        // �����������
        Vector3 dir = (worldMouse - shotPos.position).normalized;

        // ������ ����
        GameObject b = GameObject.Instantiate(ps.BulletPrefab, shotPos.position, Quaternion.LookRotation(dir));

        // ���� � ���� ���� rigidbody
        if (b.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = dir * 20f; // �������� ����
        }
    }
}

public struct PistolComponent
{
    public float delay;
    public  GameObject BulletPrefab;
}
