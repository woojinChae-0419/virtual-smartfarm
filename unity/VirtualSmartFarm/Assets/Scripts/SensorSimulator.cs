using UnityEngine;
using System.Collections.Generic;

public class SensorSimulator : MonoBehaviour
{
    [Header("기준값 (이걸 조절해서 다양한 시나리오 시뮬레이션)")]
    public float baseTemperature = 25f;
    public float baseHumidity = 60f;
    public float baseSoilMoisture = 50f;
    public float baseLight = 800f;

    [Header("개별 식물 노이즈 범위")]
    public float tempNoise = 8f;
    public float humidityNoise = 20f;
    public float soilNoise = 35f;
    public float lightNoise = 250f;

    private GreenhouseGenerator farm;

    void Start()
    {
        farm = FindFirstObjectByType<GreenhouseGenerator>();
    }

    public BatchSensorReading GenerateReadings()
    {
        BatchSensorReading batch = new BatchSensorReading();
        if (farm == null) return batch;

        int total = farm.bedCount * farm.plantsPerBed;
        for (int i = 0; i < total; i++)
        {
            batch.readings.Add(new SensorReading
            {
                plant_id = i,
                temperature = baseTemperature + Random.Range(-tempNoise, tempNoise),
                humidity = baseHumidity + Random.Range(-humidityNoise, humidityNoise),
                soil_moisture = baseSoilMoisture + Random.Range(-soilNoise, soilNoise),
                light = baseLight + Random.Range(-lightNoise, lightNoise),
            });
        }
        return batch;
    }
}