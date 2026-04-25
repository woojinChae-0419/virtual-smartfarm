import torch
import torch.nn as nn
from torchvision.models import efficientnet_b0, EfficientNet_B0_Weights


class ImageEncoder(nn.Module):
    """EfficientNet-B0 기반 이미지 인코더 — 1280-dim feature 출력"""

    def __init__(self, pretrained: bool = True, freeze_backbone: bool = False):
        super().__init__()
        weights = EfficientNet_B0_Weights.DEFAULT if pretrained else None
        backbone = efficientnet_b0(weights=weights)

        # classifier 제거, feature 추출만 사용
        self.features = backbone.features
        self.avgpool = backbone.avgpool
        self.feature_dim = 1280

        if freeze_backbone:
            for param in self.features.parameters():
                param.requires_grad = False

    def forward(self, x: torch.Tensor) -> torch.Tensor:
        """
        Args:
            x: (B, 3, H, W) — 정규화된 RGB 이미지
        Returns:
            (B, 1280) — 이미지 feature vector
        """
        x = self.features(x)
        x = self.avgpool(x)
        x = torch.flatten(x, 1)
        return x
