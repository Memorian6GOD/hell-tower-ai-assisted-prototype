using UnityEngine;

public class DefenseTowerPlacementPreview : MonoBehaviour
{
    [Header("Preview Colors")]
    [SerializeField]
    private Color allowedColor =
        new Color(
            0.2f,
            1f,
            0.2f,
            0.55f
        );

    [SerializeField]
    private Color blockedColor =
        new Color(
            1f,
            0.2f,
            0.2f,
            0.55f
        );

    private Renderer[] previewRenderers;
    private MaterialPropertyBlock propertyBlock;

    private static readonly int BaseColorProperty =
        Shader.PropertyToID("_BaseColor");

    private static readonly int ColorProperty =
        Shader.PropertyToID("_Color");

    private void Awake()
    {
        previewRenderers =
            GetComponentsInChildren<Renderer>(
                true
            );

        propertyBlock =
            new MaterialPropertyBlock();

        SetPlacementAllowed(true);
    }

    public void SetPlacementAllowed(
        bool isAllowed
    )
    {
        Color selectedColor =
            isAllowed
                ? allowedColor
                : blockedColor;

        foreach (Renderer previewRenderer
                 in previewRenderers)
        {
            if (previewRenderer == null)
            {
                continue;
            }

            propertyBlock.Clear();

            previewRenderer.GetPropertyBlock(
                propertyBlock
            );

            propertyBlock.SetColor(
                BaseColorProperty,
                selectedColor
            );

            propertyBlock.SetColor(
                ColorProperty,
                selectedColor
            );

            previewRenderer.SetPropertyBlock(
                propertyBlock
            );
        }
    }
}