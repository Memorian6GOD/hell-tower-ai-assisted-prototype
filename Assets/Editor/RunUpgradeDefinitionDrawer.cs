#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RunUpgradeDefinition))]
public sealed class RunUpgradeDefinitionDrawer : PropertyDrawer
{
    private const float VerticalSpacing = 2f;
    private const float SpecialInfoHeight = 38f;

    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label
    )
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty upgradeType =
            property.FindPropertyRelative("upgradeType");

        SerializedProperty displayName =
            property.FindPropertyRelative("displayName");

        SerializedProperty customDescription =
            property.FindPropertyRelative("customDescription");

        SerializedProperty bonusPercent =
            property.FindPropertyRelative("bonusPercent");

        SerializedProperty maximumStacks =
            property.FindPropertyRelative("maximumStacks");

        string foldoutTitle =
            string.IsNullOrWhiteSpace(displayName.stringValue)
                ? label.text
                : displayName.stringValue;

        Rect currentRect = new Rect(
            position.x,
            position.y,
            position.width,
            EditorGUIUtility.singleLineHeight
        );

        property.isExpanded =
            EditorGUI.Foldout(
                currentRect,
                property.isExpanded,
                foldoutTitle,
                true
            );

        if (!property.isExpanded)
        {
            EditorGUI.EndProperty();
            return;
        }

        EditorGUI.indentLevel++;

        MoveToNextLine(ref currentRect);
        EditorGUI.PropertyField(
            currentRect,
            upgradeType
        );

        MoveToNextLine(ref currentRect);
        EditorGUI.PropertyField(
            currentRect,
            displayName
        );

        bool usesDefinitionBalance =
            UsesDefinitionBalance(
                (RunUpgradeType)upgradeType.intValue
            );

        if (usesDefinitionBalance)
        {
            MoveToNextLine(ref currentRect);
            EditorGUI.PropertyField(
                currentRect,
                bonusPercent
            );

            MoveToNextLine(ref currentRect);
            EditorGUI.PropertyField(
                currentRect,
                maximumStacks
            );
        }
        else
        {
            float descriptionHeight =
                EditorGUI.GetPropertyHeight(
                    customDescription,
                    true
                );

            MoveToNextLine(
                ref currentRect,
                descriptionHeight
            );

            EditorGUI.PropertyField(
                currentRect,
                customDescription,
                true
            );

            MoveToNextLine(
                ref currentRect,
                SpecialInfoHeight
            );

            EditorGUI.HelpBox(
                currentRect,
                "Balance values and stack limit are controlled by " +
                "the matching special-card section in GameBalanceConfig.",
                MessageType.Info
            );
        }

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(
        SerializedProperty property,
        GUIContent label
    )
    {
        float height =
            EditorGUIUtility.singleLineHeight;

        if (!property.isExpanded)
        {
            return height;
        }

        SerializedProperty upgradeType =
            property.FindPropertyRelative("upgradeType");

        SerializedProperty customDescription =
            property.FindPropertyRelative("customDescription");

        height +=
            VerticalSpacing +
            EditorGUIUtility.singleLineHeight;

        height +=
            VerticalSpacing +
            EditorGUIUtility.singleLineHeight;

        bool usesDefinitionBalance =
            UsesDefinitionBalance(
                (RunUpgradeType)upgradeType.intValue
            );

        if (usesDefinitionBalance)
        {
            height +=
                VerticalSpacing +
                EditorGUIUtility.singleLineHeight;

            height +=
                VerticalSpacing +
                EditorGUIUtility.singleLineHeight;
        }
        else
        {
            height +=
                VerticalSpacing +
                EditorGUI.GetPropertyHeight(
                    customDescription,
                    true
                );

            height +=
                VerticalSpacing +
                SpecialInfoHeight;
        }

        return height;
    }

    private static bool UsesDefinitionBalance(
        RunUpgradeType upgradeType
    )
    {
        switch (upgradeType)
        {
            case RunUpgradeType.HeroDamage:
            case RunUpgradeType.HeroAttackSpeed:
            case RunUpgradeType.TowerDamage:
            case RunUpgradeType.TowerAttackSpeed:

            case RunUpgradeType.HeroDamageStrong:
            case RunUpgradeType.HeroAttackSpeedStrong:
            case RunUpgradeType.TowerDamageStrong:
            case RunUpgradeType.TowerAttackSpeedStrong:

            case RunUpgradeType.HeroCrit:
            case RunUpgradeType.HeroCritStrong:

            case RunUpgradeType.HeroHealth:
            case RunUpgradeType.HeroHealthStrong:

            case RunUpgradeType.TowerHealth:
            case RunUpgradeType.TowerHealthStrong:

                return true;

            default:
                return false;
        }
    }

    private static void MoveToNextLine(
        ref Rect rect,
        float height = -1f
    )
    {
        rect.y +=
            rect.height +
            VerticalSpacing;

        rect.height =
            height > 0f
                ? height
                : EditorGUIUtility.singleLineHeight;
    }
}
#endif
