using UnityEngine;
using System.Collections.Generic;

public class SensorSimulator : MonoBehaviour
{
    [Header("기준값")]
    public float baseTemperature = 25f;
    public float baseHumidity = 60f;
    public float baseSoilMoisture = 50f;
    public float baseLight = 800f;

    [Header("개별 식물 노이즈 범위")]
    public float tempNoise = 8f;
    public float humidityNoise = 20f;
    public float soilNoise = 35f;
    public float lightNoise = 250f;

    [Header("평균값 (UI 표시용, 자동 계산)")]
    public float lastAvgTemp;
    public float lastAvgHumidity;
    public float lastAvgSoil;
    public float lastAvgLight;

    private GreenhouseGenerator farm;

    void Start() { farm = FindAnyObjectByType<GreenhouseGenerator>(); }

    public BatchSensorReading GenerateReadings()
    {
        BatchSensorReading batch = new BatchSensorReading();
        if (farm == null) return batch;

        int total = farm.bedCount * farm.plantsPerBed;
        float sumT = 0, sumH = 0, sumS = 0, sumL = 0;
        for (int i = 0; i < total; i++)
        {
            SensorReading r = new SensorReading
            {
                plant_id = i,
                temperature = baseTemperature + Random.Range(-tempNoise, tempNoise),
                humidity = baseHumidity + Random.Range(-humidityNoise, humidityNoise),
                soil_moisture = baseSoilMoisture + Random.Range(-soilNoise, soilNoise),
                light = baseLight + Random.Range(-lightNoise, lightNoise),
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
