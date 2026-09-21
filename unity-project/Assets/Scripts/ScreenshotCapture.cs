using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class ScreenshotCapture : MonoBehaviour
{
    public string LastPath { get; private set; }
    private bool busy;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9)) Capture();
    }

    public void Capture()
    {
        if (!busy) StartCoroutine(SaveFrame());
    }

    private IEnumerator SaveFrame()
    {
        busy = true;
        yield return new WaitForEndOfFrame();
        string folder = Path.Combine(Application.persistentDataPath, "Screenshots");
        Directory.CreateDirectory(folder);
        LastPath = Path.Combine(folder,
            "crystal-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + ".png");
        ScreenCapture.CaptureScreenshot(LastPath);
        Debug.Log("Screenshot requested: " + LastPath);
        busy = false;
    }
}
