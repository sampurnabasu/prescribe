using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using LightBuzz.Vitruvius;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace prescribe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //kinect stuff
        private KinectSensor sensor = null;
        private MultiSourceFrameReader reader = null;
        private IList<Body> bodies;
        private Body body = null;
        private ulong userID = 0;

        //Bitmap stuff
        private BitmapSource bitmap = null;
        private BitmapEncoder encoder = null;
        private string extension = ".jpg";

        //Face rectangle variable 
        private double left = 0;
        private double top = 0;
        private double left1 = 0;
        private double top1 = 0;
        private double w = 0;
        private double h = 0;
        private double w1 = 0;
        private double h1 = 0;

        //logic
        string pName = "";
        List<string> linkArray;
        bool taken = false;
        int picCount = 0; 

        //Blob storage stuff
        private CloudStorageAccount account = null;
        private CloudBlobClient blob = null;

        //project oxford
        private string key = "6e040e8d7b514216a914cc2a7df9bd54 ";
        private string link = @"https://api.projectoxford.ai/face/v1.0/detect[?returnFaceId][&returnFaceLandmarks][&returnFaceAttributes]
&subscription-key=ad51708da89d4aa1af0d0960dda19411";
        public FaceServiceClient faceServiceClient;
        string personGroupId = "shop_database";



        public MainWindow()
        {
            sensor = KinectSensor.GetDefault();
            if (sensor != null)
            {
                sensor.Open();
                reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body);
                reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

            }

            account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            blob = account.CreateCloudBlobClient();
            InitializeFace();
            InitializeComponent();


            dropDatabase("jacket");
            dropDatabase("hoodie");
            dropDatabase("hat");

            
            
            

        }

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var frame = e.FrameReference.AcquireFrame();
            using (var colorFrame = frame.ColorFrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    bitmap = (BitmapSource)colorFrame.ToBitmap();
                    camera.Source = bitmap;
                }
            }

            using (var bodyFrame = frame.BodyFrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    
                    board.Children.Clear();
                    body = bodyFrame.Bodies().Closest();

                    if (body != null)
                    {
                        if (body.IsTracked)
                        {
                            picCount += 1;
                            Joint head = body.Joints[JointType.Head];
                            Joint spine = body.Joints[JointType.SpineMid];
                            Joint rShoulder = body.Joints[JointType.ShoulderRight];


                            CameraSpacePoint cameraPoint = head.Position;
                            ColorSpacePoint colorPoint = sensor.CoordinateMapper.MapCameraPointToColorSpace(cameraPoint);

                            CameraSpacePoint cameraPoint1 = spine.Position;
                            ColorSpacePoint colorPoint1 = sensor.CoordinateMapper.MapCameraPointToColorSpace(cameraPoint1);

                            CameraSpacePoint cameraPoint2 = rShoulder.Position;
                            ColorSpacePoint colorPoint2 = sensor.CoordinateMapper.MapCameraPointToColorSpace(cameraPoint2);

                            Point point = new Point();
                            point.X = float.IsInfinity(colorPoint.X) ? 0 : colorPoint.X;
                            point.Y = float.IsInfinity(colorPoint.Y) ? 0 : colorPoint.Y;

                            Point point1 = new Point();
                            point1.X = float.IsInfinity(colorPoint1.X) ? 0 : colorPoint1.X;
                            point1.Y = float.IsInfinity(colorPoint1.Y) ? 0 : colorPoint1.Y;

                            Point point2 = new Point();
                            point2.X = float.IsInfinity(colorPoint2.X) ? 0 : colorPoint2.X;
                            point2.Y = float.IsInfinity(colorPoint2.Y) ? 0 : colorPoint2.Y;

                            //w = (1.38 * 240) / cameraPoint1.Z;
                            //h = (1.38 * 260) / cameraPoint1.Z;

                            w = double.IsInfinity(((1.38 * 240) / cameraPoint1.Z)) ? 0 : ((1.38 * 240) / cameraPoint1.Z);
                            h = double.IsInfinity((1.38 * 260) / cameraPoint1.Z) ? 0 : (1.38 * 260) / cameraPoint1.Z;

                            Rectangle rect = new Rectangle
                            {
                                Width = w,
                                Height = h,
                                Stroke = Brushes.Red,
                                StrokeThickness = 2
                            };

                            left = point.X - rect.Width / 2;
                            top = point.Y - rect.Height / 2;

                            Canvas.SetLeft(rect, point.X - rect.Width / 2);
                            Canvas.SetTop(rect, point.Y - rect.Height / 2);
                            board.Children.Add(rect);

                            w1 = w / 1.5;
                            h1 = h / 1.5;

                            Rectangle rect1 = new Rectangle
                            {
                                Width = w1,
                                Height = h1,
                                Stroke = Brushes.Blue,
                                StrokeThickness = 2
                            };

                            left1 = (point2.X - rect1.Width / 2) - 100;
                            top1 = (point1.Y - rect1.Height / 2) + 100;

                            Canvas.SetLeft(rect1, left1 );
                            Canvas.SetTop(rect1, top1 );
                            board.Children.Add(rect1);

                            if (picCount == 120)
                            {
                                //takePicture();
                            }
                            
                        }
                    }
                }
            }

        }

        private void takePicture()
        {
            facePic.Source = cutImage();
            BitmapSource faceImage = (BitmapSource)facePic.Source;
            savePicture(faceImage, 0);

            qrPic.Source = cutImage1();
            BitmapSource qrImage = (BitmapSource)qrPic.Source;
            savePicture(qrImage, 1);
        }

        private ImageSource cutImage()
        {
            if (bitmap == null)
            {
                return null;
            }

            try
            {
                CroppedBitmap temp = new CroppedBitmap(bitmap, new Int32Rect((int)left, (int)top, (int)w, (int)h));
                ImageSource temp1 = (ImageSource)temp;
                return temp1;
            } catch (Exception e)
            {
                return null;
            }
        }

        private ImageSource cutImage1()
        {
            if (bitmap == null)
            {
                return null;
            }

            try
            {
                CroppedBitmap temp = new CroppedBitmap(bitmap, new Int32Rect((int)left1, (int)top1, (int)w1, (int)h1));
                ImageSource temp1 = (ImageSource)temp;
                return temp1;
            }
            catch (Exception e)
            {
                return null;
            }
        }



        private async void savePicture(BitmapSource map, int target)
        {
            picState.Content = "Taken";

            string path = "";
            if (target == 0)
            {
                path = @"C:\\Users\\sagar\\Desktop\\closet\\img\\face.jpg";
            }
            else
            {
                path = @"C:\\Users\\sagar\\Desktop\\closet\\img\\qr.png";
            }

            if (map != null)
            {
                encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(map));
                try
                {
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }
                }
                catch (IOException)
                {

                }
            }

            if (target == 0)
            {
                
            }
            else
            {
                path = @"C:\\Users\\sagar\\Desktop\\closet\\img\\face.jpg";
                
                string name = await IdentifyFace(path);
                if (name == "Ben")
                {
                    pName = name;
                    personName.Content = name;
                }
                else
                {
                    pName = "";

                    personName.Content = name;
                    //not ben 
                    return;
                }
                
                

                CloudBlobContainer container = blob.GetContainerReference("face");
                container.CreateIfNotExists();
                container.SetPermissions(
                    new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    }
                );

                CloudBlockBlob blockBlob = container.GetBlockBlobReference("qr.png");

                path = @"C:\\Users\\sagar\\Desktop\\closet\\img\\qr.png";
                using (var filestream = System.IO.File.OpenRead(path))
                {
                    blockBlob.UploadFromStream(filestream);
                    string link = "http://api.qrserver.com/v1/read-qr-code/?fileurl=https://prescribe.blob.core.windows.net/face/qr.png";
                    var client = new RestClient(link);
                    var request = new RestRequest("", Method.GET);
                    request.RequestFormat = RestSharp.DataFormat.Json;
                    var response = client.Execute(request);


                    var shwin = JArray.Parse(response.Content);
                    Console.WriteLine(shwin);
                    string cloth = shwin[0]["symbol"][0]["data"].ToString();
                    readyState.Content = cloth;
                    Console.WriteLine("PName: " + pName);
                    Console.WriteLine("Cloth: " + cloth);
                    updateFirebase(pName, cloth);

                    //Console.WriteLine(shwin[0]["symbol"][0]["data"]);

                    //readyState.Content = shwin["owner"];
                    //handState.Content = shwin["status"];



                }
            }
            
            
            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (reader != null)
            {
                reader.Dispose();
                reader = null;
            }

            if (sensor != null)
            {
                sensor.Close();
                sensor = null;
            }
        }


        private void updateFirebase(string name, string cloth)
        {
            string clothType = "";
            if (name != "Ben")
            {
                name = "Shwin";
            }
            if (cloth == "cloth1")
            {
                clothType = "hoodie";
            }
            else if (cloth == "cloth2")
            {
                clothType = "jacket";
            }
            //post dosage
            Console.WriteLine("CLOTHTYPE: " + clothType);
            //https://daisys-closet.firebaseio.com/red-jacket/status
            var link = "https://daisys-closet.firebaseio.com/" + clothType + "/owner/.json";
            var client = new RestClient(link);
            var request = new RestRequest("", Method.PUT);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddBody(name);
            IRestResponse response = client.Execute(request);

            var link1 = "https://daisys-closet.firebaseio.com/" + clothType + "/status/.json";
            var client1 = new RestClient(link1);
            var request1 = new RestRequest("", Method.PUT);
            request1.RequestFormat = RestSharp.DataFormat.Json;
            request1.AddBody(2);
            IRestResponse response1 = client1.Execute(request1);


            //await Task.Delay(30000);


        }


        public void dropDatabase(string cloth)
        {
            var link = "https://daisys-closet.firebaseio.com/" + cloth + "/owner/.json";
            var client = new RestClient(link);

            var request = new RestRequest("", Method.PUT);
            request.RequestFormat = RestSharp.DataFormat.Json;

            request.AddBody("");
            IRestResponse response = client.Execute(request);

            link = "https://daisys-closet.firebaseio.com/" + cloth + "/status/.json";
            var client1 = new RestClient(link);
            var request1 = new RestRequest("", Method.PUT);
            request1.RequestFormat = RestSharp.DataFormat.Json;
            request1.AddBody(0);
            IRestResponse response1 = client1.Execute(request1);


        }

        //Project Oxford Stuff
        public void InitializeFace()
        {
            faceServiceClient = new FaceServiceClient(key);
            try
            {
               // CreatePersonGroup();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async void CreatePersonGroup()
        {
            await faceServiceClient.DeletePersonGroupAsync(personGroupId);
            await faceServiceClient.CreatePersonGroupAsync(personGroupId, "Database");

            // Define Ben
            CreatePersonResult friend1 = await faceServiceClient.CreatePersonAsync(
                // Id of the person group that the person belonged to
                personGroupId,
                // Name of the person
                "Ben"
            );

            // Directory contains image files of Anna
            const string friend1ImageDir = @"C:\Users\sagar\Desktop\closet\face";

            foreach (string imagePath in Directory.GetFiles(friend1ImageDir, "*.jpg"))
            {
                using (Stream s = File.OpenRead(imagePath))
                {
                    Console.WriteLine(imagePath);
                    // Detect faces in the image and add to Anna
                    await faceServiceClient.AddPersonFaceAsync(
                        personGroupId, friend1.PersonId, s);
                              
                }
            }
            
         
            
            await faceServiceClient.TrainPersonGroupAsync(personGroupId);
            TrainingStatus trainingStatus = null;
            while (true)
            {
                trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId);

                if (trainingStatus.Status != Status.Running)
                {
                    break;
                }

                await Task.Delay(1000);
            }
            
            // Define Bill and Clare in the same way
        }

        public async Task AddFace(string path, string name)
        {
            // Define Anna
            CreatePersonResult friend1 = await faceServiceClient.CreatePersonAsync(
                // Id of the person group that the person belonged to
                personGroupId,
                // Name of the person
                name
            );

            // Directory contains image files of Anna
            string friend1ImageDir = path;

            foreach (string imagePath in Directory.GetFiles(friend1ImageDir, "*.jpg"))
            {
                using (Stream s = File.OpenRead(imagePath))
                {
                    // Detect faces in the image and add to Anna
                    await faceServiceClient.AddPersonFaceAsync(
                        personGroupId, friend1.PersonId, s);
                }
            }
            await faceServiceClient.TrainPersonGroupAsync(personGroupId);
            TrainingStatus trainingStatus = null;
            while (true)
            {
                trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId);

                if (trainingStatus.Status != Status.Running)
                {
                    break;
                }

                await Task.Delay(1000);
            }
        }

        public async Task<string> IdentifyFace(string path)
        {
            string testImageFile = path;
            try
            {
                using (Stream s = File.OpenRead(testImageFile))
                {
                    var faces = await faceServiceClient.DetectAsync(s);
                    var faceIds = faces.Select(face => face.FaceId).ToArray();

                    var results = await faceServiceClient.IdentifyAsync(personGroupId, faceIds);
                    foreach (var identifyResult in results)
                    {
                        Console.WriteLine("Result of face: {0}", identifyResult.FaceId);
                        if (identifyResult.Candidates.Length == 0)
                        {
                            Console.WriteLine("No one identified");
                            return "GG";
                        }
                        else
                        {
                            // Get top 1 among all candidates returned
                            var candidateId = identifyResult.Candidates[0].PersonId;
                            var person = await faceServiceClient.GetPersonAsync(personGroupId, candidateId);
                            Console.WriteLine("Identified as {0}", person.Name);
                            return person.Name;
                        }
                    }
                }
                return "fail";
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return "Fail";
            }
           
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            takePicture();
        }
    }
}
