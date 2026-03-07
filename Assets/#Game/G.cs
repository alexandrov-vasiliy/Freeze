/// <summary>
/// Глобальный контейнер сервисов. Все сервисы инжектятся извне (например, из Main), без синглтонов.
/// </summary>
public static class G
{
    public static ResourceService Resources;
    public static DeathPanel DeathPanel;
    public static MainMenu mainMenu;
}
