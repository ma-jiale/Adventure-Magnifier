import os

files = os.listdir('my_dataset/temp')  # 获取指定路径下的文件和子目录列表
dataset_names = []

# 获取已标注结果的图片的文件名
for file in files:
    filename, ext = os.path.splitext(file)
    if ext == ".txt":
        dataset_names.append(filename)

# 删除未标注结果的图片文件
for file in files:
    filename, ext = os.path.splitext(file)
    if ext == ".png" or ext == ".jpeg" or ext == ".jpg":
        if not filename in  dataset_names:
            path = os.path.join('my_dataset/temp', file)
            os.remove(path)
