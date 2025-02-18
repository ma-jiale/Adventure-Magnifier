import cv2
from ultralytics import YOLO

# 加载模型
model = YOLO("models/yolo11_rose3/weights/best.pt")

# 打开摄像头
cap = cv2.VideoCapture(0)  # 0 是默认摄像头

if not cap.isOpened():
    print("无法打开摄像头")
    exit()

while True:
    # 读取摄像头图像
    ret, frame = cap.read()
    if not ret:
        print("无法读取摄像头帧")
        break

    # 进行预测
    results = model.predict(source=frame)

    # 获取预测后的图像结果
    annotated_frame = results[0].plot()

    # 显示结果
    cv2.imshow("YOLO11 Detection", annotated_frame)

    # 如果按下 'q' 键，则退出循环
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# 释放摄像头并关闭所有窗口
cap.release()
cv2.destroyAllWindows()
