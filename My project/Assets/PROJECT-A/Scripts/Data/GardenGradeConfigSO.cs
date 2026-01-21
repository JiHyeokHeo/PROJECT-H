using System;
using System.Collections.Generic;
using UnityEngine;

public enum EGardenGrade
{
    None = 0,
    Sprout,
    Bud,
    Bloom,
    Blossom,
    Radiant,
    Mythic
}


[CreateAssetMenu(menuName = "TST/Garden Grade Config")]
public class GardenGradeConfigSO : ScriptableObject
{
    [Serializable]
    public class GradeStep
    {
        public EGardenGrade grade;
        public int minScore;
    }

    [Tooltip("minScore 오름차순 정렬 권장. 점수에 따라 가장 높은 minScore<=score 단계가 현재 등급")]
    public List<GradeStep> steps = new();

    public void Evaluate(int score, out EGardenGrade current, out EGardenGrade next, out float progress01, out int currentMin, out int nextMin)
    {
        current = EGardenGrade.None;
        next = EGardenGrade.None;
        progress01 = 0f;
        currentMin = 0;
        nextMin = 0;

        if (steps == null || steps.Count == 0)
            return;

        // 현재 등급: score 이하 중 가장 높은 minScore
        int curIndex = 0;
        for (int i = 0; i < steps.Count; i++)
        {
            if (steps[i] == null) continue;
            if (score >= steps[i].minScore)
                curIndex = i;
        }

        current = steps[curIndex].grade;
        currentMin = steps[curIndex].minScore;

        bool hasNext = curIndex + 1 < steps.Count;
        if (!hasNext)
        {
            next = current;
            nextMin = currentMin;
            progress01 = 1f;
            return;
        }

        next = steps[curIndex + 1].grade;
        nextMin = steps[curIndex + 1].minScore;

        int denom = Mathf.Max(1, nextMin - currentMin);
        progress01 = Mathf.Clamp01((score - currentMin) / (float)denom);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (steps == null) return;
        steps.Sort((a, b) => a.minScore.CompareTo(b.minScore));
    }
#endif
}
