# CocurrentlyRun_VproDL_Inference
## コードの内容：
18個のスレッドを起動し、18個のランタイムファイルをロードして、添付の.zip内の「画像監視先」内の「監視フォルダ0~監視フォルダ17」フォルダ内の画像を同時に検査して処理時間を出力する。
ここでロードする18個のランタイムファイルは同じランタイムファイルである。

## 利用しているランタイムファイルのバージョン：
VisionPro DeepLearning 2.1

## 実行する手順：
Step1 　1個のVisionPro DeepLearning 2.1のランタイムファイルを用意する
Step2   添付の.zip内の「画像監視先」内の「監視フォルダ0~監視フォルダ17」フォルダに数枚の画像を入れる
Step3   multiViDiRuntimes.csでEXEを生成して実行する

## 実行結果：
下記のように、18個のランタイムファイルの処理時間が18個CSVに出力されます。
ViDi実行時間≒各モデルの推論実行+画像の読み込み とのことです。

![image](https://github.com/ThisIsBen/CocurrentlyRun_VproDL_Inference/assets/8150459/b6cda437-2e49-45e7-81d4-22b9e0a87a1e)
