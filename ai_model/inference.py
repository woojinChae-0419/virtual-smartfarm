import torch
import numpy as np
from PIL import Image
from torchvision import transforms
from models import FusionModel

DEVICE = torch.device("cuda" if torch.cuda.is_available() else "cpu")
MODEL_PATH = "checkpoints/fusion_model_best.pt"

IMAGE_TRANSFORM = transforms.Compose([
    transforms.Resize((224, 224)),
    transforms.ToTensor(),
    transforms.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225]),
])


def load_model(model_path: str = MODEL_PATH) -> FusionModel:
    model = FusionModel(pretrained_image=False)
    model.load_state_dict(torch.load(model_path, map_location=DEVICE))
    model.to(DEVICE)
    model.eval()
    return model


def preprocess_image(image_path: str) -> torch.Tensor:
    img = Image.open(image_path).convert("RGB")
    return IMAGE_TRANSFORM(img).unsqueeze(0).to(DEVICE)


def preprocess_sensors(temperature: float, humidity: float, soil_moisture: float, illuminance: float) -> torch.Tensor:
    # TODO: 학습 시 사용한 정규화 통계값으로 교체
    sensor_array = np.array([[temperature, humidity, soil_moisture, illuminance]], dtype=np.float32)
    return torch.from_numpy(sensor_array).to(DEVICE)


def predict(image_path: str, temperature: float, humidity: float, soil_moisture: float, illuminance: float) -> dict:
    """단일 샘플 추론

    Returns:
        {"class_name": str, "class_id": int, "confidence": float, "probabilities": list}
    """
    model = load_model()
    image_tensor = preprocess_image(image_path)
    sensor_tensor = preprocess_sensors(temperature, humidity, soil_moisture, illuminance)

    class_idx, probs = model.predict(image_tensor, sensor_tensor)

    idx = class_idx.item()
    return {
        "class_name": FusionModel.CLASS_NAMES[idx],
        "class_id": idx,
        "confidence": round(probs[0][idx].item(), 4),
        "probabilities": {name: round(p.item(), 4) for name, p in zip(FusionModel.CLASS_NAMES, probs[0])},
    }


if __name__ == "__main__":
    # TODO: 테스트 이미지 경로 및 센서값 입력
    result = predict(
        image_path="test_images/sample.jpg",
        temperature=25.0,
        humidity=60.0,
        soil_moisture=45.0,
        illuminance=3000.0,
    )
    print(result)
