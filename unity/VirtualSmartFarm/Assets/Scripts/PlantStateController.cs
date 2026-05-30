using UnityEngine;
using System.Collections.Generic;

public class PlantStateController : MonoBehaviour
{
    [Header("진단 결과 통계 (실시간)")]
    public int healthyCount = 0;
    public int diseaseCount = 0;
    public int waterShortageCount = 0;
    public int growthStageCount = 0;

    [Header("AI 통계")]
    public float aiAvgConfidence = 0f;
    public float ruleAiAgreement = 0f;

    private GreenhouseGenerator farm;
    private PlantResponse responseManager;
    private Dictionary<int, string> previousStates = new Dictionary<int, string>();
    private Dictionary<int, Renderer[]> cachedLeaves = new Dictionary<int, Renderer[]>();

    void Start()
    {
        farm = FindAnyObjectByType<GreenhouseGenerator>();
        responseManager = GetComponent<PlantResponse>();
    }

    Renderer[] GetLeafRenderers(int plantId, GameObject plant)
    {
        if (cachedLeaves.ContainsKey(plantId)) return cachedLeaves[plantId];
        List<Renderer> leaves = new List<Renderer>();
        foreach (var r in plant.GetComponentsInChildren<Renderer>())
        {
            if (r.gameObject.name.StartsWith("Leaf"))
                leaves.Add(r);
        }
        Renderer[] arr = leaves.ToArray();
        cachedLeaves[plantId] = arr;
        return arr;
    }

    public void ApplyDiagnoses(BatchDiagnosis batch)
    {
        if (farm == null || farm.plants == null) return;

        healthyCount = 0;
        diseaseCount = 0;
        waterShortageCount = 0;
        growthStageCount = 0;
        aiAvgConfidence = batch.avg_ai_confidence;
        ruleAiAgreement = batch.agreement_rate;

        foreach (var diag in batch.diagnoses)
        {
            if (diag.plant_id < 0 || diag.plant_id >= farm.plants.Count) continue;
            GameObject plant = farm.plants[diag.plant_id];
            if (plant == null) continue;

            Renderer[] leaves = GetLeafRenderers(diag.plant_id, plant);
            Color color = new Color(diag.color_r, diag.color_g, diag.color_b);
            foreach (var r in leaves)
                r.material.color = color;

            string prevState = previousStates.ContainsKey(diag.plant_id) ? previousStates[diag.plant_id] : "unknown";
            if (prevState != diag.plant_class)
            {
                LogStateChange(diag.plant_id, prevState, diag.plant_class, diag);
                if (responseManager != null)
                    responseManager.TriggerResponse(diag.plant_id, diag.plant_class, plant);
                previousStates[diag.plant_id] = diag.plant_class;
            }

            switch (diag.plant_class)
            {
                case "healthy": healthyCount++; break;
                case "disease": diseaseCount++; break;
                case "water_shortage": waterShortageCount++; break;
                case "growth_stage": growthStageCount++; break;
            }
        }
    }

    void LogStateChange(int id, string prev, string now, PlantDiagnosis diag)
    {
        if (EventLogger.Instance == null) return;
        if (prev == "unknown") return;

        string aiTag = diag.ai_agrees ? "" : $" [AI:{diag.ai_class} {diag.ai_confidence:F2}]";
        switch (now)
        {
            case "water_shortage":
                EventLogger.Instance.Log($"Plant_{id:D2} 수분부족 감지 → 자동 급수{aiTag}");
                break;
            case "disease":
                EventLogger.Instance.Log($"Plant_{id:D2} 병해 감지 → 살균 처리{aiTag}");
                break;
            case "healthy":
                if (prev == "water_shortage") EventLogger.Instance.Log($"Plant_{id:D2} 수분 회복 완료{aiTag}");
                else if (prev == "disease") EventLogger.Instance.Log($"Plant_{id:D2} 병해 처리 완료{aiTag}");
                break;
        }
    }
}
