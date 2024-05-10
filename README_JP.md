# QRコードを使った顔認証  
dlib の顔認識モデルに日本人の顔データを追加した Taguchi model を使ってQRコードを使った顔認識アプリケーションを作成します。  

![QRcodeFaceialRecognitionSytem](https://github.com/TaguchiModels/Face-recognition-using-QR-code/assets/167880914/341ef4ad-91ae-4f8f-a571-f04f01e535b5)

## Taguchi model の概略  
dlib には優秀な顔認証モデルがあります。しかし、残念ながら人種に偏りがあり、特に北東アジア人、とりわけ日本人に関してはまったく役に立たないと言っても過言ではありません。ですが、それは仕方のないことだと思います。訓練に使用した顔認証用のデータセットに偏りがあったためです。  
そこで私は日本人を中心とした多くのデータセットを収集し、ゼロから訓練を行いました。これには途方もない時間を要しましたが、ある程度は実用に耐えうる基準に達したと思います。日本人用に訓練したとは言っても欧米人の顔認証に関しても dlib のモデルに近い結果となってます。dlib の example で用意されているハリウッドのアクション ヒーローの写真も dlib と同様に分類が可能です。  

詳細な説明は下記をご覧ください  
[Taguchi Models](https://github.com/TaguchiModels/dlibModels)

## 顔認証モデルの入手
dlib のモデル 'shape_predictor_5_face_landmarks.dat'は以下のリンクより取得してください。  
 `※ 拡張子に'.bz2'が付いていますので、ダウンロード後に解凍してください`  

[dlib.net/files](http://dlib.net/files/)

OpenCV のカスケード識別器 'haarcascade_frontalface_default.xml' を以下より取得してください。  

[OpenCV haarcascade](https://github.com/kipr/opencv/blob/master/data/haarcascades/haarcascade_frontalface_default.xml)

私のモデル 'taguchi_face_recognition_resnet_model_v1.dat' は以下のリンクよりダウンロードして解凍してください。  

[Taguchi's face recognition model](https://drive.google.com/file/d/1uMAZbPHiKOl6sjDgAoORn8g5U4wHQisW/view?usp=sharing)

## C++ コンパイル方法  
'FaceRecognition.cpp' のコンパイル方法は dlib.net に記載されていますので参考にしてください。  
* Visual studio 2022  (V143)  
* ISO C++14  
* x64, Release  
[How to compile dlib](http://dlib.net/compile.html)

## C# コンパイル方法  
'MainWindow.xaml.cs', 'MainWindow.xaml' のコンパイル方法は以下になります。  
* Visual studio 2022  
* Target framework .NET8.0  
* Nuget:OpenCvSharp4.Windows  
* Nuget:OpenCvSharp4.Extensions  
* Nuget:OpenCvSharp4.WpfExtensions  
* Nuget:ZXing.Net  

## 実行の準備  
コンパイル後に出来た 'FaceRecognition.exe' 実行ファイルを C# の実行フォルダーに配置してください。  
 * 'faces' フォルダーを配置して、その配下に '000100' フォルダーを配置してください。
 *  さらにその配下に '000100.jpg を配置してください。
 * 'faces' フォルダーの配下に 'temp' フォルダーを配置して 'YAMAZAKI_Kento' の画像を5枚配置してください。
 * 'Images' フォルダーを配置して、その配下に camera.png を配置してください。
 * 'shape_predictor_5_face_landmarks.dat' を配置してください。  
 * 'taguchi_face_recognition_resnet_model_v1.dat' を配置してください。
 * 'haarcascade_frontalface_default.xml' を配置してください。
 * WebカメラをPCに接続してください。  
 * Webカメラに映る人物には、充分な明るさを確保してください。
 * 'Images' フォルダーにある 'TestQrcode.png' を印刷するか、またはスマートフォンに保管しておいてください。

QRコードの読み取りの値で事前に 'faces' フォルダーの下にフォルダーを作成しておく必要があります。  
（※この例の場合は'000100' フォルダーで作りました。）  
そのフォルダーに保管しておく顔画像のファイルも 'QRコードの読み取り結果.png' の形式で画像ファイルを保管しておく必要があります。  

## 実行方法  
Visual studio 2022 より実行します。  
 * 「Start」ボタンをクリックします。  
 * 印刷またはスマートフォンにあるQRコードをWebカメラに向けます。
 * QRコードを正常に読めたら 'OK! Please face the webcam.' が表示されます。
 * 'Authenticating, please wait a moment.' が表示されている間、Webカメラに顔を向けてそのまま待ちます。
 * 'The system has authenticated you.' が表示されたら認証されています。
 * 認証が終わると画面下にQRコードの読み取り結果と認証精度(accuracy)が表示されます。

'FaceRecognition.exe' の単体テストを行う場合は、コマンドプロンプトより以下のコマンドを打ってください。  
 * C++ 単体テストの例  
    ```
    > FaceRecognition 000100
    ```  
    
## 制限事項
私のモデルには訓練用データが不足しているため、精度が著しく悪い場合があります。  
 * アフリカ系のルーツを持つ人物  
 * 18歳未満の人物  
 * マスクを付けている人物  

上記の用途には向かないことがあります。  
注意してご利用ください。  


