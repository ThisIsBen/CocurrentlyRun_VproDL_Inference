using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using System.Threading.Tasks;
using System.Text;
using ViDi2;
using System;

namespace ConsoleApp1
{
    class Program
    {


        public class ModelConfig
        {

            // ワークスペース
            public IWorkspace Workspace { get; set; }

            // ストリーム
            public IStream Stream { get; set; }

            // コンストラクタ
            public ModelConfig(IWorkspace workspace, string stream)
            {
                Workspace = workspace;
                Stream = Workspace.Streams[stream];

            }
        }


        public class ViDiInspector
        {
            // VIDI実行用コントロール
            private static ViDi2.Runtime.Local.Control control = new ViDi2.Runtime.Local.Control(ViDi2.GpuMode.Deferred);
            public　static string processingTimeFolderPath = "";
            private ModelConfig Model;

            //自動的に実行するstatic変数の初期化
            static ViDiInspector()
            {
                // Initializes all CUDA devices
                control.InitializeComputeDevices(ViDi2.GpuMode.SingleDevicePerTool, new List<int>() { });

                

            }

            //フォルダ内の画像をViDiで検査する
            public void runViDi(string folderPath, int modelNo)
            {
               
                try
                {
                    

                    //Inspect all images in the folder
                    foreach (string imagePath in Directory.GetFiles(folderPath, "*.jpg"))
                    {
                        //Inspect an image
                        InpsectAnImage(imagePath, modelNo);

                    }
                }
                catch(System.Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                
            }

            //ViDiランタイムファイルをロードする
            public  void LoadRuntimeFile(string runtimeFilePath, string streamName, int modelNo)
            {
                // メインランタイムの読み込み
                using (var runtimeFile = new FileStream(runtimeFilePath, FileMode.Open, FileAccess.Read))
                {

                    // ベースワークスペースにメインランタイムのワークスペースを追加
                    // 同じランタイムファイルを複数ロードできるように、ランタイムファイル名に"_[runtimeFileCount]"を付けている。
                    // GPUメモリが足りない場合、ここでメモリ足りないエラーが発生する。
                    string runTimeModelName = Path.GetFileNameWithoutExtension(runtimeFilePath);
                    IWorkspace workspace = control.Workspaces.Add($"{runTimeModelName}_{modelNo}", runtimeFile);



                    // AIモデルをDictionaryに格納
                    Model = new ModelConfig(workspace, streamName);






                }
            }

            //1枚の画像を検査する
            private async void InpsectAnImage(string imagePath, int modelNo)
            {
                //処理時間測定のための準備
                Stopwatch overallSW = new Stopwatch();
                Stopwatch sw = new Stopwatch();
                string outputStr = "";




                //ViDi実行時間測定開始
                overallSW.Restart();



                //画像の読み込み時間測定開始
                sw.Restart();
                //Inspect the image
                using (var image = new ViDi2.UI.WpfImage(imagePath))
                {

                    //画像の読み込み時間測定停止
                    sw.Stop();
                    outputStr += $"画像の読み込み,{sw.ElapsedMilliseconds},,";



                    //各モデルの推論実行時間測定開始
                    sw.Restart();

                    // 各モデルの推論実行                     
                    var Sample = Model.Stream.Process(image);

                    //各モデルの推論実行時間測定停止
                    sw.Stop();
                    outputStr += $"各モデルの推論実行,{sw.ElapsedMilliseconds},,";



                    // 実行結果の取得                   
                    Dictionary<string, ViDi2.IMarking> markings = Sample.Markings;

                    // 処理結果の破棄
                    Sample.Dispose();


                }
                //ViDi実行時間測定停止
                overallSW.Stop();
                outputStr += $"ViDi実行時間,{overallSW.ElapsedMilliseconds}\n";



                //Write processing time to a file
                using (StreamWriter file = new StreamWriter($@"{processingTimeFolderPath}\処理時間{modelNo}.csv", append: true, Encoding.GetEncoding("shift-jis")))
                {


                    await file.WriteAsync(outputStr);



                }

            }

        }



        static async void startMultiViDiRuntime(int numOfModel, string modelPath,string folderPath, string processingTimeFolderPath)
        {

            



            Console.WriteLine("複数のランタイムファイル同時実行開始");

            ViDiInspector.processingTimeFolderPath = processingTimeFolderPath;
            List<ViDiInspector> ViDiObj_List = new List<ViDiInspector>(numOfModel);

            //複数のランタイムファイルをロードする
            for (int i = 0; i < numOfModel; i++)
            {

                int modelNo = i;
                ViDiObj_List.Add(new ViDiInspector());
                //Load ViDi model
                ViDiObj_List[i].LoadRuntimeFile(modelPath, streamName: "default", modelNo);

            }

            //同時に複数のViDiランタイムファイルを実行し、それぞれのフォルダ内の画像を検査する（それぞれのフォルダ内の画像は同じである）
            var tasks = new List<Task>();
            for (int i = 0; i < numOfModel; i++)
            {

                int modelNo = i;
                var task = Task.Run(() =>
                {
                    ViDiObj_List[modelNo].runViDi($@"{folderPath}\監視フォルダ{modelNo}", modelNo);
                });
                tasks.Add(task);
                
            }

            await Task.WhenAll(tasks.ToArray());
            Console.WriteLine("複数のランタイムファイル同時実行終了\n任意のキーを押して終了してください。");

        }

        static  void Main(string[] args)
        {

            //同時に実行するViDiランタイムファイルの個数を指定
            int numOfModel = 18;
            
            //同時に実行するViDiランタイムファイルを指定
            string modelPath = @"..\RuntimeFile.vrws";

            //画像監視先指定
            //画像監視先に画像が入っている"監視フォルダ0"~"監視フォルダnumOfModel-1"の存在が必要である
            string folderPath = @"..\画像監視先";

            //処理時間の出力先指定とフォルダ作成
            string processingTimeFolderPath = @"..\処理時間の出力";
            Directory.CreateDirectory(processingTimeFolderPath);

            //同時実行開始
            startMultiViDiRuntime(numOfModel, modelPath, folderPath,processingTimeFolderPath);


            //終了までまつ
            Console.Read();
        }



    }
}


