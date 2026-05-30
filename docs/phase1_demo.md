# Phase 1 데모 문서

## 구현 내용 요약

Unity와 Python FastAPI 서버 간의 실시간 통신을 구현하고, 센서 데이터를 기반으로 식물 상태를 4가지 클래스로 분류하여 Unity 씬에서 색상으로 시각화합니다.

## 시스템 데이터 흐름

```
Unity (SensorSimulator)
    ↓ 가상 센서 데이터 생성 (온도/습도/토양수분/광량)
Unity (APIClient)
    ↓ POST /predict_batch  (매 1초)
FastAPI 서버 (api_server.py)
    ↓ 룰 기반 4클래스 분류 + 색상 반환
Unity (PlantStateController)
    ↓ 식물 색상 업데이트
Unity 씬 (GreenhouseGenerator의 plants 리스트)
```

## API 명세

### POST /predict_batch

**요청 바디**
```json
{
  "readings": [
    {
      "plant_id": 0,
      "temperature": 25.3,
      "humidity": 62.1,
      "soil_moisture": 48.5,
      "light": 810.0
    }
  ]
}
```

**응답 바디**
```json
{
  "diagnoses": [
    {
      "plant_id": 0,
      "plant_class": "healthy",
      "confidence": 0.923,
      "color_r": 0.30,
      "color_g": 0.70,
      "color_b": 0.30
    }
  ]
}
```

### GET /health
서버 상태 확인. `{"status": "ok"}` 반환.

## 룰 기반 분류 로직 (4클래스 임계값)

| 클래스 | 조건 | 색상 |
|--------|------|------|
| water_shortage | 토양수분 < 30 | 노랑 (0.95, 0.85, 0.20) |
| disease | 온도 > 32 또는 습도 > 90 | 빨강 (0.90, 0.20, 0.20) |
| growth_stage | 광량 > 700 AND 20 < 온도 < 28 | 연두 (0.60, 0.90, 0.40) |
| healthy | 위 조건 모두 해당 없음 | 초록 (0.30, 0.70, 0.30) |

> 조건은 위에서 아래 순서로 평가 (water_shortage 우선)

## 실행 방법

### 1. 서버 실행
```bash
cd server
pip install -r ../ai_model/requirements.txt
python api_server.py
# http://localhost:8000/docs 에서 Swagger UI 확인 가능
```

### 2. Unity 실행
1. Unity에서 프로젝트 열기
2. SampleScene 로드
3. 빈 GameObject 생성 → `APIClient`, `SensorSimulator`, `PlantStateController` 컴포넌트 추가
4. Play 버튼 클릭
5. 식물 색상이 1초마다 업데이트됨

## 추후 작업 (2학기)

- 룰 기반 분류 → 학습된 AI 모델(CNN/MLP) 추론으로 교체
- 실제 센서 데이터 수신 (MQTT/WebSocket)
- 대시보드 UI 추가 (식물별 통계, 경고 알림)
- 데이터 로깅 및 히스토리 시각화