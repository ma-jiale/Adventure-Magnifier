import cv2
from ultralytics import YOLO

# 加载模型
model = YOLO("models/yolo11s.pt")

def YOLO_detect(img):
    # 进行预测
    results = model.predict(source=img)
    detected_objects = []

    # 提取检测结果
    for result in results:
        boxes = result.boxes.xyxy  # 边界框坐标
        scores = result.boxes.conf  # 置信度分数
        classes = result.boxes.cls  # 类别索引

        # 如果有类别名称，可以通过类别索引获取
        class_names = [model.names[int(cls)] for cls in classes]

        for box, score, class_name in zip(boxes, scores, class_names):
            # print(f"Class: {class_name}, Score: {score:.2f}, Box: {box}")
            if score > 0.7 and class_name not in detected_objects:
                detected_objects.append(class_name)
    if detected_objects:
        return detected_objects
    else:
        return None




# 读取图像
img = cv2.imread("test-imgs/cat.jpg")
result = YOLO_detect(img)
