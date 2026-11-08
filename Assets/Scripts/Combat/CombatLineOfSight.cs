using UnityEngine;

/// <summary>
/// Common line-of-fire check for the hero, enemies and towers.
/// Existing defense towers use the DefenseTower layer; future cover
/// objects can use the LOSObstacle layer without changing combat code.
/// </summary>
public static class CombatLineOfSight
{
    private const string DefenseTowerLayerName = "DefenseTower";
    private const string GeneralObstacleLayerName = "LOSObstacle";

    // A thin ray can skim the edge of a tower even while a projectile's
    // body would hit it. Give line-of-fire checks a small physical width.
    private const float ShotClearanceRadius = 0.25f;

    private static int? blockerLayerMask;

    public static bool HasClearShot(Vector3 origin, Vector3 targetPosition)
    {
        Vector3 toTarget = targetPosition - origin;
        float distance = toTarget.magnitude;

        if (distance <= 0.01f)
        {
            return true;
        }

        if (!blockerLayerMask.HasValue)
        {
            blockerLayerMask = LayerMask.GetMask(
                DefenseTowerLayerName,
                GeneralObstacleLayerName
            );
        }

        return !Physics.SphereCast(
            origin,
            ShotClearanceRadius,
            toTarget / distance,
            out _,
            distance,
            blockerLayerMask.Value,
            QueryTriggerInteraction.Ignore
        );
    }
}
