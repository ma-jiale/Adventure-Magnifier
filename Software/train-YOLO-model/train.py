from ultralytics import YOLO

# Load a model
model = YOLO("yolo11s.pt")  # load a pretrained model (recommended for training)
data_set="my_dataset/data.yaml"
if __name__ == '__main__':
    # Train the model
    results = model.train(data=data_set,  epochs=50, imgsz=416,  rect = True, project='models', name='last_model', workers = 0)





