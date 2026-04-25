import torch
import torch.nn as nn
from torch.utils.data import DataLoader
from models import FusionModel

# TODO: 하이퍼파라미터 설정
EPOCHS = 50
BATCH_SIZE = 32
LEARNING_RATE = 1e-4
DEVICE = torch.device("cuda" if torch.cuda.is_available() else "cpu")
MODEL_SAVE_PATH = "checkpoints/fusion_model_best.pt"


def get_dataloaders():
    # TODO: PlantDataset 클래스 구현 (이미지 경로 + 센서 CSV + 레이블)
    # TODO: train/val split (80/20)
    # TODO: 이미지 augmentation (RandomHorizontalFlip, ColorJitter 등)
    train_loader = None
    val_loader = None
    return train_loader, val_loader


def train_epoch(model, loader, optimizer, criterion):
    model.train()
    total_loss, correct = 0.0, 0
    for images, sensors, labels in loader:
        images, sensors, labels = images.to(DEVICE), sensors.to(DEVICE), labels.to(DEVICE)
        optimizer.zero_grad()
        logits = model(images, sensors)
        loss = criterion(logits, labels)
        loss.backward()
        optimizer.step()
        total_loss += loss.item() * images.size(0)
        correct += (logits.argmax(1) == labels).sum().item()
    return total_loss / len(loader.dataset), correct / len(loader.dataset)


def validate(model, loader, criterion):
    model.eval()
    total_loss, correct = 0.0, 0
    with torch.no_grad():
        for images, sensors, labels in loader:
            images, sensors, labels = images.to(DEVICE), sensors.to(DEVICE), labels.to(DEVICE)
            logits = model(images, sensors)
            loss = criterion(logits, labels)
            total_loss += loss.item() * images.size(0)
            correct += (logits.argmax(1) == labels).sum().item()
    return total_loss / len(loader.dataset), correct / len(loader.dataset)


def main():
    model = FusionModel(pretrained_image=True, freeze_backbone=False).to(DEVICE)
    optimizer = torch.optim.AdamW(model.parameters(), lr=LEARNING_RATE, weight_decay=1e-4)
    criterion = nn.CrossEntropyLoss()
    # TODO: LR 스케줄러 추가 (CosineAnnealingLR 등)
    scheduler = None

    train_loader, val_loader = get_dataloaders()

    best_val_acc = 0.0
    for epoch in range(1, EPOCHS + 1):
        # TODO: train_loader, val_loader 준비 후 아래 주석 해제
        # train_loss, train_acc = train_epoch(model, train_loader, optimizer, criterion)
        # val_loss, val_acc = validate(model, val_loader, criterion)
        # print(f"[{epoch:03d}/{EPOCHS}] train_loss={train_loss:.4f} acc={train_acc:.4f} | val_loss={val_loss:.4f} acc={val_acc:.4f}")
        # if val_acc > best_val_acc:
        #     best_val_acc = val_acc
        #     torch.save(model.state_dict(), MODEL_SAVE_PATH)
        pass

    print(f"학습 완료. Best val acc: {best_val_acc:.4f}")


if __name__ == "__main__":
    main()
