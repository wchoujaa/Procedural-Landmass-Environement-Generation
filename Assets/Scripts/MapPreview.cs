using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

[Serializable]
public struct DataRegion
{
    public Vector3 localisation;
    public HeightMapSettings settings;
}
[ExecuteInEditMode]
public class MapPreview : MonoBehaviour
{

    public Renderer textureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public Transform eastLocation;
    public Transform westLocation;
    public Vector3 eastLocationPos = Vector3.zero;
    public Vector3 westLocationPos = Vector3.zero;
    public HeightMapSettings eastSettings;
    public HeightMapSettings westSettings;

    public enum DrawMode { NoiseMap, Mesh, FalloffMap };
    public DrawMode drawMode;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureData;


    public Material terrainMaterial;

    [Range(0, 10)]
    public float power = 0.1f;
    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int editorPreviewLOD;
    public bool autoUpdate;

   



    public void DrawMapInEditor()
    {

        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
        HeightMap heightMap;
        if (eastSettings != null && westSettings != null && westLocation != null && westLocation != null)
        {
            Region east = new Region(eastSettings, eastLocationPos);
            Region west = new Region(westSettings, westLocationPos);
            heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings, new Region(heightMapSettings, east, west, Vector3.zero, power), Vector2.zero);
        }
        else
        {
            heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings, heightMapSettings, Vector2.zero);

        }

        if (drawMode == DrawMode.NoiseMap)
        {
            DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numVertsPerLine), 0, 1)));
        }
    }





    public void DrawTexture(Texture2D texture)
    {
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f;

        textureRender.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }

    public void DrawMesh(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();

        textureRender.gameObject.SetActive(false);
        meshFilter.gameObject.SetActive(true);
    }



    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    void OnValidate()
    {

        if (meshSettings != null)
        {
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
        if (eastSettings != null)
        {
            eastSettings.OnValuesUpdated -= OnValuesUpdated;
            eastSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (westSettings != null)
        {
            westSettings.OnValuesUpdated -= OnValuesUpdated;
            westSettings.OnValuesUpdated += OnValuesUpdated;
        }


    }

    void FixedUpdate()
    {
        if (eastLocation == null || westLocation == null) return;
 
        eastLocationPos = eastLocation.position;
        westLocationPos = westLocation.position;
        StartCoroutine(WaitAndCheck(0.3f));

    }

 
    IEnumerator WaitAndCheck(float waittime)
    {
        yield return new WaitForSeconds(waittime);
         if (eastLocationPos == eastLocation.position || westLocationPos == westLocation.position)
        {
            DrawMapInEditor();
        }
 
    }
}
