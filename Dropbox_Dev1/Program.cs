using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Dropbox_Dev1
{
    class Program
    {
        static void Main(string[] args)
        {
            var task = Task.Run(getmyaccount);
            task.Wait();
        }

        static async Task<bool> getmyaccount()
        {
            //passing the target URL into a string. Target endpoint is below
            string url = "https://api.dropboxapi.com/2/team_log/get_events";

            //instantiating a webrequest and passing the target endpoint url from above into it
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));

            //setting appropriate headers for the HTTP request
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", "Bearer teHGRgW-saAAAAAAAAAA09H6NA-kpaTFXVUM7j1ctaCXm0OOiNGK5WXXrYKOtgOL");

            request.Method = "POST";//Dropbox's API communicates using the POST methos, so that header is set appropriately

            //Data requested is then fetched and stored
            string postData = "null\n";
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] byte1 = encoding.GetBytes(postData);
            request.ContentLength = byte1.Length;
            Stream newStream = request.GetRequestStream();
            newStream.Write(byte1, 0, byte1.Length);
            newStream.Close();

            //response is called and the data is processed (into a raw string format) appropriately 
            using (WebResponse response = await request.GetResponseAsync())
            {

                using (Stream stream = response.GetResponseStream())
                {
                    //process the response
                    StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                    String responseString = reader.ReadToEnd();
                    //Json.NET stuff 
                    dynamic array = JsonConvert.DeserializeObject(responseString);
                    string json = Convert.ToString(array);

                    //response written to a json.gz file
                    string path = @"c:\Users\Andrew\Desktop\Test.text";

                    if (!File.Exists(path))
                    {
                        File.WriteAllText(path, json);
                        Console.WriteLine("You have successfully written to a file.");
                        Console.ReadLine();
                    }
                    //if the file already exists
                    else
                    {
                        Console.WriteLine("Here is the following data: " + "\n");

                        string readText = File.ReadAllText(path);
                        Console.WriteLine(readText);
                        Console.ReadLine();
                    }


                    return true;
                }
            }
        }

    }
}
