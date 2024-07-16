using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System.IO;
using System.Diagnostics;
using UnityEngine.Rendering.HighDefinition;


public class SceneRenderer : EditorWindow
{
    private string imageFolder = "Assets/SceneGen/Image_Landscape";
    private float minx, maxx, minz, maxz;
    private float width, height;
    private float baseHeight;


    [MenuItem("Tools/Scene Renderer")]
    public static void ShowWindow()
    {
        GetWindow<SceneRenderer>("Scene Renderer");
    }

    void OnGUI()
    {
        GUILayout.Label("Scene Render Settings", EditorStyles.boldLabel);
        imageFolder = EditorGUILayout.TextField("Output Folder", imageFolder);
        minx = EditorGUILayout.FloatField("Min X", minx);
        maxx = EditorGUILayout.FloatField("Max X", maxx);
        minz = EditorGUILayout.FloatField("Min Z", minz);
        maxz = EditorGUILayout.FloatField("Max Z", maxz);
        baseHeight = EditorGUILayout.FloatField("Base Height", baseHeight);

        if (GUILayout.Button("Generate"))
        {
            if (!Directory.Exists(imageFolder))
            {
                Directory.CreateDirectory(imageFolder);
            }
            width = maxx - minx;
            height = maxz - minz;
            Generate();
        }
    }

    List<ViewpointsData> GenerateViewpoints()
    {
        var view_points = new List<ViewpointsData>(); // x, y, z, xrot, yrot 
        float x1 = 0.05f * width + minx, x2 = 0.15f * width + minx, x3 = 0.5f * width + minx, x4 = 0.85f * width + minx, x5 = 0.95f * width + minx;
        float z1 = 0.05f * height + minz, z2 = 0.15f * height + minz, z3 = 0.5f * height + minz, z4 = 0.85f * height + minz, z5 = 0.95f * height + minz;
        float xrot = 25f * Mathf.PI / 180;
        view_points.Add(new ViewpointsData(x1, 0, z1, xrot, Mathf.PI / 4));
        view_points.Add(new ViewpointsData(x1, 0, z5, xrot, -Mathf.PI / 4));
        view_points.Add(new ViewpointsData(x5, 0, z1, xrot, 3 * Mathf.PI / 4));
        view_points.Add(new ViewpointsData(x5, 0, z5, xrot, -3 * Mathf.PI / 4));
        // four edges
        view_points.Add(new ViewpointsData(x2, 0, z3, xrot, Mathf.PI / 4));
        view_points.Add(new ViewpointsData(x2, 0, z3, xrot, -Mathf.PI / 4));
        view_points.Add(new ViewpointsData(x3, 0, z2, xrot, 3 * Mathf.PI / 4));
        view_points.Add(new ViewpointsData(x3, 0, z2, xrot, Mathf.PI / 4));
        view_points.Add(new ViewpointsData(x4, 0, z3, xrot, -3 * Mathf.PI / 4));
        view_points.Add(new ViewpointsData(x4, 0, z3, xrot, 3 * Mathf.PI / 4));
        view_points.Add(new ViewpointsData(x3, 0, z4, xrot, -Mathf.PI / 4));
        view_points.Add(new ViewpointsData(x3, 0, z4, xrot, -3 * Mathf.PI / 4));
        // central
        for (int i = 0; i < 4; i++)
        {
            view_points.Add(new ViewpointsData(x3, 0, z3, xrot, i * Mathf.PI / 2));
        }

        for (int i = 0; i < view_points.Count; i++)
        {
            float x = view_points[i].x;
            float z = view_points[i].z;
            float xr = view_points[i].xrot;
            float yr = view_points[i].yrot;
            float y = baseHeight + 15;
            yr = Mathf.PI / 2 - yr;
            view_points[i] = new ViewpointsData(x, y, z, xr, yr);
        }

        return view_points;
    }

    void Generate()
    {
        CaptureBirdsEyeView();
        CaptureFrontView();
        var viewpoints = GenerateViewpoints();
        CaptureRandomView(viewpoints);
    }

    void CaptureRandomView(List<ViewpointsData> viewpoints)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        GameObject camera = new GameObject("Camera");
        camera.AddComponent<Camera>();
        int resx = 2048, resy = (int)(resx * 9 / 16);
        camera.GetComponent<Camera>().targetTexture = new RenderTexture(resx, resy, 24);
        for (int i = 0; i < viewpoints.Count; i++)
        {
            string savePath = imageFolder + "/" + sceneName + "_view" + i + ".png";
            camera.transform.position = new Vector3(viewpoints[i].x, viewpoints[i].y, viewpoints[i].z);
            camera.transform.rotation = Quaternion.Euler(viewpoints[i].xrot * 180 / Mathf.PI, viewpoints[i].yrot * 180 / Mathf.PI, 0);
            camera.GetComponent<Camera>().Render();
            RenderTexture.active = camera.GetComponent<Camera>().targetTexture;
            Texture2D image = new Texture2D(resx, resy, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, resx, resy), 0, 0);
            image.Apply();
            byte[] bytes = image.EncodeToPNG();
            File.WriteAllBytes(savePath, bytes);
        }
        DestroyImmediate(camera);
    }

    void CaptureFrontView()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        GameObject camera = new GameObject("Camera");
        camera.AddComponent<Camera>();
        int resx = 2048, resy = (int)(resx * 9 / 16);
        camera.GetComponent<Camera>().targetTexture = new RenderTexture(resx, resy, 24);
        for (int i = 0; i < 4; i++)
        {
            if (i == 0)
            {
                camera.transform.position = new Vector3((minx + maxx) / 2, 50, minz - 50);
                camera.transform.rotation = Quaternion.Euler(15, 0, 0);
            }
            else if (i == 1)
            {
                camera.transform.position = new Vector3((minx + maxx) / 2, 50, maxz + 50);
                camera.transform.rotation = Quaternion.Euler(15, 180, 0);
            }
            else if (i == 2)
            {
                camera.transform.position = new Vector3(minx - 50, 50, (minz + maxz) / 2);
                camera.transform.rotation = Quaternion.Euler(15, 90, 0);
            }
            else
            {
                camera.transform.position = new Vector3(maxx + 50, 50, (minz + maxz) / 2);
                camera.transform.rotation = Quaternion.Euler(15, -90, 0);
            }
            string savePath = imageFolder + "/" + sceneName + "_front" + i + ".png";
            camera.GetComponent<Camera>().Render();
            RenderTexture.active = camera.GetComponent<Camera>().targetTexture;
            Texture2D image = new Texture2D(resx, resy, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, resx, resy), 0, 0);
            image.Apply();
            byte[] bytes = image.EncodeToPNG();
            File.WriteAllBytes(savePath, bytes);
        }
        DestroyImmediate(camera);
    }

    void CaptureBirdsEyeView()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string savePath = imageFolder + "/" + sceneName + ".png";

        GameObject camera = new GameObject("Camera");
        camera.AddComponent<Camera>();
        camera.transform.position = new Vector3((minx + maxx) / 2, 100, (minz + maxz) / 2);
        camera.transform.rotation = Quaternion.Euler(90, 0, 0);
        camera.GetComponent<Camera>().orthographic = true;
        camera.GetComponent<Camera>().orthographicSize = Mathf.Min(width, height) / 2;
        camera.GetComponent<Camera>().nearClipPlane = 3f;
        int resx = 2048, resy = (int)(resx * 9 / 16);
        camera.GetComponent<Camera>().targetTexture = new RenderTexture(resx, resy, 24);
        camera.GetComponent<Camera>().Render();
        RenderTexture.active = camera.GetComponent<Camera>().targetTexture;
        Texture2D image = new Texture2D(resx, resy, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, resx, resy), 0, 0);
        image.Apply();
        byte[] bytes = image.EncodeToPNG();
        File.WriteAllBytes(savePath, bytes);
        DestroyImmediate(camera);
    }

}
