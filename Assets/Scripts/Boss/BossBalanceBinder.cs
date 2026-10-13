using UnityEngine;

[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
public class BossBalanceBinder : MonoBehaviour
{
    [Header("Master Balance Source")]
    [SerializeField]
    private GameBalanceConfig balanceConfig;

    private void Awake()
    {
        if (balanceConfig == null)
        {
            Debug.LogError(
                "BossBalanceBinder: GameBalanceConfig is not assigned.",
                this
            );

            return;
        }

        BossCoreBalanceValues coreBalance =
            balanceConfig.FirstBossCore;

        BossAttackPayloadBalanceValues attackBalance =
            balanceConfig.FirstBossAttacks;

        if (coreBalance == null)
        {
            Debug.LogError(
                "BossBalanceBinder: First Boss core balance data is missing.",
                this
            );

            return;
        }

        if (attackBalance == null ||
            attackBalance.Melee == null ||
            attackBalance.MediumLine == null ||
            attackBalance.LongZones == null)
        {
            Debug.LogError(
                "BossBalanceBinder: First Boss attack payload " +
                "balance data is missing.",
                this
            );

            return;
        }

        EnemyHealth enemyHealth =
            GetComponent<EnemyHealth>();

        BossMovement bossMovement =
            GetComponent<BossMovement>();

        BossAttackController bossAttackController =
            GetComponent<BossAttackController>();

        BossMeleeAttack meleeAttack =
            GetComponent<BossMeleeAttack>();

        BossMediumLineAttack mediumLineAttack =
            GetComponent<BossMediumLineAttack>();

        BossLongZonesAttack longZonesAttack =
            GetComponent<BossLongZonesAttack>();

        bool healthConfigured =
            enemyHealth != null &&
            enemyHealth.ConfigureBaseMaxHealth(
                coreBalance.BaseMaxHealth
            );

        bool movementConfigured =
            bossMovement != null &&
            bossMovement.ConfigureBalance(
                coreBalance
            );

        bool controllerConfigured =
            bossAttackController != null &&
            bossAttackController.ConfigureBalance(
                coreBalance
            );

        bool meleeConfigured =
            meleeAttack != null &&
            meleeAttack.ConfigureBalance(
                attackBalance.Melee
            );

        bool mediumConfigured =
            mediumLineAttack != null &&
            mediumLineAttack.ConfigureBalance(
                attackBalance.MediumLine
            );

        bool longConfigured =
            longZonesAttack != null &&
            longZonesAttack.ConfigureBalance(
                attackBalance.LongZones
            );

        if (!healthConfigured ||
            !movementConfigured ||
            !controllerConfigured ||
            !meleeConfigured ||
            !mediumConfigured ||
            !longConfigured)
        {
            Debug.LogError(
                "BossBalanceBinder: First Boss balance could not " +
                "be applied completely. Check EnemyHealth, " +
                "BossMovement, BossAttackController and all " +
                "three Boss attack components.",
                this
            );

            return;
        }

        Debug.Log(
            "BossBalanceBinder: First Boss balance applied. " +
            $"HP = {coreBalance.BaseMaxHealth}. " +
            $"Move Speed = {coreBalance.MoveSpeed:F2}. " +
            $"Stop Distance = {coreBalance.StoppingDistance:F2}. " +
            $"Initial Attack Delay = {coreBalance.InitialAttackDelay:F2}. " +
            $"Ranges = {coreBalance.MeleeAttackRange:F2} / " +
            $"{coreBalance.MediumAttackRange:F2} / " +
            $"{coreBalance.LongAttackRange:F2}. " +
            $"Cooldowns = {coreBalance.MeleeAttackCooldown:F2} / " +
            $"{coreBalance.MediumAttackCooldown:F2} / " +
            $"{coreBalance.LongAttackCooldown:F2}. " +
            $"Windups = {coreBalance.MeleeAttackWindup:F2} / " +
            $"{coreBalance.MediumAttackWindup:F2} / " +
            $"{coreBalance.LongAttackWindup:F2}. " +
            $"Recovery = {coreBalance.RecoveryDuration:F2}. " +
            $"Medium Before Long = " +
            $"{coreBalance.RequireMediumAttackBeforeLong}. " +
            $"Melee Payload = P{attackBalance.Melee.PlayerDamage} / " +
            $"C{attackBalance.Melee.CoreTowerDamage} / " +
            $"R{attackBalance.Melee.AttackRadius:F2}. " +
            $"Medium Payload = P{attackBalance.MediumLine.PlayerDamage} / " +
            $"C{attackBalance.MediumLine.CoreTowerDamage} / " +
            $"L{attackBalance.MediumLine.LineLength:F2} / " +
            $"W{attackBalance.MediumLine.LineWidth:F2} / " +
            $"H{attackBalance.MediumLine.AttackHeight:F2}. " +
            $"Long Payload = P{attackBalance.LongZones.PlayerDamage} / " +
            $"R{attackBalance.LongZones.ZoneRadius:F2} / " +
            $"Placement{attackBalance.LongZones.RandomPlacementRadius:F2} / " +
            $"Spacing{attackBalance.LongZones.MinimumZoneSpacing:F2}.",
            this
        );
    }
}
