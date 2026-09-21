using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// Запуск из меню только в копии проекта для проверки.
[InitializeOnLoad]
public static class PracticeVerification
{
    private static int step;
    private static double next;
    private static int oldUnlocked;
    private static float oldVolume;
    private static readonly int[] oldBest = new int[3];
    private const string Prefix = "PP12.Crystal.v1.";

    static PracticeVerification()
    {
        if (SessionState.GetBool("PP13.Verify", false))
        {
            step = -10;
            next = EditorApplication.timeSinceStartup + 5;
            EditorApplication.update += Tick;
        }
    }

    [MenuItem("Practice/Verify And Capture")]
    public static void Run()
    {
        Directory.CreateDirectory("Documentation");
        File.WriteAllText("Documentation/verification.txt", "RUNNING");
        SessionState.SetBool("PP13.Verify", true);
        SessionState.SetInt("PP13.OldTutorial", PlayerPrefs.GetInt(TutorialController.SeenKey, 0));
        PlayerPrefs.SetInt(TutorialController.SeenKey, 0);
        oldUnlocked = SaveService.Unlocked;
        oldVolume = SaveService.Volume;
        for (int i = 0; i < 3; i++) oldBest[i] = SaveService.Best(i + 1);
        SessionState.SetInt("PP13.OldUnlocked", oldUnlocked);
        SessionState.SetFloat("PP13.OldVolume", oldVolume);
        for (int i = 0; i < 3; i++) SessionState.SetInt("PP13.OldBest" + i, oldBest[i]);
        PlayerPrefs.SetInt(Prefix + "Unlocked", 1);
        PlayerPrefs.Save();
        Application.runInBackground = true;
        PracticeProjectSetup.PrepareMainScene();
        EditorApplication.isPlaying = true;
        step = -10;
        next = EditorApplication.timeSinceStartup + 4;
        EditorApplication.update += Tick;
    }

    [MenuItem("Practice/Verify UI And Settings")]
    public static void CheckUI()
    {
        var game = GameManager.Instance;
        if (game == null) return;
        game.ToMenu();
        var buttons = UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Button>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var button in buttons)
            if (button.name == "Settings") button.onClick.Invoke();
        SaveService.SetVolume(0.4f);
        EditorApplication.delayCall += () => Capture("settings");
        Debug.Log("PASS Settings button callback");
    }

    private static void Check(bool value, string message)
    {
        if (!value) throw new Exception(message);
        Debug.Log("PASS " + message);
    }

    private static void Capture(string name)
    {
        Directory.CreateDirectory("Documentation/Screenshots");
        ScreenCapture.CaptureScreenshot("Documentation/Screenshots/pp13-" + name + ".png");
    }

    private static void Tick()
    {
        if (!EditorApplication.isPlaying || GameManager.Instance == null ||
            EditorApplication.timeSinceStartup < next) return;
        next = EditorApplication.timeSinceStartup + 1.2;
        var game = GameManager.Instance;
        try
        {
            switch (step++)
            {
                case -10:
                    Check(game.GetComponent<TutorialController>().IsOpen, "First launch tutorial");
                    Capture("tutorial1"); break;
                case -9: game.GetComponent<TutorialController>().Advance(); break;
                case -8:
                    Check(game.GetComponent<TutorialController>().Step == 1, "Tutorial advances");
                    Capture("tutorial2"); break;
                case -7:
                    game.GetComponent<TutorialController>().Advance();
                    Capture("tutorial3"); break;
                case -6:
                    game.GetComponent<TutorialController>().Advance();
                    Check(PlayerPrefs.GetInt(TutorialController.SeenKey) == 1, "Tutorial persisted");
                    break;
                case -5: ClickButton("About"); break;
                case -4: Capture("about"); break;
                case -3: ClickButton("AboutBack"); break;
                case -2:
                    var safe = UnityEngine.Object.FindAnyObjectByType<SafeAreaPanel>();
                    safe.simulateInsets = true; safe.Apply();
                    Capture("safearea"); break;
                case -1:
                    ClickButton("Tutorial");
                    Check(game.GetComponent<TutorialController>().Step == 0, "Replay tutorial");
                    game.GetComponent<TutorialController>().Close();
                    UnityEngine.Object.FindAnyObjectByType<SafeAreaPanel>().simulateInsets = false;
                    break;
                case 0:
                    Check(!game.Levels.Select(2), "Closed level rejected");
                    Capture("menu");
                    CaptureLayout(1280, 720);
                    CaptureLayout(1440, 720);
                    CaptureLayout(1024, 768);
                    break;
                case 1: game.StartLevel(1); break;
                case 2:
                    Check(game.IsPlaying && game.Hearts == 3, "Level starts");
                    game.TryFinishLevel();
                    Check(game.IsPlaying, "Early exit blocked");
                    Capture("level1"); break;
                case 3: game.Pause(); break;
                case 4:
                    Check(Time.timeScale == 0 && !game.IsPlaying, "Pause freezes play");
                    Capture("pause"); break;
                case 5:
                    game.TogglePause();
                    Check(game.IsPlaying && Time.timeScale == 1, "Resume");
                    foreach (var c in UnityEngine.Object.FindObjectsByType<Collectible>(
                        FindObjectsSortMode.None)) game.Collect(c);
                    game.TryFinishLevel(); break;
                case 6:
                    Check(game.CurrentState == GameManager.State.Won &&
                        SaveService.Unlocked == 2, "Win unlocks next level");
                    Capture("won"); break;
                case 7: game.ToMenu(); break;
                case 8: Capture("unlocked"); break;
                case 9: game.StartLevel(2); break;
                case 10: Capture("level2"); game.HitHazard(); break;
                case 11: game.HitHazard(); break;
                case 12: game.HitHazard(); break;
                case 13:
                    Check(game.CurrentState == GameManager.State.Lost,
                        "Three hazards cause loss");
                    Check(SaveService.Unlocked == 2, "Loss does not unlock");
                    Capture("lost"); break;
                case 14:
                    game.StartLevel(2);
                    Check(game.Collected == 0 && game.Hearts == 3, "Retry resets run");
                    SaveService.SetVolume(0.4f);
                    Check(Mathf.Approximately(SaveService.Volume, 0.4f), "Volume saved");
                    foreach (var c in UnityEngine.Object.FindObjectsByType<Collectible>(
                        FindObjectsSortMode.None)) game.Collect(c);
                    game.TryFinishLevel(); break;
                case 15: game.StartLevel(3); break;
                case 16: Capture("level3"); break;
                case 17:
                    foreach (var c in UnityEngine.Object.FindObjectsByType<Collectible>(
                        FindObjectsSortMode.None)) game.Collect(c);
                    game.TryFinishLevel(); break;
                case 18:
                    Check(SaveService.Unlocked == 3, "Final level bounded at three");
                    Capture("completed"); break;
                case 19: game.ToMenu(); break;
                case 20:
                    ClickButton("Settings"); break;
                case 21:
                    Capture("settings"); break;
                case 22:
                    ClickButton("Back"); break;
                case 23: ClickButton("Level1"); break;
                case 24:
                    Check(game.IsPlaying, "Level button starts game");
                    var right = FindButton("Right").GetComponent<HoldButton>();
                    right.OnPointerDown(new UnityEngine.EventSystems.PointerEventData(
                        UnityEngine.EventSystems.EventSystem.current));
                    Check(game.MobileDirection == Vector2.right, "Hold sets direction");
                    right.OnPointerUp(null);
                    Check(game.MobileDirection == Vector2.zero, "Release clears direction");
                    ClickButton("PauseButton"); break;
                case 25:
                    Check(game.CurrentState == GameManager.State.Paused,
                        "Pause button pauses");
                    ClickButton("Resume"); break;
                case 26:
                    Check(game.IsPlaying, "Resume button resumes");
                    game.SendMessage("OnApplicationFocus", false,
                        SendMessageOptions.DontRequireReceiver);
                    Check(game.CurrentState == GameManager.State.Paused,
                        "Focus loss pauses");
                    game.ToMenu();
                    File.WriteAllText("Documentation/verification.txt",
                        "PASS: start, closed level, early exit, pause, resume, win, " +
                        "unlock, loss, retry, volume, final level, UI raycasts, button callbacks, " +
                        "hold/release, focus-loss callback; first tutorial, steps, seen flag, replay, about, simulated safe area.\n" +
                        "These checks call game methods; manual input tests are separate.\n");
                    Finish(); break;
            }
        }
        catch (Exception error)
        {
            File.WriteAllText("Documentation/verification.txt", "FAIL: " + error.Message);
            Debug.LogException(error);
            Finish();
        }
    }

    private static UnityEngine.UI.Button FindButton(string name)
    {
        foreach (var button in UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Button>(
            FindObjectsSortMode.None))
            if (button.name == name) return button;
        throw new Exception("Active button not found: " + name);
    }

    private static void ClickButton(string name)
    {
        Canvas.ForceUpdateCanvases();
        var button = FindButton(name);
        var rect = (RectTransform)button.transform;
        var point = RectTransformUtility.WorldToScreenPoint(null,
            rect.TransformPoint(rect.rect.center));
        var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(
            new UnityEngine.EventSystems.PointerEventData(
                UnityEngine.EventSystems.EventSystem.current) { position = point }, hits);
        Check(hits.Count > 0 && hits[0].gameObject == button.gameObject,
            "Raycast reaches " + name + " (hits: " +
            string.Join(",", hits.ConvertAll(h => h.gameObject.name)) + ")");
        Check(button.interactable, "Button enabled: " + name);
        button.onClick.Invoke();
    }

    private static void CaptureLayout(int width, int height)
    {
        var canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        var safe = UnityEngine.Object.FindAnyObjectByType<SafeAreaPanel>();
        var cam = Camera.main;
        var rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 1;
        Canvas.ForceUpdateCanvases();
        safe.ApplyRect(new Rect(width * 0.08f, height * 0.05f,
            width * 0.84f, height * 0.9f), new Vector2(width, height));
        Canvas.ForceUpdateCanvases();
        foreach (var button in UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Button>(
            FindObjectsSortMode.None))
        {
            var corners = new Vector3[4];
            ((RectTransform)button.transform).GetWorldCorners(corners);
            foreach (var corner in corners)
            {
                var pixel = cam.WorldToScreenPoint(corner);
                Check(pixel.x >= width * 0.08f - 1 && pixel.x <= width * 0.92f + 1 &&
                    pixel.y >= height * 0.05f - 1 && pixel.y <= height * 0.95f + 1,
                    "Safe bounds " + width + "x" + height + " " + button.name);
            }
        }
        cam.Render();
        var previous = RenderTexture.active;
        RenderTexture.active = rt;
        var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0); texture.Apply();
        File.WriteAllBytes("Documentation/Screenshots/pp13-layout-" +
            width + "x" + height + ".png", texture.EncodeToPNG());
        RenderTexture.active = previous;
        cam.targetTexture = null;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        UnityEngine.Object.DestroyImmediate(texture);
        rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
        Canvas.ForceUpdateCanvases(); safe.Apply();
    }

    private static void Finish()
    {
        EditorApplication.update -= Tick;
        SessionState.SetBool("PP13.Verify", false);
        oldUnlocked = SessionState.GetInt("PP13.OldUnlocked", 1);
        oldVolume = SessionState.GetFloat("PP13.OldVolume", 0.7f);
        for (int i = 0; i < 3; i++) oldBest[i] = SessionState.GetInt("PP13.OldBest" + i, 0);
        PlayerPrefs.SetInt(Prefix + "Unlocked", oldUnlocked);
        for (int i = 0; i < 3; i++) PlayerPrefs.SetInt(Prefix + "Best" + (i + 1), oldBest[i]);
        SaveService.SetVolume(oldVolume);
        PlayerPrefs.SetInt(TutorialController.SeenKey, SessionState.GetInt("PP13.OldTutorial", 0));
        PlayerPrefs.Save();
        EditorApplication.isPlaying = false;
        EditorApplication.delayCall += () => EditorApplication.Exit(
            File.ReadAllText("Documentation/verification.txt").StartsWith("PASS") ? 0 : 1);
    }
}
