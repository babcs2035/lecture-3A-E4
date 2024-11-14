import os
import matplotlib.pyplot as plt


# ディレクトリ内でファイル名が辞書順最大のものを取得する関数
def get_latest_file(directory):
    files = os.listdir(directory)
    files = [
        f
        for f in files
        if os.path.isfile(os.path.join(directory, f)) and f.endswith(".txt")
    ]
    latest_file = max(files)
    return os.path.join(directory, latest_file)


# ファイルからデータを読み込む関数
def read_data_from_file(file_path):
    data1 = []
    data2 = []
    with open(file_path, "r") as file:
        for line in file:
            values = line.strip().split(",")
            data1.append(float(values[0]))
            data2.append(float(values[1]))
    return data1, data2


# ディレクトリのパス
directory_path = "Fitness/"  # ディレクトリ名を指定

# 最新のファイルを取得
latest_file_path = get_latest_file(directory_path)
print(f"Latest file: {latest_file_path}")

# ファイルからデータを読み込む
data1, data2 = read_data_from_file(latest_file_path)

# グラフをプロット
plt.figure(figsize=(10, 5))
plt.plot(data1, label="Max")
plt.plot(data2, label="Avg")
plt.xlabel("Generation")
plt.ylabel("Fitness")
plt.title("Fitness")
plt.legend()
plt.grid(True)
plt.savefig(latest_file_path.replace(".txt", ".png"))

print(f"Save fitness graph: {latest_file_path.replace(".txt", ".png")}")
