from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List
import random
import uvicorn

app = FastAPI(title="Virtual Smart Farm API")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)


class SensorReading(BaseModel):
    plant_id: int
    temperature: float
    humidity: float
    soil_moisture: float
    light: float


class BatchSensorReading(BaseModel):
    readings: List[SensorReading]


class PlantDiagnosis(BaseModel):
    plant_id: int
    plant_class: str
    confidence: float
    color_r: float
    color_g: float
    color_b: float


class BatchDiagnosis(BaseModel):
    diagnoses: List[PlantDiagnosis]


CLASS_COLORS = {
    "healthy":        (0.30, 0.70, 0.30),
    "disease":        (0.90, 0.20, 0.20),
    "water_shortage": (0.95, 0.85, 0.20),
    "growth_stage":   (0.60, 0.90, 0.40),
}


def classify_rule_based(reading: SensorReading):
    """룰 기반 분류 (1학기 시제품 단계, 2학기에 학습 모델로 교체 예정)"""
    if reading.soil_moisture < 30:
        return "water_shortage", 0.90 + random.uniform(-0.05, 0.05)
    if reading.temperature > 32 or reading.humidity > 90:
        return "disease", 0.85 + random.uniform(-0.05, 0.05)
    if reading.light > 700 and 20 < reading.temperature < 28:
        return "growth_stage", 0.80 + random.uniform(-0.05, 0.05)
    return "healthy", 0.92 + random.uniform(-0.05, 0.05)


@app.get("/health")
def health():
    return {"status": "ok"}


@app.post("/predict_batch", response_model=BatchDiagnosis)
def predict_batch(batch: BatchSensorReading):
    diagnoses = []
    for reading in batch.readings:
        plant_class, conf = classify_rule_based(reading)
        r, g, b = CLASS_COLORS[plant_class]
        diagnoses.append(PlantDiagnosis(
            plant_id=reading.plant_id,
            plant_class=plant_class,
            confidence=round(conf, 3),
            color_r=r, color_g=g, color_b=b,
        ))
    return BatchDiagnosis(diagnoses=diagnoses)


if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)