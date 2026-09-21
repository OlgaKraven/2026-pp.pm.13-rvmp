using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    public const string SeenKey = "PP13.Crystal.Tutorial.v1";
    private readonly string[] steps =
    {
        "1 / 3 · ДВИЖЕНИЕ\nУдерживайте экранную стрелку. Отпустите её, чтобы остановиться.",
        "2 / 3 · ЦЕЛЬ\nСоберите все жёлтые ромбы. Счётчик вверху показывает собранное и цель.",
        "3 / 3 · ОПАСНОСТЬ И ВЫХОД\nКрасный крест отнимает жизнь. Соберите цель и войдите в зелёный портал."
    };
    private GameObject panel;
    private Text message, nextLabel;
    private int index;
    public bool IsOpen => panel != null && panel.activeSelf;
    public int Step => index;

    public void Initialize(MenuController ui, Transform parent)
    {
        panel = ui.Panel("TutorialPanel", parent, 0, 0, 1000, 580);
        ui.Label(panel.transform, "КАК ИГРАТЬ", 0, 220, 900, 60, 36);
        message = ui.Label(panel.transform, "", 0, 45, 900, 230, 30);
        var next = ui.Button(panel.transform, "TutorialNext", "Далее", 200, -200,
            360, 70, Advance);
        nextLabel = next.GetComponentInChildren<Text>();
        ui.Button(panel.transform, "TutorialSkip", "Пропустить", -200, -200,
            360, 70, Close);
        panel.SetActive(false);
        if (PlayerPrefs.GetInt(SeenKey, 0) == 0) Open();
    }

    public void Open()
    {
        if (GameManager.Instance.CurrentState != GameManager.State.Menu) return;
        index = 0;
        panel.SetActive(true);
        panel.transform.SetAsLastSibling();
        Refresh();
    }

    public void Advance()
    {
        if (!IsOpen) return;
        if (index < steps.Length - 1) { index++; Refresh(); }
        else Close();
    }

    public void Close()
    {
        PlayerPrefs.SetInt(SeenKey, 1);
        PlayerPrefs.Save();
        panel.SetActive(false);
    }

    private void Refresh()
    {
        message.text = steps[index];
        nextLabel.text = index == steps.Length - 1 ? "В меню" : "Далее";
    }
}
