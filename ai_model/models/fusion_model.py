import torch
import torch.nn as nn
from .image_encoder import ImageEncoder
from .sensor_encoder import SensorEncoder


class FusionModel(nn.Module):
    """멀티모달 융합 모델 — 이미지(1280) + 센서(64) → 4클래스 분류

    분류 클래스: 0=정상, 1=병해, 2=수분부족, 3=성장단계
    """

    NUM_CLASSES = 4
    CLASS_NAMES = ["healthy", "disease", "drought", "growth_stage"]

    def __init__(
        self,
        pretrained_image: bool = True,
        freeze_backbone: bool = False,
        dropout_rate: float = 0.3,
    ):
        super().__init__()
        self.image_encoder = ImageEncoder(pretrained=pretrained_image, freeze_backbone=freeze_backbone)
        self.sensor_encoder = SensorEncoder()

        fused_dim = self.image_encoder.feature_dim + self.sensor_encoder.feature_dim  # 1280 + 64 = 1344

        self.classifier = nn.Sequential(
            nn.Linear(fused_dim, 512),
            nn.ReLU(),
            nn.Dropout(p=dropout_rate),
            nn.Linear(512, self.NUM_CLASSES),
        )

    def forward(self, image: torch.Tensor, sensor: torch.Tensor) -> torch.Tensor:
        """
        Args:
            image:  (B, 3, H, W) — 정규화된 RGB 이미지
            sensor: (B, 4)       — [온도, 습도, 토양수분, 조도]
        Returns:
            (B, 4) — 클래스별 logit (softmax 미적용, CrossEntropyLoss와 함께 사용)
        """
        img_feat = self.image_encoder(image)
        sen_feat = self.sensor_encoder(sensor)
        fused = torch.cat([img_feat, sen_feat], dim=1)
        return self.classifier(fused)

    def predict(self, image: torch.Tensor, sensor: torch.Tensor):
        """추론 전용 — 클래스 인덱스와 확률 반환"""
        self.eval()
        with torch.no_grad():
            logits = self.forward(image, sensor)
            probs = torch.softmax(logits, dim=1)
            class_idx = torch.argmax(probs, dim=1)
        return class_idx, probs
