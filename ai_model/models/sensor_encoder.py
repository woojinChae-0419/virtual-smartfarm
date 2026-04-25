import torch
import torch.nn as nn


class SensorEncoder(nn.Module):
    """MLP 3층 센서 인코더 — 4개 센서값 → 64-dim feature 출력

    입력 센서: 온도(°C), 습도(%), 토양수분(%), 조도(lux)
    """

    def __init__(self, input_dim: int = 4, hidden_dim: int = 128, output_dim: int = 64):
        super().__init__()
        self.encoder = nn.Sequential(
            nn.Linear(input_dim, hidden_dim),
            nn.BatchNorm1d(hidden_dim),
            nn.ReLU(),
            nn.Linear(hidden_dim, hidden_dim),
            nn.BatchNorm1d(hidden_dim),
            nn.ReLU(),
            nn.Linear(hidden_dim, output_dim),
            nn.ReLU(),
        )
        self.feature_dim = output_dim

    def forward(self, x: torch.Tensor) -> torch.Tensor:
        """
        Args:
            x: (B, 4) — [온도, 습도, 토양수분, 조도] 정규화된 센서값
        Returns:
            (B, 64) — 센서 feature vector
        """
        return self.encoder(x)
