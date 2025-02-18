from ultralytics import YOLO

# Load a model
model = YOLO("models/yolo11_rose5/weights/best.pt")  # load a custom model

if __name__ == '__main__':
    # Validate the model
    metrics = model.val()  # no arguments needed, dataset and settings remembered
    print(metrics.box.map)