using System;
using UnityEngine;

[Serializable]
public class EnemyData
{
    public string Label;

    [Header("SETUP")]
    public Sprite sprite;
    public float scaleCoef = 1f;
    public Color color = Color.white;

    [Header("STATS")]
    public int pv = 1;
    public int damage = 1;
    public float speed = 2f;

    [Header("BEHAVIOR")]
    public float durationIDLE = 1f;
    public float detectionRange = 10f;
    public float pursuitDuration = 5f;
    public float turnSpeed = 180f;
}
