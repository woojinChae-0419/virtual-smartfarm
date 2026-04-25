# Unity 씬 구성 가이드

## 사용 버전

- **Unity 6 LTS** (6000.4.4f1)
- 렌더 파이프라인: Built-in Render Pipeline

---

## 씬 구성 (SampleScene)

| 오브젝트 이름 | 타입 | 역할 |
|--------------|------|------|
| `Ground` | Plane | 온실 바닥면 |
| `Wall_Front` | Cube | 앞면 벽 |
| `Wall_Back` | Cube | 뒷면 벽 |
| `Wall_Left` | Cube | 왼쪽 벽 |
| `Wall_Right` | Cube | 오른쪽 벽 |
| `FarmGenerator` | Empty GameObject | GreenhouseGenerator.cs 부착 |

---

## GreenhouseGenerator.cs — 자동 생성 스크립트

[Assets/Scripts/GreenhouseGenerator.cs](../unity/VirtualSmartFarm/Assets/Scripts/GreenhouseGenerator.cs)

### 동작 원리

`Start()` 호출 시 `GenerateFarm()`이 실행되어 베드와 식물을 동적으로 생성합니다.

```
GenerateFarm()
├── CreateBed(z, bedIndex)         — Cube 프리미티브로 흙 베드 생성
└── CreatePlantsOnBed(z, bedIndex) — Capsule 프리미티브로 식물 생성
```

### 인스펙터 파라미터

| 파라미터 | 기본값 | 설명 |
|---------|--------|------|
| `bedCount` | 4 | 베드(밭) 개수 |
| `plantsPerBed` | 12 | 베드 1개당 식물 수 |
| `bedLength` | 16.0 | 베드 길이 (Unity 단위) |
| `bedWidth` | 1.2 | 베드 폭 |
| `bedHeight` | 0.2 | 베드 높이 |
| `bedSpacing` | 3.0 | 베드 간격 |
| `plantHeight` | 0.8 | 식물 높이 |
| `plantScale` | 0.3 | 식물 폭 스케일 |
| `soilMaterial` | M_Soil | 베드에 적용할 머티리얼 |
| `plantMaterial` | M_Plant | 식물에 적용할 머티리얼 |

### 생성 결과

```
총 식물 수 = bedCount × plantsPerBed = 4 × 12 = 48개
```

각 식물은 `Plant_{bedIndex}_{plantIndex}` 형식으로 네이밍됩니다.  
모든 오브젝트는 `FarmGenerator`의 자식으로 계층 구조가 정리됩니다.

---

## 머티리얼

| 파일 | 색상 | 적용 대상 |
|------|------|----------|
| `M_Soil.mat` | 갈색 (RGB: ~0.5, 0.3, 0.1) | 베드 (흙) |
| `M_Plant.mat` | 초록 (RGB: ~0.2, 0.7, 0.2) | 식물 (캡슐) |

---

## 추후 작업

- [ ] **센서 시뮬레이션**: 각 베드별 온도/습도/토양수분/조도 가상 센서값 생성
- [ ] **AI 서버 통신**: `UnityWebRequest`로 `/predict` 엔드포인트 호출 (C# 코루틴)
- [ ] **식물 상태 시각화**: 진단 결과(정상/병해/수분부족)에 따라 식물 색상 변경
- [ ] **카메라 캡처**: `RenderTexture`로 각 식물 이미지 캡처 후 서버 전송
- [ ] **UI 패널**: Canvas에 실시간 진단 결과 표시 (클래스명 + 신뢰도)
