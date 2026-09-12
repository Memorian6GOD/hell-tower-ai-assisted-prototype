using UnityEngine;

public class EnemyAggroHealthBar : MonoBehaviour
{
    private EnemyMovement enemyMovement;
    private RangedEnemyMovement rangedEnemyMovement;
    private EnemyHealth enemyHealth;

    private bool wasTargetingPlayer;

    private void Awake()
    {
        enemyMovement =
            GetComponent<EnemyMovement>();

        rangedEnemyMovement =
            GetComponent<RangedEnemyMovement>();

        enemyHealth =
            GetComponent<EnemyHealth>();

        if (enemyMovement == null &&
            rangedEnemyMovement == null)
        {
            Debug.LogWarning(
                "EnemyAggroHealthBar: Enemy movement component was not found."
            );
        }

        if (enemyMovement != null &&
            rangedEnemyMovement != null)
        {
            Debug.LogWarning(
                "EnemyAggroHealthBar: Two movement components were found."
            );
        }

        if (enemyHealth == null)
        {
            Debug.LogWarning(
                "EnemyAggroHealthBar: EnemyHealth was not found."
            );
        }

        if (enemyHealth != null)
        {
            enemyHealth.HideHealthBar();
        }
    }

    private void Update()
    {
        if (enemyHealth == null)
        {
            return;
        }

        if (enemyMovement == null &&
            rangedEnemyMovement == null)
        {
            return;
        }

        bool isTargetingPlayer =
            GetIsTargetingPlayer();

        if (isTargetingPlayer == wasTargetingPlayer)
        {
            return;
        }

        wasTargetingPlayer =
            isTargetingPlayer;

        if (isTargetingPlayer)
        {
            enemyHealth.ShowHealthBar();
        }
        else
        {
            enemyHealth.HideHealthBar();
        }
    }

    private bool GetIsTargetingPlayer()
    {
        if (enemyMovement != null)
        {
            return enemyMovement.IsTargetingPlayer;
        }

        if (rangedEnemyMovement != null)
        {
            return rangedEnemyMovement.IsTargetingPlayer;
        }

        return false;
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.HideHealthBar();
        }
    }
}