using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TST/Garden Score Config")]
public class GardenScoreConfigSO : ScriptableObject
{
    [Header("Base Diminishing Returns (same itemId)")]
    public List<float> repeatMultipliers = new() { 1f, 1f, 0.7f, 0.5f, 0.3f, 0.2f };

    [Header("Theme Synergy")]
    public int synergyGroupSize = 3;
    public int synergyUnit = 20;
    public int synergyTierCap = 4;

    [Header("Variety Bonus")]
    public int varietyUnit = 15;
    public int varietyCap = 8;
}

// 밸런스 파라미터
//repeat(도배 방지) 곱 배열: 예) [1, 1, 0.7, 0.5, 0.3, 0.2...]
//synergy:
//synergyGroupSize = 3
//synergyUnit = 20
//synergyTierCap = 4(12개 이상 캡)
//variety:
//varietyUnit = 15