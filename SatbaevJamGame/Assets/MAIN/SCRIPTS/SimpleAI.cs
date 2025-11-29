using Systems;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleShooterAI : SavingEntity, ReInitAfterRePlay
{
    public float moveSpeed = 3f;         // �������� ��������
    public float rotationSpeed = 10f;    // �������� ��������
    public float attackDistance = 5f;    // ��������� �����
    public float attackCooldown = 1f;    // �����������
    private HealthComponent healthComponent;

    private Transform player;
    private Rigidbody rb;
    private float cooldownTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        healthComponent = GetControllerComponent<HealthComponent>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }
    public void ReInit()
    {
        EntitySetup();
        healthComponent = GetControllerComponent<HealthComponent>();
    }
    void FixedUpdate()
    {
        if (!player)
            return;

        cooldownTimer -= Time.fixedDeltaTime;

        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;

        // ������� � ������
        if (toPlayer != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }

        // ���� ������ � ���
        if (distance > attackDistance)
        {
            MoveTowardsPlayer(toPlayer.normalized);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); // ���������
            TryShoot();
        }
    }

    private void MoveTowardsPlayer(Vector3 direction)
    {
        Vector3 velocity = direction * moveSpeed;
        velocity.y = rb.linearVelocity.y; // ����� ���������� �������� ���������
        rb.linearVelocity = velocity;
    }

    private void TryShoot()
    {
        if (cooldownTimer > 0f)
            return;

        cooldownTimer = attackCooldown;

        // TODO: SHOOT HERE
        // ����� �� ��������� ���� ��������: Instantiate(����), Raycast, ���� � ��� ������.
        Debug.Log("AI SHOOTS!");
    }
}
