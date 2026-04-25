import json
from fastapi import FastAPI, File, Form, UploadFile
from fastapi.middleware.cors import CORSMiddleware
import uvicorn

app = FastAPI(
    title="Virtual Smart Farm API",
    description="Unity 3D 가상 온실 ↔ PyTorch AI 모델 연결 서버",
    version="0.1.0",
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/health")
def health_check():
    return {"status": "ok"}


@app.post("/predict")
async def predict(
    image: UploadFile = File(..., description="식물 이미지 파일 (jpg/png)"),
    sensor_data: str = Form(..., description='센서 JSON: {"temperature":25.0,"humidity":60.0,"soil_moisture":45.0,"illuminance":3000.0}'),
):
    """식물 상태 분류 추론

    - **image**: Unity에서 촬영한 가상 식물 이미지
    - **sensor_data**: 가상 센서 JSON (온도/습도/토양수분/조도)

    Returns:
        분류 결과 (class, confidence, probabilities)
    """
    # TODO: 센서 데이터 파싱 및 검증
    sensors = json.loads(sensor_data)

    # TODO: 이미지 바이트 읽기 및 전처리
    image_bytes = await image.read()

    # TODO: 실제 AI 모델 추론으로 교체
    # from inference import predict as ai_predict
    # result = ai_predict(image_bytes, sensors)

    # 더미 응답
    return {
        "class": "healthy",
        "class_id": 0,
        "confidence": 0.95,
        "probabilities": {
            "healthy": 0.95,
            "disease": 0.02,
            "drought": 0.02,
            "growth_stage": 0.01,
        },
        "sensor_data": sensors,
    }


if __name__ == "__main__":
    uvicorn.run("api_server:app", host="0.0.0.0", port=8000, reload=True)
