from ultralytics import YOLO

# Load a model
model = YOLO("models/yolo11s.pt")  # load a pretrained model (recommended for training)

if __name__ == '__main__':
    # Train the model
    results = model.train(data="my_dataset/data.yaml",  epochs=20, imgsz=416,  rect = True, project='models', name='yolo11_demo1', workers = 0)