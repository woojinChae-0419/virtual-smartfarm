"""PyTorch 식물 상태 분류기 - 학습 + 추론 통합 모듈"""
import os
import torch
import torch.nn as nn
import numpy as np
from pathlib import Path

CLASSES = ['healthy', 'disease', 'water_shortage', 'growth_stage']
MODEL_PATH = Path(__file__).parent / 'sensor_classifier.pt'
STATS_PATH = Path(__file__).parent / 'sensor_stats.npz'


class SensorClassifier(nn.Module):
    """4개 센서값 → 4클래스 분류 MLP"""
    def __init__(self, input_dim=4, hidden_dim=64, num_classes=4):
        super().__init__()
        self.net = nn.Sequential(
            nn.Linear(input_dim, hidden_dim),
            nn.ReLU(),
            nn.Dropout(0.2),
            nn.Linear(hidden_dim, hidden_dim),
            nn.ReLU(),
            nn.Linear(hidden_dim, num_classes)
        )

    def forward(self, x):
        return self.net(x)


def rule_label(temp, humidity, soil, light):
    """학습 데이터 라벨링용 룰"""
    if soil < 30:
        return 2  # water_shortage
    if temp > 32 or humidity > 90:
        return 1  # disease
    if light > 700 and 20 < temp < 28:
        return 3  # growth_stage
    return 0  # healthy


def generate_synthetic_data(n_samples=8000, noise_pct=0.05):
    """합성 센서 데이터 생성 (룰 기반 라벨 + 약간의 노이즈)"""
    X = np.random.uniform(
        low=[15, 30, 10, 300],
        high=[40, 95, 80, 1200],
        size=(n_samples, 4)
    )
    y = np.array([rule_label(*row) for row in X])
    # 5% 라벨 노이즈 (현실성)
    noise_idx = np.random.choice(n_samples, int(n_samples * noise_pct), replace=False)
    y[noise_idx] = np.random.randint(0, 4, len(noise_idx))
    return X.astype(np.float32), y.astype(np.int64)


def train_model():
    """모델 학습 + 저장"""
    print("[plant_ai] 학습 데이터 생성 중...")
    X, y = generate_synthetic_data()
    mean = X.mean(axis=0)
    std = X.std(axis=0) + 1e-6
    X_norm = (X - mean) / std

    X_t = torch.from_numpy(X_norm)
    y_t = torch.from_numpy(y)

    model = SensorClassifier()
    optimizer = torch.optim.Adam(model.parameters(), lr=1e-3)
    criterion = nn.CrossEntropyLoss()

    print("[plant_ai] 학습 시작 (50 epochs)...")
    model.train()
    batch_size = 128
    n = len(X_t)
    for epoch in range(50):
        perm = torch.randperm(n)
        total_loss = 0
        for i in range(0, n, batch_size):
            idx = perm[i:i + batch_size]
            optimizer.zero_grad()
            logits = model(X_t[idx])
            loss = criterion(logits, y_t[idx])
            loss.backward()
            optimizer.step()
            total_loss += loss.item()
        if (epoch + 1) % 10 == 0:
            with torch.no_grad():
                preds = model(X_t).argmax(dim=1)
                acc = (preds == y_t).float().mean().item()
            print(f"  Epoch {epoch + 1}/50 | Loss {total_loss:.2f} | Acc {acc:.4f}")

    torch.save(model.state_dict(), MODEL_PATH)
    np.savez(STATS_PATH, mean=mean, std=std)
    print(f"[plant_ai] 모델 저장 완료: {MODEL_PATH}")
    return model, mean, std


class PlantAI:
    """추론 엔진 - 서버에서 사용"""
    def __init__(self):
        if not MODEL_PATH.exists() or not STATS_PATH.exists():
            print("[plant_ai] 모델 파일 없음 → 학습 시작")
            self.model, self.mean, self.std = train_model()
        else:
            print(f"[plant_ai] 기존 모델 로드: {MODEL_PATH}")
            self.model = SensorClassifier()
            self.model.load_state_dict(torch.load(MODEL_PATH, map_location='cpu'))
            stats = np.load(STATS_PATH)
            self.mean = stats['mean']
            self.std = stats['std']
        self.model.eval()

    def predict(self, temperature, humidity, soil_moisture, light):
        x = np.array([[temperature, humidity, soil_moisture, light]], dtype=np.float32)
        x_norm = (x - self.mean) / self.std
        with torch.no_grad():
            logits = self.model(torch.from_numpy(x_norm.astype(np.float32)))
            probs = torch.softmax(logits, dim=1)
            pred_idx = int(probs.argmax(dim=1).item())
            confidence = float(probs[0, pred_idx].item())
        return CLASSES[pred_idx], confidence


if __name__ == "__main__":
    # 단독 실행 시 학습만 수행
    train_model()
