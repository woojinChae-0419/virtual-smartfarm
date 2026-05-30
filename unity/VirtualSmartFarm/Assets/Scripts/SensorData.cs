using System;
using System.Collections.Generic;

[Serializable]
public class SensorReading
{
    public int plant_id;
    public float temperature;
    public float humidity;
    public float soil_moisture;
    public float light;
}

[Serializable]
public class BatchSensorReading
{
    public List<SensorReading> readings = new List<SensorReading>();
}

[Serializable]
public class PlantDiagnosis
{
    public int plant_id;
    public string plant_class;
    public float confidence;
    public float color_r;
    public float color_g;
    public float color_b;
}

[Serializable]
public class BatchDiagnosis
{
    public List<PlantDiagnosis> diagnoses = new List<PlantDiagnosis>();
}