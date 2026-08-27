using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float hitDistance = 0.2f;

    [Header("Hit Effect")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float hitEffectOffset = 0.5f;

    private EnemyHealth target;
    private int damage;

    public void Initialize(EnemyHealth newTarget, int newDamage)
    {
        target = newTarget;
        damage = newDamage;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = target.transform.position;

        Vector3 direction =
            (targetPosition - transform.position).normalized;

        transform.position +=
            direction * moveSpeed * Time.deltaTime;

        float distanceToTarget =
            Vector3.Distance(
                transform.position,
                targetPosition
            );

        if (distanceToTarget <= hitDistance)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        if (target == null)
            return;

        Vector3 directionFromEnemy =
            (transform.position - target.transform.position).normalized;

        Vector3 hitEffectPosition =
            target.transform.position +
            directionFromEnemy * hitEffectOffset;

        target.TakeDamage(damage);

        if (hitEffectPrefab != null)
        {
            Instantiate(
                hitEffectPrefab,
                hitEffectPosition,
                Quaternion.identity
            );
        }

        Destroy(gameObject);
    }
}