using UnityEngine;

/// <summary>
/// Глобальный контейнер сервисов. Все сервисы инжектятся извне (например, из Main), без синглтонов.
/// </summary>
public static class G
{
    public static ResourceService Resources;
    public static DeathPanel DeathPanel;
    public static MainMenu mainMenu;
    public static SubtitlePlayer subtitleText;

    /// <summary>
    /// Ссылка на монстра на сцене. Задаётся самим монстром при появлении; используется MonsterChaseZone, чтобы не прокидывать ссылку в каждую зону.
    /// </summary>
    public static MonsterChaseRetreat Monster;
    public static GameObject hintText;
}
