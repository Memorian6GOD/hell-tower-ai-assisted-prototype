using UnityEngine;

// Добавляет в меню Create команду для создания файла настроек.
[CreateAssetMenu(
    fileName = "GameBalanceConfig",
    menuName = "Hell Tower/Progression/Game Balance Config"
)]
public class GameBalanceConfig : ScriptableObject
{
    [Header("Hero - Base Attack")]

    // Урон героя до постоянных покупок и карточек забега.
    [SerializeField, Min(1)]
    private int heroBaseDamage = 20;

    // Интервал между выстрелами до улучшений скорости атаки.
    // Меньше интервал — чаще выстрелы.
    [SerializeField, Min(0.01f)]
    private float heroBaseAttackInterval = 1f;

    [Header("Hero - Critical Hit")]

    // Здесь вводим проценты: 3 означает шанс крита 3%.
    [SerializeField, Range(0f, 100f)]
    private float heroBaseCritChancePercent = 3f;

    // Во сколько раз критическое попадание сильнее обычного.
    [SerializeField, Min(1f)]
    private float heroCritDamageMultiplier = 2f;

    [Header("Base Health")]

    // Начальное максимальное здоровье каждого типа объекта.
    // Текущее здоровье во время боя здесь не хранится.
    [SerializeField, Min(1)]
    private int heroBaseMaxHealth = 100;

    [SerializeField, Min(1)]
    private int coreTowerBaseMaxHealth = 350;

    [SerializeField, Min(1)]
    private int defenseTowerBaseMaxHealth = 300;

    [Header("Defense Tower - Base Attack")]

    // Доля постоянного урона героя для одной защитной башни.
    // Карточки героя в эту основу не входят.
    // 0.5 означает 50%. Это тестовое значение для баланса.
    [SerializeField, Range(0f, 1f)]
    private float defenseTowerDamageCoefficient = 0.5f;

    // Собственный интервал стрельбы защитной башни.
    // Улучшения скорости атаки героя его не меняют.
    [SerializeField, Min(0.01f)]
    private float defenseTowerBaseAttackInterval = 1f;

    // Через эти свойства другие скрипты смогут читать настройки.
    // Изменять поля напрямую через свойства они не смогут.
    public int HeroBaseDamage => heroBaseDamage;

    public float HeroBaseAttackInterval =>
        heroBaseAttackInterval;

    public float HeroBaseCritChancePercent =>
        heroBaseCritChancePercent;

    public float HeroCritDamageMultiplier =>
        heroCritDamageMultiplier;

    public int HeroBaseMaxHealth => heroBaseMaxHealth;

    public int CoreTowerBaseMaxHealth =>
        coreTowerBaseMaxHealth;

    public int DefenseTowerBaseMaxHealth =>
        defenseTowerBaseMaxHealth;

    public float DefenseTowerDamageCoefficient =>
        defenseTowerDamageCoefficient;

    public float DefenseTowerBaseAttackInterval =>
        defenseTowerBaseAttackInterval;
}