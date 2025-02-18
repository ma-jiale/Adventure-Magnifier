import cv2
from ultralytics import YOLO

# 加载模型
model = YOLO("yolo11s.pt")

# 读取图像
im2 = cv2.imread("test-img/sunflower.jpg")

# 进行预测
results = model.predict(source=im2)

# 获取预测后的图像结果
annotated_frame = results[0].plot()

# 使用OpenCV显示结果
cv2.imshow("YOLO11 Detection", annotated_frame)
cv2.waitKey(0)  # 等待按键关闭窗口
cv2.destroyAllWindows()
