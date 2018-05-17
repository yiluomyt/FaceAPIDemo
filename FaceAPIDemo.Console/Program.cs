using Microsoft.ProjectOxford.Face;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.ProjectOxford.Face.Contract;

namespace FaceAPIDemo.Console
{
    class Program
    {
        private const string groupId = "Your Group Id";

        static void Main(string[] args)
        {
            System.Console.WriteLine("1. Add Faces");
            System.Console.WriteLine("2. List Persons");
            System.Console.WriteLine("3. Train Model");
            System.Console.WriteLine("4. Identify Face");
            System.Console.WriteLine("Any Other Key to Exit.");
            int key = Convert.ToInt32(System.Console.ReadLine());
            switch (key)
            {
                case 1:
                    AddFacesAsync().Wait();
                    break;
                case 2:
                    ListPersonAsync().Wait();
                    break;
                case 3:
                    TrainAsync().Wait();
                    break;
                case 4:
                    IdentifyAsync().Wait();
                    break;
                default:
                    break;
            }
            System.Console.WriteLine("Please Hit Enter to Exit.");
            System.Console.ReadLine();
        }

        async static Task ListPersonAsync()
        {
            using (FaceServiceClient faceClient = CreateClient())
            {
                var persons = await faceClient.ListPersonsAsync(groupId);
                foreach (var person in persons)
                {
                    System.Console.WriteLine($"Name: {person.Name}");
                    System.Console.WriteLine($"StudentId: {person.UserData}");
                    System.Console.WriteLine($"PersonId: {person.PersonId}");

                    int count = 0;
                    foreach (var face in person.PersistedFaceIds)
                    {
                        System.Console.WriteLine($"Face {count++}: {face}");
                    }
                }
            }
        }

        async static Task AddFacesAsync()
        {
            // Input Info
            System.Console.Write("Input Name: ");
            string name = System.Console.ReadLine();
            System.Console.Write("Input StudentId: ");
            string studentId = System.Console.ReadLine();

            using (FaceServiceClient faceClient = CreateClient())
            {
                // judge person created
                var persons = await faceClient.ListPersonsAsync(groupId);
                var person = persons.FirstOrDefault(x => x.Name == name);
                Guid personId;
                if (person == null)
                {
                    var personResult = await faceClient.CreatePersonAsync(groupId, name, studentId);
                    System.Console.WriteLine($"Create Person As: {personResult.PersonId}");
                    personId = personResult.PersonId;
                }
                else
                {
                    personId = person.PersonId;
                }

                string path;
                while (true)
                {
                    System.Console.WriteLine("Please Input Face Image Path: ");
                    path = System.Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        break;
                    }
                    try
                    {
                        using (FileStream fs = new FileStream(path, FileMode.Open))
                        {
                            var personFace = await faceClient.AddPersonFaceInPersonGroupAsync(groupId, personId, fs, userData: fs.Name);
                            System.Console.WriteLine($"Added Face: {personFace.PersistedFaceId}");
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        System.Console.WriteLine("Please Check File Path.");
                    }
                    catch (FaceAPIException)
                    {
                        System.Console.WriteLine("Please Change A Iamge.");
                    }
                }
            }
        }

        async static Task TrainAsync()
        {
            using (FaceServiceClient faceClient = CreateClient())
            {
                await faceClient.TrainPersonGroupAsync(groupId);
                TrainingStatus status;
                do
                {
                    System.Threading.Thread.Sleep(1000);
                    status = await faceClient.GetPersonGroupTrainingStatusAsync(groupId);
                    System.Console.WriteLine($"Now Train Status: {status.Status}...");
                } while (status.Status != Status.Succeeded);
                System.Console.WriteLine("Train Successed.");
            }
        }

        async static Task IdentifyAsync()
        {
            System.Console.Write("Please Input Image Path: ");
            string path = System.Console.ReadLine();
            using (FaceServiceClient faceClient = CreateClient())
            {
                Face[] faces;
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    try
                    {
                        faces = await faceClient.DetectAsync(fs);
                    }
                    catch (FileNotFoundException)
                    {
                        System.Console.WriteLine("Please Check Image Path.");
                        return;
                    }
                }
                if (faces.Length == 0)
                {
                    System.Console.WriteLine("Can't Get Any Face.");
                    return;
                }

                var results = await faceClient.IdentifyAsync(groupId, faces.Select(face => face.FaceId).ToArray(), 0.6f);
                foreach(var result in results)
                {
                    System.Console.WriteLine($"FaceId: {result.FaceId}");
                    var rect = faces.FirstOrDefault(x => x.FaceId == result.FaceId).FaceRectangle;
                    System.Console.WriteLine($"Face Rectangle: {rect.Left}, {rect.Top}, {rect.Width}, {rect.Height}");
                    foreach (var candidate in result.Candidates)
                    {
                        Person person = await faceClient.GetPersonAsync(groupId, candidate.PersonId);
                        System.Console.WriteLine($"Name: {person.Name}, Confidence: {candidate.Confidence}");
                    }
                }
            }
        }

        static FaceServiceClient CreateClient() => new FaceServiceClient("Your Cognitive Service Key", "https://eastasia.api.cognitive.microsoft.com/face/v1.0/");
    }
}
