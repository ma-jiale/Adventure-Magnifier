import os

# 设置前缀
prefix = 'dog_'  # 这里替换成你要的前缀

# 设置目标文件夹路径
folder_path = 'D:/Downloads/archive/raw-img/cane'  # 这里替换成你的文件夹路径

num = 0
# 获取文件夹中所有文件
for filename in os.listdir(folder_path):
    if os.path.isfile(os.path.join(folder_path, filename)):
        # 构建新的文件名
        new_filename = prefix + str(num) + '.jpg'
        # 获取完整的旧文件路径和新文件路径
        old_file = os.path.join(folder_path, filename)
        new_file = os.path.join(folder_path, new_filename)
        # 重命名文件
        os.rename(old_file, new_file)
        num = num + 1

print("文件重命名完成！")
