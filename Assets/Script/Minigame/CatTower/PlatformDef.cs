using System;
using UnityEngine;

public enum PlatformType
{
    Static,
    Moveable,
    Seesaw,
    Goal
}

[Serializable]
public struct PlatformDef
{
    public Vector2 localPos;   // セクション原点からの相対座標
    public Vector2 size;       // 幅・高さ（units / タイル数相当）
    public PlatformType type;

    // Optional: 動く足場
    public float moveAmplitude;  // 例: 1.5
    public float movePeriod;     // 例: 2.0

    // Optional: 崩れる足場
    public float crumbleDelay;   // 例: 0.5
}
