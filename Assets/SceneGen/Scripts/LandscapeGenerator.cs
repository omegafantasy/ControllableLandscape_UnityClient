using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System.IO;
using System.Diagnostics;
using UnityEngine.Rendering.HighDefinition; 


public class LandscapeGenerator : EditorWindow
{
    private string inputFolder = "Assets/SceneGen/Inputs"; 
    private string sceneFolder = "Assets/SceneGen/Scene_Landscape";
    private string matFolder = "Assets/SceneGen/Mat_Landscape";
    private string imageFolder = "Assets/SceneGen/Image_Landscape"; 
    private Material terrainMaterial;
    private int terrainWidth = 440; 
    private int terrainLength = 440; 
    private int terrainHeight = 80;
    private float waterHeight = 6.4f; 

    private int resolution = 1024;
    private int widthOffset = -20;
    private int lengthOffset = -20;



    [MenuItem("Tools/Landscape Generator")]
    public static void ShowWindow()
    {
        GetWindow<LandscapeGenerator>("Landscape Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Generate Settings", EditorStyles.boldLabel);
        inputFolder = EditorGUILayout.TextField("Input Folder", inputFolder);
        sceneFolder = EditorGUILayout.TextField("Scene Folder", sceneFolder);
        matFolder = EditorGUILayout.TextField("Mat Folder", matFolder);

        if (GUILayout.Button("Generate"))
        {
            if (!Directory.Exists(sceneFolder))
            {
                Directory.CreateDirectory(sceneFolder);
            }
            if (!Directory.Exists(matFolder))
            {
                Directory.CreateDirectory(matFolder);
            }
            if (!Directory.Exists(inputFolder))
            {
                Directory.CreateDirectory(inputFolder);
            }
            if (!Directory.Exists(imageFolder))
            {
                Directory.CreateDirectory(imageFolder);
            }
            Gen();
        }
    }

    void Gen()
    {
        string[] jsonFiles = Directory.GetFiles(inputFolder, "*.json");

        for (int i = 0; i < jsonFiles.Length; i++)
        {
            string sceneName = Path.GetFileNameWithoutExtension(jsonFiles[i]);
            // parse json
            string jsonText = File.ReadAllText(jsonFiles[i]);
            JsonData jsonData = JsonUtility.FromJson<JsonData>(jsonText);
            string heightmapPath = inputFolder + "/" + jsonData.height_map_path;
            string labelmapPath = inputFolder + "/" + jsonData.label_map_path;
            // UnityEngine.Debug.Log(heightmapPath + " " + labelmapPath);

            // generate
            terrainWidth = jsonData.real_width;
            terrainLength = jsonData.real_height;
            terrainHeight = jsonData.max_height;
            waterHeight = jsonData.water_height;
            widthOffset = jsonData.width_offset;
            lengthOffset = jsonData.height_offset;
            resolution = Mathf.Max(jsonData.map_width, jsonData.map_height);
            CreateTerrainFromHeightmap(heightmapPath, labelmapPath, sceneName);

            Scene activeScene = EditorSceneManager.GetActiveScene();
            JsonTreePlacerEditor.PlaceTreesFromJson(jsonData, activeScene);
            EditorSceneManager.OpenScene(activeScene.path, OpenSceneMode.Single);
            // capture images, optional
            CaptureBirdsEyeView(terrainWidth, terrainWidth * jsonData.map_height / jsonData.map_width, widthOffset, lengthOffset);
            CaptureFrontView(terrainWidth, terrainWidth * jsonData.map_height / jsonData.map_width, widthOffset, lengthOffset);
            CaptureRandomView(jsonData.viewpoints);
        }
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

    void CaptureFrontView(float xlength, float zlength, float xoffset, float zoffset)
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
                camera.transform.position = new Vector3(xlength / 2 + xoffset, 50, -30 + zoffset);
                camera.transform.rotation = Quaternion.Euler(15, 0, 0);
            }
            else if (i == 1)
            {
                camera.transform.position = new Vector3(xlength / 2 + xoffset, 50, zlength + zoffset + 30);
                camera.transform.rotation = Quaternion.Euler(15, 180, 0);
            }
            else if (i == 2)
            {
                camera.transform.position = new Vector3(-30 + xoffset, 50, zlength / 2 + zoffset);
                camera.transform.rotation = Quaternion.Euler(15, 90, 0);
            }
            else
            {
                camera.transform.position = new Vector3(xlength + xoffset + 30, 50, zlength / 2 + zoffset);
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

    void CaptureBirdsEyeView(float xlength, float zlength, float xoffset, float zoffset)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string savePath = imageFolder + "/" + sceneName + ".png";

        GameObject camera = new GameObject("Camera");
        camera.AddComponent<Camera>();
        camera.transform.position = new Vector3(xlength / 2 + xoffset, 200, zlength / 2 + zoffset);
        camera.transform.rotation = Quaternion.Euler(90, 0, 0);
        camera.GetComponent<Camera>().orthographic = true;
        camera.GetComponent<Camera>().orthographicSize = Mathf.Min(xlength, zlength) / 2;
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

    void CreateTerrainFromHeightmap(string heightmapPath, string labelmapPath, string sceneName)
    {
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        EditorSceneManager.SetActiveScene(newScene);

        GlobalPrefab();
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = resolution; 
        terrainData.size = new Vector3(terrainWidth, terrainHeight, terrainLength);
        GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
        terrainObject.transform.position = new Vector3(widthOffset, 0, lengthOffset);

        ApplyNewTerrainMaterial(terrainObject, labelmapPath, sceneName);

        byte[] heightmapBytes = File.ReadAllBytes(heightmapPath);
        Texture2D heightmapTexture = new Texture2D(2, 2);
        heightmapTexture.LoadImage(heightmapBytes);
        float[,] heights = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        for (int y = 0; y < terrainData.heightmapResolution; y++)
        {
            for (int x = 0; x < terrainData.heightmapResolution; x++)
            {
                heights[y, x] = heightmapTexture.GetPixel(x, y).grayscale;
            }
        }
        terrainData.SetHeights(0, 0, heights);

        string savePath = sceneFolder + "/" + sceneName + ".unity";
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
        EditorSceneManager.SaveScene(newScene, savePath);
    }

    void CreateDirectionalLight()
    {
        GameObject lightGameObject = new GameObject("Directional Light");
        Light lightComp = lightGameObject.AddComponent<Light>();

        lightComp.type = LightType.Directional;
        lightComp.color = Color.white;
        lightComp.intensity = 1.5f;

        lightComp.shadows = LightShadows.Soft;

        lightGameObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        HDAdditionalLightData hdLight = lightGameObject.AddComponent<HDAdditionalLightData>();
    }

    void ApplyNewTerrainMaterial(GameObject terrainObject, string labelmapPath, string sceneName)
    {
        Shader terrainShader = Shader.Find("Shader Graphs/TerrainBlend");
        if (terrainShader == null)
        {
            UnityEngine.Debug.LogError("TerrainBlend shader not found.");
            return;
        }

        Material newTerrainMaterial = new Material(terrainShader);

        if (!string.IsNullOrEmpty(labelmapPath))
        {
            Texture2D labelmapTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(labelmapPath);
            if (labelmapTexture != null)
            {
                newTerrainMaterial.SetTexture("_Tex_Blend", labelmapTexture);
                UnityEngine.Debug.Log("Texture applied: " + labelmapPath);
            }
            else
            {
                UnityEngine.Debug.LogWarning("Labelmap not found at path: " + labelmapPath);
            }
        }

        string savePath = matFolder + "/" + sceneName + "_Material.mat";
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
        AssetDatabase.CreateAsset(newTerrainMaterial, savePath);

        if (terrainObject != null)
        {
            Terrain terrain = terrainObject.GetComponent<Terrain>();
            if (terrain != null)
            {
                terrain.materialTemplate = newTerrainMaterial;
            }
        }
    }

    void GlobalPrefab()
    {
        GameObject globalVolumePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SceneGen/Environment/Global Volume.prefab");
        GameObject waterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SceneGen/Environment/Water System.prefab");
        if (globalVolumePrefab != null && waterPrefab != null)
        {
            PrefabUtility.InstantiatePrefab(globalVolumePrefab);
            GameObject waterInstance = (GameObject)PrefabUtility.InstantiatePrefab(waterPrefab);

            float customYValue = waterHeight;
            Vector3 position = waterInstance.transform.position;
            position.y = customYValue;
            waterInstance.transform.position = position;
        }
        else
        {
            UnityEngine.Debug.LogError("Failed to load Global Volume prefab.");
        }
    }

}
