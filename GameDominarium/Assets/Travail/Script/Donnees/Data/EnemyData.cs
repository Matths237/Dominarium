using System;
using System.Reflection.Emit;
using UnityEngine;

[Serializable]
public class EnemyData
{
    public string Label;

    [Header("SETUP")]
    public Sprite sprite;
    public float scaleCoef;
    public Color color;

    [Header("STATS")]
    public int pv;
    public int damage;
    public float speed;

    [Header("STATS")]
    public float durationIDLE;

    
    
}