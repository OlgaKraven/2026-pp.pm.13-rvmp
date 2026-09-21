using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class ReleaseAssets
{
    [MenuItem("Practice/Export Release Artwork")]
    public static void Export()
    {
        Directory.CreateDirectory("Release");
        var scene = EditorSceneManager.NewPreviewScene();
        var camObject = new GameObject("ArtworkCamera");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(camObject, scene);
        var cam = camObject.AddComponent<Camera>();
        cam.scene = scene;
        cam.transform.position = new Vector3(0, 0, -10);
        cam.orthographic = true; cam.orthographicSize = 3;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.035f, 0.055f, 0.10f);
        var symbol = GameObject.CreatePrimitive(PrimitiveType.Quad);
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(symbol, scene);
        symbol.transform.rotation = Quaternion.Euler(0, 0, 45);
        symbol.transform.localScale = new Vector3(2.3f, 2.3f, 1);
        var material = new Material(Shader.Find("Unlit/Color"));
        material.color = new Color(1, 0.76f, 0.12f);
        symbol.GetComponent<Renderer>().sharedMaterial = material;
        Save(cam, 512, 512, "Release/icon.png");
        symbol.transform.position = new Vector3(-3.4f, 0, 0);
        var c = new GameObject("ArtworkCanvas", typeof(Canvas), typeof(CanvasScaler));
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(c, scene);
        var canvas = c.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = cam;
        canvas.planeDistance = 5;
        var scaler = c.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        Text(c.transform, "КРИСТАЛЬНЫЙ\nМАРШРУТ", new Vector2(190, 90), 62);
        Text(c.transform, "Соберите цель. Найдите портал.\nТри уровня · локальный прогресс", new Vector2(190, -90), 28);
        Save(cam, 1280, 720, "Release/promo.png");
        EditorSceneManager.ClosePreviewScene(scene);
        Object.DestroyImmediate(material);
        Debug.Log("PASS Release artwork exported with Unity camera");
    }

    private static void Text(Transform parent, string value, Vector2 position, int size)
    {
        var go = new GameObject("Caption", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(780, 220); rt.anchoredPosition = position;
        var label = go.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = value; label.fontSize = size; label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;
    }

    private static void Save(Camera cam, int width, int height, string path)
    {
        var rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt; cam.aspect = (float)width / height;
        Canvas.ForceUpdateCanvases(); cam.Render();
        var previous = RenderTexture.active; RenderTexture.active = rt;
        var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0); texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        RenderTexture.active = previous; cam.targetTexture = null;
        Object.DestroyImmediate(texture); rt.Release(); Object.DestroyImmediate(rt);
    }
}
