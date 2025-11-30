using UnityEngine;
using UnityEditor;
using System.IO;

public class HeightMapGeneratorWindow : EditorWindow
{
    int width = 512;
    int height = 512;
    float scale = 20f;
    int octaves = 4;
    float persistence = 0.5f;
    float lacunarity = 2f;
    int seed = 0;
    Vector2 offset = Vector2.zero;

    Texture2D previewTexture;

    [MenuItem("Tools/Height Map Generator")]
    public static void ShowWindow()
    {
        GetWindow<HeightMapGeneratorWindow>("Height Map Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Height Map Settings", EditorStyles.boldLabel);

        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);
        scale = EditorGUILayout.FloatField("Scale", scale);
        octaves = EditorGUILayout.IntSlider("Octaves", octaves, 1, 10);
        persistence = EditorGUILayout.Slider("Persistence", persistence, 0, 1);
        lacunarity = EditorGUILayout.Slider("Lacunarity", lacunarity, 1, 10);
        seed = EditorGUILayout.IntField("Seed", seed);
        offset = EditorGUILayout.Vector2Field("Offset", offset);

        if (GUILayout.Button("Generate"))
        {
            GenerateMap();
        }

        if (previewTexture != null)
        {
            GUILayout.Label("Preview:");
            // Draw preview maintaining aspect ratio, max width of window
            float aspect = (float)width / height;
            float previewWidth = Mathf.Min(position.width - 20, 512);
            float previewHeight = previewWidth / aspect;
            Rect previewRect = GUILayoutUtility.GetRect(previewWidth, previewHeight);
            EditorGUI.DrawPreviewTexture(previewRect, previewTexture);

            if (GUILayout.Button("Save Texture"))
            {
                SaveTexture();
            }
        }
    }

    void GenerateMap()
    {
        float[,] noiseMap = GenerateNoiseMap(width, height, seed, scale, octaves, persistence, lacunarity, offset);
        previewTexture = GenerateTexture(noiseMap);
    }

    float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistence, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0) scale = 0.0001f;

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }

    Texture2D GenerateTexture(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);
        Color[] colourMap = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }

    void SaveTexture()
    {
        byte[] bytes = previewTexture.EncodeToPNG();
        string path = "Assets/HeightMap.png";
        // Ensure unique name
        int count = 1;
        while (File.Exists(path))
        {
            path = $"Assets/HeightMap_{count}.png";
            count++;
        }
        
        File.WriteAllBytes(path, bytes);
        AssetDatabase.Refresh();
        Debug.Log("Saved Height Map to " + path);
    }
}
