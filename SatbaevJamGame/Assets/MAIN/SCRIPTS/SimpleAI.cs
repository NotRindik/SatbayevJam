using Systems;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleShooterAI : SavingEntity, ReInitAfterRePlay
{
    public float moveSpeed = 3f;         // скорость движения
    public float rotationSpeed = 10f;    // скорость поворота
    public float attackDistance = 5f;    // дистанция атаки
    public float attackCooldown = 1f;    // перезарядка
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

        // Поворот к игроку
        if (toPlayer != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }

        // Если далеко — идём
        if (distance > attackDistance)
        {
            MoveTowardsPlayer(toPlayer.normalized);
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0); // остановка
            TryShoot();
        }
    }

    private void MoveTowardsPlayer(Vector3 direction)
    {
        Vector3 velocity = direction * moveSpeed;
        velocity.y = rb.velocity.y; // чтобы гравитация работала нормально
        rb.velocity = velocity;
    }

    private void TryShoot()
    {
        if (cooldownTimer > 0f)
            return;

        cooldownTimer = attackCooldown;

        // TODO: SHOOT HERE
        // Здесь ты вызываешь свою стрельбу: Instantiate(пуля), Raycast, урон — что угодно.
        Debug.Log("AI SHOOTS!");
    }
}
