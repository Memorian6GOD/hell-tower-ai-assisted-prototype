using System;
using UnityEngine;

[RequireComponent(typeof(DefenseTowerRunStats))]
public class DefenseTowerHealth : MonoBehaviour
{
    private CombatStats combatStats;
    private DefenseTowerHealthBar healthBar;
    private int maxHealth;
    private int currentHealth;
    private bool isDestroyed;
    private bool isInitialized;
    private GameObject ruinsMarker;
    private Renderer[] originalRenderers;
    private bool[] originalRendererStates;
    private Collider[] originalColliders;
    private bool[] originalColliderStates;
    private UnityEngine.AI.NavMeshObstacle originalObstacle;
    private bool originalObstacleEnabled;
    private bool originalObstacleCarving;
    private DefenseTowerAttack originalAttack;
    private bool originalAttackEnabled;

    public event Action<DefenseTowerHealth> Destroyed;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDestroyed => isDestroyed;

    private void Start()
    {
        combatStats = FindAnyObjectByType<CombatStats>();
        if (combatStats == null)
        {
            Debug.LogError("DefenseTowerHealth: CombatStats was not found. Check GameProgression in the scene.", this);
            return;
        }

        int configuredMaxHealth = combatStats.DefenseTowerMaxHealth;
        if (configuredMaxHealth <= 0)
        {
            Debug.LogError("DefenseTowerHealth: Check GameBalanceConfig on CombatStats.", this);
            return;
        }

        maxHealth = configuredMaxHealth;
        currentHealth = maxHealth;
        isDestroyed = false;
        isInitialized = true;

        // Kept for compatibility with existing CombatStats debug tools.
        // RunUpgradeManager now sends actual card HP bonuses directly to each living tower.
        combatStats.DefenseTowerRunHealthBonusAdded += HandleRunHealthBonusAdded;

        healthBar = GetComponentInChildren<DefenseTowerHealthBar>(true);
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
            healthBar.Show();
        }

        Debug.Log($"DefenseTowerHealth: {gameObject.name} initial health = {currentHealth}/{maxHealth}.", this);
    }

    public void TakeDamage(int damageAmount)
    {
        if (!isInitialized || isDestroyed || damageAmount <= 0) return;
        currentHealth = Mathf.Max(0, currentHealth - damageAmount);
        if (healthBar != null) healthBar.SetHealth(currentHealth, maxHealth);
        Debug.Log($"{gameObject.name} health: {currentHealth}/{maxHealth}", this);
        if (currentHealth == 0) DestroyTower();
    }

    public int RestoreHealthPercent(float healthPercent)
    {
        if (!Application.isPlaying || !isInitialized || isDestroyed ||
            healthPercent <= 0f || float.IsNaN(healthPercent) || float.IsInfinity(healthPercent))
            return 0;

        int missingHealth = maxHealth - currentHealth;
        if (missingHealth <= 0) return 0;

        double percent = Math.Min(100d, (double)healthPercent);
        int requestedHealth = (int)Math.Min((double)int.MaxValue,
            Math.Ceiling(maxHealth * percent / 100d));
        int restoredHealth = Math.Min(missingHealth, Math.Max(1, requestedHealth));
        currentHealth += restoredHealth;
        if (healthBar != null) healthBar.SetHealth(currentHealth, maxHealth);
        Debug.Log($"DefenseTowerHealth: {gameObject.name} restored {restoredHealth} HP. HP: {currentHealth}/{maxHealth}.", this);
        return restoredHealth;
    }

    // New: applies a run card to this instance, not a global shared stat.
    public bool TryAddIndividualRunMaxHealth(int amount)
    {
        if (!Application.isPlaying || !isInitialized || isDestroyed ||
            currentHealth <= 0 || amount <= 0) return false;
        if (amount > int.MaxValue - maxHealth || amount > int.MaxValue - currentHealth)
        {
            Debug.LogError("DefenseTowerHealth: HP overflow while applying run card.", this);
            return false;
        }

        maxHealth += amount;
        currentHealth += amount;
        if (healthBar != null) healthBar.SetHealth(currentHealth, maxHealth);
        Debug.Log($"DefenseTowerHealth: {gameObject.name} individual run HP upgrade +{amount}. HP: {currentHealth}/{maxHealth}.", this);
        return true;
    }

    private void HandleRunHealthBonusAdded(int amount)
    {
        TryAddIndividualRunMaxHealth(amount);
    }

    private void UnsubscribeFromHealthBonuses()
    {
        if (combatStats != null)
            combatStats.DefenseTowerRunHealthBonusAdded -= HandleRunHealthBonusAdded;
    }

    private void OnDestroy()
    {
        UnsubscribeFromHealthBonuses();
    }

    // Ruins keep the original tower object alive so its position, slot, and
    // DefenseTowerRunStats survive destruction. Restoration is implemented later.
    private void DestroyTower()
    {
        if (isDestroyed) return;

        isDestroyed = true;
        UnsubscribeFromHealthBonuses();
        if (healthBar != null) healthBar.Hide();

        // Save the exact pre-destruction states. Some prefab children can be
        // intentionally disabled, so blindly enabling everything is unsafe.
        originalRenderers = GetComponentsInChildren<Renderer>(true);
        originalRendererStates = new bool[originalRenderers.Length];
        for (int i = 0; i < originalRenderers.Length; i++)
        {
            originalRendererStates[i] = originalRenderers[i] != null && originalRenderers[i].enabled;
            if (originalRenderers[i] != null) originalRenderers[i].enabled = false;
        }

        originalAttack = GetComponent<DefenseTowerAttack>();
        if (originalAttack != null)
        {
            originalAttackEnabled = originalAttack.enabled;
            originalAttack.enabled = false;
        }

        // A ruin is no longer a siege target, but it remains a physical obstacle.
        // Carving prevents NavMesh agents from planning a route through the ruins.
        originalObstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();
        if (originalObstacle != null)
        {
            originalObstacleEnabled = originalObstacle.enabled;
            originalObstacleCarving = originalObstacle.carving;
            originalObstacle.carving = true;
            originalObstacle.enabled = true;
        }

        // Hide the old hittable geometry; the new ruins marker has its own
        // collider, on Default, and remains an impassable world obstacle.
        originalColliders = GetComponentsInChildren<Collider>(true);
        originalColliderStates = new bool[originalColliders.Length];
        for (int i = 0; i < originalColliders.Length; i++)
        {
            originalColliderStates[i] = originalColliders[i] != null && originalColliders[i].enabled;
            if (originalColliders[i] != null) originalColliders[i].enabled = false;
        }

        CreateTemporaryRuinsMarker();

        // Do not notify DefenseTowerPlacement.Destroyed: it currently removes
        // the tower from its occupied-slot list. The ruins must reserve this slot.
        // Do not call Destroy(gameObject): that would erase the individual bonuses.
        DefenseTowerRunStats runStats = GetComponent<DefenseTowerRunStats>();
        Debug.Log(
            $"DefenseTowerHealth: {gameObject.name} became ruins at {transform.position}. " +
            $"Stored HP max = {maxHealth}, " +
            $"damage bonus = +{(runStats != null ? runStats.DamageBonusPercent : 0f):F1}%, " +
            $"attack speed bonus = +{(runStats != null ? runStats.AttackSpeedBonusPercent : 0f):F1}%. " +
            "Original tower and occupied slot are preserved.", this);
    }

    // Restores this exact instance: its position, individual card bonuses and
    // tower placement slot remain unchanged throughout the entire operation.
    public bool TryRestoreFromRuins()
    {
        if (!Application.isPlaying || !isInitialized || !isDestroyed || ruinsMarker == null)
            return false;

        // Do not rebuild a solid tower around the player. The player can
        // request restoration again once they have moved away from the ruins.
        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        if (player != null)
        {
            Vector3 delta = player.transform.position - transform.position;
            delta.y = 0f;
            if (delta.sqrMagnitude < 2.25f * 2.25f)
            {
                Debug.LogWarning("DefenseTowerHealth: Move the hero away from the ruins before restoration.", this);
                return false;
            }
        }

        // Destroy the marker's physics immediately so it cannot trap the hero
        // between two collider sets during the same frame.
        Collider markerCollider = ruinsMarker.GetComponent<Collider>();
        if (markerCollider != null) markerCollider.enabled = false;
        ruinsMarker.SetActive(false);
        Destroy(ruinsMarker);
        ruinsMarker = null;

        for (int i = 0; originalRenderers != null && i < originalRenderers.Length; i++)
        {
            if (originalRenderers[i] != null) originalRenderers[i].enabled = originalRendererStates[i];
        }
        for (int i = 0; originalColliders != null && i < originalColliders.Length; i++)
        {
            if (originalColliders[i] != null) originalColliders[i].enabled = originalColliderStates[i];
        }
        if (originalObstacle != null)
        {
            originalObstacle.carving = originalObstacleCarving;
            originalObstacle.enabled = originalObstacleEnabled;
        }
        if (originalAttack != null) originalAttack.enabled = originalAttackEnabled;

        isDestroyed = false;
        currentHealth = maxHealth;
        if (combatStats != null)
        {
            // A tower that was ruined did not collect new tower cards.
            // This subscription only supports the existing debug balance tools.
            combatStats.DefenseTowerRunHealthBonusAdded -= HandleRunHealthBonusAdded;
            combatStats.DefenseTowerRunHealthBonusAdded += HandleRunHealthBonusAdded;
        }
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
            healthBar.Show();
        }

        DefenseTowerRunStats stats = GetComponent<DefenseTowerRunStats>();
        Debug.Log($"DefenseTowerHealth: {name} restored from ruins at {transform.position}. " +
                  $"HP = {currentHealth}/{maxHealth}. " +
                  $"Damage bonus = +{(stats != null ? stats.DamageBonusPercent : 0f):F1}%, " +
                  $"attack speed bonus = +{(stats != null ? stats.AttackSpeedBonusPercent : 0f):F1}%.", this);
        return true;
    }

    private void CreateTemporaryRuinsMarker()
    {
        if (ruinsMarker != null) return;

        ruinsMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ruinsMarker.name = "Ruins_Prototype_ReplaceWithArtLater";
        // The DefenseTower layer may ignore Player collisions in the Physics matrix.
        // Ruins are scenery, not a living DefenseTower: use Default for physical blocking.
        ruinsMarker.layer = 0; // Default
        ruinsMarker.transform.SetParent(transform, false);
        ruinsMarker.transform.localPosition = new Vector3(0f, 0.14f, 0f);
        ruinsMarker.transform.localRotation = Quaternion.identity;
        ruinsMarker.transform.localScale = new Vector3(1.5f, 0.28f, 1.5f);

        // The visible slab is low, but its collision volume represents the
        // future pile of rubble and prevents characters from stepping over it.
        BoxCollider markerCollider = ruinsMarker.GetComponent<BoxCollider>();
        if (markerCollider != null)
        {
            markerCollider.isTrigger = false;
            markerCollider.center = new Vector3(0f, 2.57f, 0f);
            markerCollider.size = new Vector3(1f, 5.14f, 1f);
            markerCollider.enabled = true;
        }

        Renderer markerRenderer = ruinsMarker.GetComponent<Renderer>();
        if (markerRenderer != null)
        {
            // A plain marker is intentional. We'll replace it with ruined
            // tower artwork during the visual-production stage.
            markerRenderer.material.color = new Color(0.32f, 0.20f, 0.18f, 1f);
        }
    }

    [ContextMenu("Test Damage 50")]
    private void TestDamage()
    {
        if (Application.isPlaying) TakeDamage(50);
    }

    [ContextMenu("Test Health/Print Health")]
    private void TestPrintHealth()
    {
        if (!Application.isPlaying) return;
        Debug.Log($"DefenseTowerHealth: {gameObject.name} HP = {currentHealth}/{maxHealth}. IsDestroyed = {isDestroyed}.", this);
    }
}
