using UnityEngine;
using System.Collections.Generic;

public class PlantStateController : MonoBehaviour
{
    [Header("진단 결과 통계 (실시간)")]
    public int healthyCount = 0;
    public int diseaseCount = 0;
    public int waterShortageCount = 0;
    public int growthStageCount = 0;

    private GreenhouseGenerator farm;

    void Start()
    {
        farm = FindFirstObjectByType<GreenhouseGenerator>();
    }

    public void ApplyDiagnoses(BatchDiagnosis batch)
    {
        if (farm == null || farm.plants == null) return;

        healthyCount = 0;
        diseaseCount = 0;
        waterShortageCount = 0;
        growthStageCount = 0;

        foreach (var diag in batch.diagnoses)
        {
            if (diag.plant_id < 0 || diag.plant_id >= farm.plants.Count) continue;
            GameObject plant = farm.plants[diag.plant_id];
            if (plant == null) continue;

            Renderer renderer = plant.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(diag.color_r, diag.color_g, diag.color_b);
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
}