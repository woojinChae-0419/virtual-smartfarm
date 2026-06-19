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
    private Dictionary<int, PlantLabel> cachedLabels = new Dictionary<int, PlantLabel>();

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

    PlantLabel GetLabel(int plantId, GameObject plant)
    {
        if (cachedLabels.ContainsKey(plantId)) return cachedLabels[plantId];
        PlantLabel lbl = plant.GetComponent<PlantLabel>();
        cachedLabels[plantId] = lbl;
        return lbl;
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

            // 잎사귀 색
            Renderer[] leaves = GetLeafRenderers(diag.plant_id, plant);
            Color color = new Color(diag.color_r, diag.color_g, diag.color_b);
            foreach (var r in leaves)
                r.material.color = color;

            // 라벨 업데이트 (매번)
            PlantLabel lbl = GetLabel(diag.plant_id, plant);
            if (lbl != null) lbl.SetState(diag.plant_class);

            // 상태 변화 감지 → 효과 트리거
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

        switch (now)
        {
            case "water_shortage":
                EventLogger.Instance.Log($"Plant_{id:D2} 수분부족 → 스프링클러 급수 시작");
                break;
            case "disease":
                EventLogger.Instance.Log($"Plant_{id:D2} 병해 → 살균제 분무 시작");
                break;
            case "growth_stage":
                EventLogger.Instance.Log($"Plant_{id:D2} 성장 단계 → LED 점등");
                break;
            case "healthy":
                if (prev == "water_shortage") EventLogger.Instance.Log($"Plant_{id:D2} 수분 회복 완료");
                else if (prev == "disease") EventLogger.Instance.Log($"Plant_{id:D2} 병해 처리 완료");
                else if (prev == "growth_stage") EventLogger.Instance.Log($"Plant_{id:D2} 성장 단계 완료");
                break;
        }
    }
}
