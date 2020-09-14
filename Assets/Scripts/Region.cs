using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Region
{
    public HeightMapSettings heightMapSettings;

    public Region east;

    public Region west;

    [NonSerialized]
    public Vector3 localisation;

    public float scale = 1;

    public float power = 2;

    public Region(HeightMapSettings heightMapSettings, Region east, Region west, Vector3 localisation, float power)
    {
        this.heightMapSettings = heightMapSettings;
        this.east = east;
        this.west = west;
        this.localisation = localisation;
        this.power = power;
    }

    public Region(HeightMapSettings heightMapSettings, Vector3 localisation)
    {
        this.heightMapSettings = heightMapSettings;
        this.localisation = localisation;
    }

}