using UnityEngine;
using System.Collections.Generic;

public class SensorSimulator : MonoBehaviour
{
    [Header("기준값 (전체 평균)")]
    public float baseTemperature = 25f;
    public float baseHumidity = 60f;
    public float baseSoilMoisture = 50f;
    public float baseLight = 800f;

    [Header("식물별 고유 편차 (개성)")]
    public float tempVariance = 5f;
    public float humidityVariance = 12f;
    public float soilVariance = 18f;
    public float lightVariance = 150f;

    [Header("실시간 작은 흔들림 (자연 변동)")]
    public float jitter = 0.5f;

    [Header("평균값 (UI 표시용)")]
    public float lastAvgTemp;
    public float lastAvgHumidity;
    public float lastAvgSoil;
    public float lastAvgLight;

    private GreenhouseGenerator farm;
    private Dictionary<int, Vector4> plantOffsets = new Dictionary<int, Vector4>();

    void Start() { farm = FindAnyObjectByType<GreenhouseGenerator>(); }

    public BatchSensorReading GenerateReadings()
    {
        BatchSensorReading batch = new BatchSensorReading();
        if (farm == null) return batch;

        int total = farm.bedCount * farm.plantsPerBed;
        float sumT = 0, sumH = 0, sumS = 0, sumL = 0;

        for (int i = 0; i < total; i++)
        {
            if (!plantOffsets.ContainsKey(i))
            {
                plantOffsets[i] = new Vector4(
                    Random.Range(-tempVariance, tempVariance),
                    Random.Range(-humidityVariance, humidityVariance),
                    Random.Range(-soilVariance, soilVariance),
                    Random.Range(-lightVariance, lightVariance)
                );
            }
            Vector4 off = plantOffsets[i];

            SensorReading r = new SensorReading
            {
                plant_id = i,
                temperature = baseTemperature + off.x + Random.Range(-jitter, jitter),
                humidity = baseHumidity + off.y + Random.Range(-jitter * 2, jitter * 2),
                soil_moisture = baseSoilMoisture + off.z + Random.Range(-jitter * 2, jitter * 2),
                light = baseLight + off.w + Random.Range(-jitter * 10, jitter * 10),
            };
            batch.readings.Add(r);
            sumT += r.temperature; sumH += r.humidity; sumS += r.soil_moisture; sumL += r.light;
        }

        if (total > 0)
        {
            lastAvgTemp = sumT / total;
            lastAvgHumidity = sumH / total;
            lastAvgSoil = sumS / total;
            lastAvgLight = sumL / total;
        }
        return batch;
    }
}
