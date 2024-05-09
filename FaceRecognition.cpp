/*
  Written by Taguchi.
    QRコードを利用した顔認証のサンプルコードです。
    「taguchi_face_recognition_resnet_model_v1.dat」と
    「shape_predictor_5_face_landmarks.dat」の
    詳細については以下を参照してください。
    https://github.com/TaguchiModels/dlibModels

    単体テスト用のコマンド例) ./FaceRecognition QRcode
    ----------------------------------------------------------

    This is a sample code for facial recognition using QR code.
    "taguchi_face_recognition_resnet_model_v1.dat" and
    "shape_predictor_5_face_landmarks.dat"
    See below for details.
    https://github.com/TaguchiModels/dlibModels

    Example commands for unit testing) ./FaceRecognition QRcode
    ----------------------------------------------------------
*/

#include <fstream>
#include <dlib/dnn.h>
#include <dlib/gui_widgets.h>
#include <dlib/clustering.h>
#include <dlib/string.h>
#include <dlib/image_io.h>
#include <dlib/image_processing/frontal_face_detector.h>

using namespace dlib;
using namespace std;

// ----------------------------------------------------------------------------------------
template <template <int, template<typename>class, int, typename> class block, int N, template<typename>class BN, typename SUBNET>
using residual = add_prev1<block<N, BN, 1, tag1<SUBNET>>>;

template <template <int, template<typename>class, int, typename> class block, int N, template<typename>class BN, typename SUBNET>
using residual_down = add_prev2<avg_pool<2, 2, 2, 2, skip1<tag2<block<N, BN, 2, tag1<SUBNET>>>>>>;

template <int N, template <typename> class BN, int stride, typename SUBNET>
using block = BN<con<N, 3, 3, 1, 1, relu<BN<con<N, 3, 3, stride, stride, SUBNET>>>>>;

template <int N, typename SUBNET> using ares = relu<residual<block, N, affine, SUBNET>>;
template <int N, typename SUBNET> using ares_down = relu<residual_down<block, N, affine, SUBNET>>;

template <typename SUBNET> using alevel0 = ares_down<256, SUBNET>;
template <typename SUBNET> using alevel1 = ares<256, ares<256, ares_down<256, SUBNET>>>;
template <typename SUBNET> using alevel2 = ares<128, ares<128, ares_down<128, SUBNET>>>;
template <typename SUBNET> using alevel3 = ares<64, ares<64, ares<64, ares_down<64, SUBNET>>>>;
template <typename SUBNET> using alevel4 = ares<32, ares<32, ares<32, SUBNET>>>;

using anet_type = loss_metric<fc_no_bias<128, avg_pool_everything<
    alevel0<
    alevel1<
    alevel2<
    alevel3<
    alevel4<
    max_pool<3, 3, 2, 2, relu<affine<con<32, 7, 7, 2, 2,
    input_rgb_image_sized<150>
    >>>>>>>>>>>>;

// ----------------------------------------------------------------------------------------

int main(int argc, char** argv)
try
{
    if (argc != 2)
    {
        cout << "Run this example by invoking it like this: " << endl;
        cout << "  ./FaceRecognition QRcode" << endl;
        return 1;
    }

    frontal_face_detector detector = get_frontal_face_detector();
    shape_predictor sp;
    deserialize("shape_predictor_5_face_landmarks.dat") >> sp;
    anet_type net;
    deserialize("taguchi_face_recognition_resnet_model_v1.dat") >> net;
    string path = "faces/";
    path += argv[1];

    matrix<rgb_pixel> webcamImg;
    std::vector<matrix<rgb_pixel>> faces;

    // get QR code image 
    std::vector<file> QRfiles = get_files_in_directory_tree(path, match_ending(".png"));
    for (const auto& QRfile : QRfiles)
    {
        load_image(webcamImg, QRfile);
        for (auto face : detector(webcamImg))
        {
            auto shape = sp(webcamImg, face);
            matrix<rgb_pixel> face_chip;
            extract_image_chip(webcamImg, get_face_chip_details(shape, 150, 0.25), face_chip);
            faces.push_back(move(face_chip));

            break; // Assuming one person
        }

        break; // Assuming one image
    }

    if (faces.size() == 0)
    {
        cout << "No images found in " << path << endl;
        return 2;
    }

    // get webcam image 
    std::vector<file> imgfiles = get_files_in_directory_tree("faces/temp", match_ending(".jpg"));
    for (const auto& imgfile : imgfiles)
    {
        load_image(webcamImg, imgfile);
        for (auto face : detector(webcamImg))
        {
            auto shape = sp(webcamImg, face);
            matrix<rgb_pixel> face_chip;
            extract_image_chip(webcamImg, get_face_chip_details(shape, 150, 0.25), face_chip);
            faces.push_back(move(face_chip));
        }
    }

    if (faces.size() < 2)
    {
        cout << "No faces found in image!" << "faces/temp" << endl;
        return 3;
    }

    cout << "face count:" << faces.size() << endl;

    std::vector<matrix<float, 0, 1>> face_descriptors = net(faces);
    float accuracy = std::numeric_limits<float>::max();

    for (size_t j = 1; j < face_descriptors.size(); ++j)
    {
        cout << "length: " << length(face_descriptors[0] - face_descriptors[j]) << endl;
        if (accuracy > length(face_descriptors[0] - face_descriptors[j]))
        {
            accuracy = length(face_descriptors[0] - face_descriptors[j]);
        }
    }

    ofstream writing_file;
    string filename = path + "/accuracy.txt";
    writing_file.open(filename, std::ios::out);
    string writing_text = to_string(accuracy);
    writing_file << writing_text << std::endl;
    writing_file.close();

    cout << "accuracy: " << accuracy << endl;
    cout << "hit enter to terminate" << endl;

    //cin.get();

    return 0;
}
catch (std::exception& e)
{
    cout << e.what() << endl;
    return 4;
}

