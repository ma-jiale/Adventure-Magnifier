import os

# 设置标签文件夹路径和需要修正的旧标签和新标签
labels_dir = "my_dataset/temp"  # 这里设置你的标签文件夹路径
old_class_id = 1  # 错误的标签编号
new_class_id = 0  # 正确的标签编号

# 遍历标签文件夹
for filename in os.listdir(labels_dir):
    if filename.endswith(".txt"):  # 只处理 txt 文件
        file_path = os.path.join(labels_dir, filename)

        # 打开文件读取内容
        with open(file_path, "r") as file:
            lines = file.readlines()

        # 检查每一行，替换错误的标签
        new_lines = []
        for line in lines:
            parts = line.strip().split()  # 使用 strip() 去除换行符
            if int(parts[0]) == old_class_id:  # 如果标签是错误的
                parts[0] = str(new_class_id)  # 修改为正确的标签
            new_lines.append(" ".join(parts))

        # 将修改后的内容写回文件
        with open(file_path, "w") as file:
            file.writelines([line + "\n" for line in new_lines])  # 确保每行末尾有换行符

print("标签修正完成！")


