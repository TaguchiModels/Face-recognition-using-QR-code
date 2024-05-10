# Face recognition using QR code
Create a face recognition application using a QR code using the Taguchi model, which adds Japanese face data to the dlib face recognition model.

![QRcodeFaceialRecognitionSytem](https://github.com/TaguchiModels/Face-recognition-using-QR-code/assets/167880914/341ef4ad-91ae-4f8f-a571-f04f01e535b5)

## Overview of Taguchi model  
dlib has an excellent facial recognition model. However, unfortunately, it is racially biased, and it is no exaggeration to say that it is completely useless when it comes to Northeast Asians, especially Japanese people. However, I think it can't be helped. This is because the facial recognition dataset used for training was biased.  
So I collected a lot of datasets, mainly Japanese, and trained from scratch. This took a tremendous amount of time, but I think we have reached a standard that can withstand practical use to some extent. Even though it was trained for Japanese people, the results are similar to the dlib model when it comes to facial recognition for Westerners. Photos of Hollywood action heroes provided in the dlib example can also be classified in the same way as dlib.  

Please see below for detailed explanation  
[Taguchi Models](https://github.com/TaguchiModels/dlibModels/blob/main/README_EN.md)

## Obtaining a facial recognition model  
Please obtain the dlib model 'shape_predictor_5_face_landmarks.dat' from the link below.  
  `*The extension has '.bz2', so please unzip it after downloading`  
[dlib.net/files](http://dlib.net/files/)

Please obtain the OpenCV cascade discriminator 'haarcascade_frontalface_default.xml' from below.  
[OpenCV haarcascade](https://github.com/kipr/opencv/blob/master/data/haarcascades/haarcascade_frontalface_default.xml)

Please download and unzip my model 'taguchi_face_recognition_resnet_model_v1.dat' from the link below.  
[Taguchi's face recognition model](https://drive.google.com/file/d/1uMAZbPHiKOl6sjDgAoORn8g5U4wHQisW/view?usp=sharing)

## C++ compilation method  
Please refer to dlib.net for instructions on how to compile 'FaceRecognition.cpp'.   
* Visual studio 2022  (V143)  
* ISO C++14  
* x64, Release  
[How to compile dlib](http://dlib.net/compile.html)

## C# compilation method  
'MainWindow.xaml.cs', 'MainWindow.xaml' のコンパイル方法は以下になります。  
* Visual studio 2022  
* Target framework .NET8.0  
* Nuget:OpenCvSharp4.Windows  
* Nuget:OpenCvSharp4.Extensions  
* Nuget:OpenCvSharp4.WpfExtensions  
* Nuget:ZXing.Net  

## Preparing for execution  
Place the 'FaceRecognition.exe' executable file created after compilation into the C# execution folder.  
 * Place a 'faces' folder and a '000100' folder under it.
 * Also place '000100.jpg' under it.
 * Place the 'temp' folder under the 'faces' folder and place 5 images of 'YAMAZAKI_Kento'.
 * Place 'Images' folder and place camera.png under it.
 * Place 'shape_predictor_5_face_landmarks.dat'.
 * Please place 'taguchi_face_recognition_resnet_model_v1.dat'.
 * Place 'haarcascade_frontalface_default.xml'.
 * Connect your webcam to your PC.
 * Please ensure that there is sufficient brightness for the person appearing on the web camera.
 * Print 'TestQrcode.png' located in the 'Images' folder or keep it on your smartphone.

You need to create a folder under the 'faces' folder in advance with the value of reading the QR code.  
(*In this example, it was created in the '000100' folder.)  
The face image file stored in that folder must also be saved in the 'QR code reading result.png' format.  

## Execution method  
Run from Visual studio 2022.  
 * Click the "Start" button.
 * Point the QR code printed or on your smartphone at the web camera.
 * If the QR code is successfully read, 'OK! Please face the webcam.' will be displayed.
 * Face the webcam and wait while 'Authenticating, please wait a moment.' is displayed.
 * You are authenticated when you see 'The system has authenticated you.'
 * Once authentication is complete, the QR code reading result and authentication accuracy will be displayed at the bottom of the screen.

If you want to unit test 'FaceRecognition.exe', please type the following command from the command prompt.    
  * C++ unit test example    
    ```
    > FaceRecognition 000100
    ```  
## Limitations
My model lacks training data, so its accuracy may be significantly worse.  
 * Person with African roots
 * Persons under 18 years of age
 * Person wearing a mask

It may not be suitable for the above applications.  
Please use with caution.  

