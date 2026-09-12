using System.Collections;
using UnityEngine;

public class BossAttackController : MonoBehaviour
{
    public enum BossAttackType
    {
        None,
        Melee,
        MediumLine,
        LongZones
    }

    [Header("Attack Components")]
    [SerializeField]
    private BossMeleeAttack meleeAttack;

    [SerializeField]
    private BossMediumLineAttack mediumLineAttack;

    [SerializeField]
    private BossLongZonesAttack longZonesAttack;

    [Header("Fight Start Settings")]
    [SerializeField] private float initialAttackDelay = 2f;

    [Header("Attack Unlock Settings")]
    [SerializeField]
    private bool requireMediumAttackBeforeLong = true;

    [Header("Attack Range Settings")]
    [SerializeField] private float meleeAttackRange = 3f;
    [SerializeField] private float mediumAttackRange = 7f;
    [SerializeField] private float longAttackRange = 20f;

    [Header("Attack Cooldown Settings")]
    [SerializeField] private float meleeAttackCooldown = 3f;
    [SerializeField] private float mediumAttackCooldown = 5f;
    [SerializeField] private float longAttackCooldown = 7f;

    [Header("Attack Windup Settings")]
    [SerializeField] private float meleeAttackWindup = 0.6f;
    [SerializeField] private float mediumAttackWindup = 1f;
    [SerializeField] private float longAttackWindup = 1.2f;

    [Header("Recovery Settings")]
    [SerializeField] private float recoveryDuration = 0.8f;

    [Header("Debug - Do Not Edit")]
    [SerializeField] private bool debugIsAttacking;
    [SerializeField] private float debugDistanceToPlayer;
    [SerializeField] private float debugInitialAttackDelay;
    [SerializeField] private bool debugLongAttackUnlocked;

    [SerializeField]
    private BossAttackType debugCurrentAttack;

    [SerializeField]
    private BossAttackType debugLastExecutedAttack;

    [SerializeField] private float debugMeleeCooldown;
    [SerializeField] private float debugMediumCooldown;
    [SerializeField] private float debugLongCooldown;

    [SerializeField]
    private float debugPostContinueMovementDelay;

    [SerializeField]
    private float debugPostContinueAttackDelay;

    private BossMovement bossMovement;
    private PlayerHealth playerHealth;

    private Coroutine attackCoroutine;

    private float initialAttackDelayTimer;

    private float meleeCooldownTimer;
    private float mediumCooldownTimer;
    private float longCooldownTimer;

    private float postContinueMovementDelayTimer;
    private float postContinueAttackDelayTimer;

    private bool hasExecutedMediumAttack;

    public bool IsAttacking { get; private set; }

    public BossAttackType CurrentAttack
    {
        get;
        private set;
    }

    public bool IsLongAttackUnlocked
    {
        get
        {
            return
                !requireMediumAttackBeforeLong ||
                hasExecutedMediumAttack;
        }
    }

    public bool IsPostContinueAttackDelayed =>
        postContinueAttackDelayTimer > 0f;

    private void Start()
    {
        bossMovement =
            GetComponent<BossMovement>();

        if (meleeAttack == null)
        {
            meleeAttack =
                GetComponent<BossMeleeAttack>();
        }

        if (mediumLineAttack == null)
        {
            mediumLineAttack =
                GetComponent<BossMediumLineAttack>();
        }

        if (longZonesAttack == null)
        {
            longZonesAttack =
                GetComponent<BossLongZonesAttack>();
        }

        playerHealth =
            FindAnyObjectByType<PlayerHealth>();

        initialAttackDelayTimer =
            Mathf.Max(
                0f,
                initialAttackDelay
            );

        hasExecutedMediumAttack = false;

        if (bossMovement == null)
        {
            Debug.LogWarning(
                "BossAttackController: " +
                "BossMovement was not found."
            );
        }

        if (meleeAttack == null)
        {
            Debug.LogWarning(
                "BossAttackController: " +
                "BossMeleeAttack was not found."
            );
        }

        if (mediumLineAttack == null)
        {
            Debug.LogWarning(
                "BossAttackController: " +
                "BossMediumLineAttack was not found."
            );
        }

        if (longZonesAttack == null)
        {
            Debug.LogWarning(
                "BossAttackController: " +
                "BossLongZonesAttack was not found."
            );
        }

        if (playerHealth == null)
        {
            Debug.LogWarning(
                "BossAttackController: " +
                "PlayerHealth was not found."
            );
        }
    }

    private void Update()
    {
        UpdateInitialAttackDelay();
        UpdatePostContinueDelays();
        UpdateCooldowns();

        if (bossMovement == null ||
            playerHealth == null)
        {
            return;
        }

        if (playerHealth.IsDead)
        {
            CancelCurrentAttack();
            return;
        }

        if (IsAttacking)
        {
            return;
        }

        if (initialAttackDelayTimer > 0f)
        {
            return;
        }

        if (postContinueAttackDelayTimer > 0f)
        {
            return;
        }

        BossAttackType selectedAttack =
            SelectAttack(
                bossMovement.DistanceToPlayer
            );

        if (selectedAttack !=
            BossAttackType.None)
        {
            attackCoroutine =
                StartCoroutine(
                    AttackSequence(
                        selectedAttack
                    )
                );
        }
    }

    private void LateUpdate()
    {
        UpdateDebugInfo();
    }

    public void StartPostContinueDelays(
        float movementDelay,
        float attackDelay
    )
    {
        CancelCurrentAttack();
        HideAllAttackIndicators();

        postContinueMovementDelayTimer =
            Mathf.Max(
                0f,
                movementDelay
            );

        postContinueAttackDelayTimer =
            Mathf.Max(
                0f,
                attackDelay
            );

        ApplyMovementLockState();

        Debug.Log(
            "BossAttackController: Movement blocked for " +
            postContinueMovementDelayTimer.ToString("F1") +
            " seconds. Attacks blocked for " +
            postContinueAttackDelayTimer.ToString("F1") +
            " seconds."
        );
    }

    private BossAttackType SelectAttack(
        float distanceToPlayer
    )
    {
        if (distanceToPlayer <=
            meleeAttackRange)
        {
            if (meleeCooldownTimer <= 0f)
            {
                return BossAttackType.Melee;
            }

            return BossAttackType.None;
        }

        if (distanceToPlayer <=
            mediumAttackRange)
        {
            if (mediumCooldownTimer <= 0f)
            {
                return BossAttackType.MediumLine;
            }

            return BossAttackType.None;
        }

        if (distanceToPlayer <=
            longAttackRange)
        {
            if (!IsLongAttackUnlocked)
            {
                return BossAttackType.None;
            }

            if (longCooldownTimer <= 0f)
            {
                return BossAttackType.LongZones;
            }

            return BossAttackType.None;
        }

        return BossAttackType.None;
    }

    private IEnumerator AttackSequence(
        BossAttackType attackType
    )
    {
        IsAttacking = true;
        CurrentAttack = attackType;

        ApplyMovementLockState();

        PrepareAttack(attackType);

        float windupDuration =
            GetAttackWindup(attackType);

        Debug.Log(
            "BossAttackController: Preparing " +
            attackType
        );

        yield return new WaitForSeconds(
            windupDuration
        );

        if (playerHealth == null ||
            playerHealth.IsDead)
        {
            FinishAttack();
            yield break;
        }

        ExecuteAttack(attackType);
        StartAttackCooldown(attackType);

        yield return new WaitForSeconds(
            recoveryDuration
        );

        FinishAttack();
    }

    private void PrepareAttack(
        BossAttackType attackType
    )
    {
        switch (attackType)
        {
            case BossAttackType.Melee:
                if (meleeAttack != null)
                {
                    meleeAttack.ShowIndicator();
                }
                break;

            case BossAttackType.MediumLine:
                if (mediumLineAttack != null)
                {
                    mediumLineAttack.PrepareAttack();
                }
                break;

            case BossAttackType.LongZones:
                if (longZonesAttack != null)
                {
                    longZonesAttack.PrepareAttack();
                }
                break;
        }
    }

    private void ExecuteAttack(
        BossAttackType attackType
    )
    {
        debugLastExecutedAttack =
            attackType;

        Debug.Log(
            "BossAttackController: Executed " +
            attackType
        );

        switch (attackType)
        {
            case BossAttackType.Melee:
                if (meleeAttack != null)
                {
                    meleeAttack.ExecuteAttack();
                }
                break;

            case BossAttackType.MediumLine:
                if (mediumLineAttack != null)
                {
                    mediumLineAttack.ExecuteAttack();
                    UnlockLongAttack();
                }
                break;

            case BossAttackType.LongZones:
                if (longZonesAttack != null)
                {
                    longZonesAttack.ExecuteAttack();
                }
                break;
        }
    }

    private void UnlockLongAttack()
    {
        if (hasExecutedMediumAttack)
        {
            return;
        }

        hasExecutedMediumAttack = true;

        if (requireMediumAttackBeforeLong)
        {
            Debug.Log(
                "BossAttackController: " +
                "LongZones attack is now unlocked."
            );
        }
    }

    private float GetAttackWindup(
        BossAttackType attackType
    )
    {
        switch (attackType)
        {
            case BossAttackType.Melee:
                return meleeAttackWindup;

            case BossAttackType.MediumLine:
                return mediumAttackWindup;

            case BossAttackType.LongZones:
                return longAttackWindup;

            default:
                return 0f;
        }
    }

    private void StartAttackCooldown(
        BossAttackType attackType
    )
    {
        switch (attackType)
        {
            case BossAttackType.Melee:
                meleeCooldownTimer =
                    meleeAttackCooldown;
                break;

            case BossAttackType.MediumLine:
                mediumCooldownTimer =
                    mediumAttackCooldown;
                break;

            case BossAttackType.LongZones:
                longCooldownTimer =
                    longAttackCooldown;
                break;
        }
    }

    private void UpdateInitialAttackDelay()
    {
        if (initialAttackDelayTimer <= 0f)
        {
            return;
        }

        initialAttackDelayTimer =
            Mathf.Max(
                0f,
                initialAttackDelayTimer -
                Time.deltaTime
            );
    }

    private void UpdatePostContinueDelays()
    {
        if (postContinueMovementDelayTimer > 0f)
        {
            postContinueMovementDelayTimer =
                Mathf.Max(
                    0f,
                    postContinueMovementDelayTimer -
                    Time.deltaTime
                );
        }

        if (postContinueAttackDelayTimer > 0f)
        {
            postContinueAttackDelayTimer =
                Mathf.Max(
                    0f,
                    postContinueAttackDelayTimer -
                    Time.deltaTime
                );
        }

        ApplyMovementLockState();
    }

    private void UpdateCooldowns()
    {
        meleeCooldownTimer =
            Mathf.Max(
                0f,
                meleeCooldownTimer -
                Time.deltaTime
            );

        mediumCooldownTimer =
            Mathf.Max(
                0f,
                mediumCooldownTimer -
                Time.deltaTime
            );

        longCooldownTimer =
            Mathf.Max(
                0f,
                longCooldownTimer -
                Time.deltaTime
            );
    }

    private void FinishAttack()
    {
        HideAllAttackIndicators();

        IsAttacking = false;
        CurrentAttack =
            BossAttackType.None;

        attackCoroutine = null;

        ApplyMovementLockState();
    }

    private void CancelCurrentAttack()
    {
        if (!IsAttacking)
        {
            return;
        }

        if (attackCoroutine != null)
        {
            StopCoroutine(
                attackCoroutine
            );
        }

        FinishAttack();
    }

    private void ApplyMovementLockState()
    {
        if (bossMovement == null)
        {
            return;
        }

        bool shouldLockMovement =
            IsAttacking ||
            postContinueMovementDelayTimer > 0f;

        bossMovement.SetMovementLocked(
            shouldLockMovement
        );
    }

    private void HideAllAttackIndicators()
    {
        if (meleeAttack != null)
        {
            meleeAttack.HideIndicator();
        }

        if (mediumLineAttack != null)
        {
            mediumLineAttack.HideIndicator();
        }

        if (longZonesAttack != null)
        {
            longZonesAttack.HideIndicators();
        }
    }

    private void UpdateDebugInfo()
    {
        debugIsAttacking =
            IsAttacking;

        debugCurrentAttack =
            CurrentAttack;

        debugInitialAttackDelay =
            initialAttackDelayTimer;

        debugLongAttackUnlocked =
            IsLongAttackUnlocked;

        debugMeleeCooldown =
            meleeCooldownTimer;

        debugMediumCooldown =
            mediumCooldownTimer;

        debugLongCooldown =
            longCooldownTimer;

        debugPostContinueMovementDelay =
            postContinueMovementDelayTimer;

        debugPostContinueAttackDelay =
            postContinueAttackDelayTimer;

        if (bossMovement != null)
        {
            debugDistanceToPlayer =
                bossMovement.DistanceToPlayer;
        }
    }

    private void OnDisable()
    {
        HideAllAttackIndicators();

        if (bossMovement != null)
        {
            bossMovement.SetMovementLocked(false);
        }
    }
}