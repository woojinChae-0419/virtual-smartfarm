"""Virtual Smart Farm FastAPI Server"""
import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent.parent / 'ai_model'))

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List
import random
import uvicorn

from plant_ai import PlantAI

app = FastAPI(title="Virtual Smart Farm API")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

# PyTorch 모델 초기화 (서버 시작 시 1회)
print("=" * 60)
print("[Server] PyTorch AI 엔진 초기화...")
ai_engine = PlantAI()
print("[Server] AI 엔진 준비 완료")
print("=" * 60)


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
    plant_class: str       # 룰 기반 결과 (Unity 색상 결정)
    confidence: float
    color_r: float
    color_g: float
    color_b: float
    ai_class: str          # PyTorch 결과
    ai_confidence: float
    ai_agrees: bool        # 룰과 AI 일치 여부


class BatchDiagnosis(BaseModel):
    diagnoses: List[PlantDiagnosis]
    avg_ai_confidence: float
    agreement_rate: float  # 룰-AI 일치율


CLASS_COLORS = {
    "healthy":        (0.30, 0.70, 0.30),
    "disease":        (0.90, 0.20, 0.20),
    "water_shortage": (0.95, 0.85, 0.20),
    "growth_stage":   (0.60, 0.90, 0.40),
}


def classify_rule_based(reading: SensorReading):
    if reading.soil_moisture < 30:
        return "water_shortage", 0.90 + random.uniform(-0.05, 0.05)
    if reading.temperature > 32 or reading.humidity > 90:
        return "disease", 0.85 + random.uniform(-0.05, 0.05)
    if reading.light > 700 and 20 < reading.temperature < 28:
        return "growth_stage", 0.80 + random.uniform(-0.05, 0.05)
    return "healthy", 0.92 + random.uniform(-0.05, 0.05)


@app.get("/health")
def health():
    return {"status": "ok", "ai_loaded": True}


@app.post("/predict_batch", response_model=BatchDiagnosis)
def predict_batch(batch: BatchSensorReading):
    diagnoses = []
    sum_ai_conf = 0.0
    agree_count = 0

    for reading in batch.readings:
        # 룰 기반
        rule_class, rule_conf = classify_rule_based(reading)
        r, g, b = CLASS_COLORS[rule_class]

        # PyTorch AI
        ai_class, ai_conf = ai_engine.predict(
            reading.temperature, reading.humidity,
            reading.soil_moisture, reading.light
        )

        agrees = (rule_class == ai_class)
        if agrees:
            agree_count += 1
        sum_ai_conf += ai_conf

        diagnoses.append(PlantDiagnosis(
            plant_id=reading.plant_id,
            plant_class=rule_class,
            confidence=round(rule_conf, 3),
            color_r=r, color_g=g, color_b=b,
            ai_class=ai_class,
            ai_confidence=round(ai_conf, 3),
            ai_agrees=agrees,
        ))

    n = max(len(batch.readings), 1)
    return BatchDiagnosis(
        diagnoses=diagnoses,
        avg_ai_confidence=round(sum_ai_conf / n, 3),
        agreement_rate=round(agree_count / n, 3),
    )


if __name__ == "__main__":
    uvicorn.run(app, host="127.0.0.1", port=8000)
