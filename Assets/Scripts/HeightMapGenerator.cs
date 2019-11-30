using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator
{

    public static HeightMap GenerateHeightMap(MeshSettings meshSettings, Region region, Vector2 sampleCentre)
    {
        int vertexWidth = meshSettings.numVertsPerLine;
        int vertexHeight = meshSettings.numVertsPerLine;
        int vertexScale = vertexWidth / 2;
        float worldScale = meshSettings.meshWorldSize / 2;

        Vector3 centre = region.localisation;
        HeightMapSettings centerSettings = region.heightMapSettings;
        HeightMapSettings eastSettings = region.east.heightMapSettings;
        HeightMapSettings westSettings = region.west.heightMapSettings;


        float[,] values = new float[vertexWidth, vertexHeight];

        float[,] valuesCenter = Noise.GenerateNoiseMap(vertexWidth, vertexHeight, centerSettings.noiseSettings, sampleCentre);
        float[,] valuesEast = Noise.GenerateNoiseMap(vertexWidth, vertexHeight, eastSettings.noiseSettings, sampleCentre);
        float[,] valuesWest = Noise.GenerateNoiseMap(vertexWidth, vertexHeight, westSettings.noiseSettings, sampleCentre);

        AnimationCurve heightCurve_threadsafe_Center = new AnimationCurve(centerSettings.heightCurve.keys);
        AnimationCurve heightCurve_threadsafe_East = new AnimationCurve(eastSettings.heightCurve.keys);
        AnimationCurve heightCurve_threadsafe_West = new AnimationCurve(westSettings.heightCurve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int i = 0; i < vertexWidth; i++)
        {
            for (int j = 0; j < vertexHeight; j++)
            {

                float x = (i - vertexWidth / 2) * worldScale / vertexScale + region.localisation.x;
                float z = (j - vertexHeight / 2) * worldScale / vertexScale + region.localisation.z;

                Vector3 position = new Vector3(x, 0, -z);
                Vector3 eastRegion = region.east.localisation;
                Vector3 westRegion = region.west.localisation;

                float distanceEastCenter = Vector3.Distance(centre, eastRegion);
                float distanceWestCenter = Vector3.Distance(centre, westRegion);

                float distanceEast = Vector3.Distance(position, eastRegion);
                float distanceWest = Vector3.Distance(position, westRegion);

                float valueCenter = valuesCenter[i, j];
                float valueEast = valuesEast[i, j];
                float valueWest = valuesWest[i, j];



                float valueMultiCenter = valueCenter * heightCurve_threadsafe_Center.Evaluate(valueCenter) * centerSettings.heightMultiplier;
                float valueMultiEast = valueEast * heightCurve_threadsafe_East.Evaluate(valueEast) * eastSettings.heightMultiplier;
                float valueMultiWest = valueWest * heightCurve_threadsafe_West.Evaluate(valueWest) * westSettings.heightMultiplier;

                if (i==j && i % 40  == 0)
                {

                    Debug.Log("height at " + position + ": centre " + valueMultiCenter + " east " + valueMultiEast + " lerp e" + distanceEast + " ec" + distanceEastCenter + " e/c" + distanceEast / distanceEastCenter);
                }
                float lerpEast = Mathf.Clamp(position.magnitude / distanceEastCenter, 0, 1);
                float lerpWest = Mathf.Clamp(position.magnitude / distanceWestCenter, 0, 1);

                float heightEast = Mathf.Lerp(valueMultiEast, valueMultiCenter, 1-lerpEast);
                float heighWest = Mathf.Lerp(valueMultiWest, valueMultiCenter, 1-lerpWest);

                float heightData;

                if (distanceEast > distanceWest)
                {
                    heightData = heightEast;
                }
                else
                {
                    heightData = heighWest;
                }

                //heightData = (heightEast + heighWest) /2;


                if (heightData > maxValue)
                {
                    maxValue = heightData;
                }
                if (heightData < minValue)
                {
                    minValue = heightData;
                }

                values[i, j] = heightData;

            }
        }

        return new HeightMap(values, minValue, maxValue);
    }

    public static HeightMap GenerateHeightMap(MeshSettings meshSettings, HeightMapSettings settings, Vector2 sampleCentre)
    {
        int width = meshSettings.numVertsPerLine;
        int height = meshSettings.numVertsPerLine;
        float[,] values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCentre);

        AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                values[i, j] *= heightCurve_threadsafe.Evaluate(values[i, j]) * settings.heightMultiplier;

                if (values[i, j] > maxValue)
                {
                    maxValue = values[i, j];
                }
                if (values[i, j] < minValue)
                {
                    minValue = values[i, j];
                }
            }
        }

        return new HeightMap(values, minValue, maxValue);
    }

}

public struct HeightMap
{
    public readonly float[,] values;
    public readonly float minValue;
    public readonly float maxValue;

    public HeightMap(float[,] values, float minValue, float maxValue)
    {
        this.values = values;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}

