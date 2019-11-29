using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainGeneratorLerp : TerrainGenerator
{

    [SerializeField]
    public Region region;
    protected Dictionary<Vector2, TerrainChunkLerp> terrainChunkDictionaryLerp = new Dictionary<Vector2, TerrainChunkLerp>();
    protected List<TerrainChunkLerp> visibleTerrainChunksLerp = new List<TerrainChunkLerp>();

    // Use this for initialization
    override public void Start()
    {
        textureSettings.ApplyToMaterial(mapMaterial);
        textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        float maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshWorldSize);

        UpdateVisibleChunks();
    }

    // Update is called once per frame
    override public void Update()
    {
        base.Update();
    }

    override public void UpdateVisibleChunks()
    {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        for (int i = visibleTerrainChunksLerp.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunksLerp[i].coord);
            visibleTerrainChunksLerp[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                {
                    if (terrainChunkDictionaryLerp.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunkDictionaryLerp[viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else
                    {
                        TerrainChunkLerp newChunk = new TerrainChunkLerp(viewedChunkCoord, region, meshSettings, detailLevels, colliderLODIndex, transform, viewer, mapMaterial);
                        terrainChunkDictionaryLerp.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        newChunk.Load();
                    }
                }

            }
        }
    }

    public void OnTerrainChunkVisibilityChanged(TerrainChunkLerp chunk, bool isVisible)
    {
        if (isVisible)
        {
            visibleTerrainChunksLerp.Add(chunk);
        }
        else
        {
            visibleTerrainChunksLerp.Remove(chunk);
        }
    }



}

