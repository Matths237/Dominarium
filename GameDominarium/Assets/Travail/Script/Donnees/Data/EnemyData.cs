using System;
using UnityEngine;

[Serializable]
public struct EnemyData
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

    [Header("BEHAVIOR")]
    public float durationIDLE;
    public float detectionRange;
    public float pursuitDuration;
    public float turnSpeed;

}