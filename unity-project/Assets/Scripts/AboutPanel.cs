using UnityEngine;

public class AboutPanel : MonoBehaviour
{
    private GameObject panel;
    public bool IsOpen => panel != null && panel.activeSelf;

    public void Initialize(MenuController ui, Transform parent)
    {
        panel = ui.Panel("AboutPanel", parent, 0, 0, 1000, 580);
        ui.Label(panel.transform, "ОБ ИГРЕ", 0, 220, 900, 60, 36);
        ui.Label(panel.transform, "Кристальный маршрут · " + Application.version +
            "\nУчебный пример ПП.ПМ.13\n" +
            "Три маршрута: соберите кристаллы и найдите портал.\n" +
            "Графика и звуки создаются кодом примера.\n" +
            "Unity UI; шрифт LegacyRuntime из Unity.\n" +
            "Источники перечислены в Release/CREDITS.md.",
            0, 20, 900, 300, 25);
        ui.Button(panel.transform, "AboutBack", "Назад", 0, -215,
            420, 65, () => panel.SetActive(false));
        panel.SetActive(false);
    }

    public void Open()
    {
        if (GameManager.Instance.CurrentState != GameManager.State.Menu) return;
        panel.SetActive(true);
        panel.transform.SetAsLastSibling();
    }
}
