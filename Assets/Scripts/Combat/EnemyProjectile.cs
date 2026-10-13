using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Technical Settings")]
    [SerializeField] private float lifeTime = 5f;

    private Rigidbody projectileRigidbody;

    private Vector3 moveDirection;
    private int damage;
    private float moveSpeed;

    private bool isInitialized;
    private bool hasHit;

    private void Awake()
    {
        projectileRigidbody =
            GetComponent<Rigidbody>();

        Collider projectileCollider =
            GetComponent<Collider>();

        projectileRigidbody.useGravity = false;
        projectileRigidbody.isKinematic = true;

        projectileRigidbody.collisionDetectionMode =
            CollisionDetectionMode.ContinuousSpeculative;

        projectileCollider.isTrigger = true;
    }

    private void Start()
    {
        Destroy(
            gameObject,
            lifeTime
        );
    }

    private void FixedUpdate()
    {
        if (!isInitialized ||
            hasHit)
        {
            return;
        }

        Vector3 movement =
            moveDirection *
            moveSpeed *
            Time.fixedDeltaTime;

        projectileRigidbody.MovePosition(
            projectileRigidbody.position +
            movement
        );
    }

    public void Initialize(
        Vector3 travelDirection,
        int projectileDamage,
        float projectileMoveSpeed
    )
    {
        if (travelDirection.sqrMagnitude <
            0.001f)
        {
            Debug.LogWarning(
                "EnemyProjectile: Travel direction is too small."
            );

            Destroy(gameObject);

            return;
        }

        if (projectileMoveSpeed <= 0f)
        {
            Debug.LogWarning(
                "EnemyProjectile: Projectile move speed must be positive."
            );

            Destroy(gameObject);

            return;
        }

        moveDirection =
            travelDirection.normalized;

        moveSpeed =
            Mathf.Max(
                0.01f,
                projectileMoveSpeed
            );

        damage =
            Mathf.Max(0, projectileDamage);

        isInitialized = true;

        transform.rotation =
            Quaternion.LookRotation(
                moveDirection
            );
    }

    private void OnTriggerEnter(
        Collider other
    )
    {
        if (!isInitialized ||
            hasHit)
        {
            return;
        }

        EnemyHealth enemyHealth =
            other.GetComponentInParent<EnemyHealth>();

        if (enemyHealth != null)
        {
            return;
        }

        hasHit = true;

        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            if (!playerHealth.IsDead)
            {
                playerHealth.TakeDamage(
                    damage
                );
            }

            Destroy(gameObject);

            return;
        }

        CoreTowerHealth coreTower =
            other.GetComponentInParent<CoreTowerHealth>();

        if (coreTower != null)
        {
            coreTower.TakeDamage(
                damage
            );

            Destroy(gameObject);

            return;
        }

        Destroy(gameObject);
    }
}