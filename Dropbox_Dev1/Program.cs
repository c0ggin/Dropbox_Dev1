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
            //this is the cursor's file name, it is used in several places in the code so it is easier to intialize it at the top
            static string cursorPath = @"c:\Users\acoggin16\Desktop\Cursor.json.gz";

            //create the timestamp to be a part of the data's file name, this code also exists in the 3 spots it is used if you would like to not use a static variable
            static DateTime ts = DateTime.Now;
            static String timeStamp = String.Format("{0:MM-dd-yyyy}", ts);

            static void Main(string[] args)
            {
                var task = Task.Run(getmyaccount);
                task.Wait();
            }

            static async Task<bool> getmyaccount()
            {
                if (!File.Exists(cursorPath))
                {
                    //passing the target URL into a string. Target endpoint is below
                    //endpoint below sends back data equivilant to that in the PBA json schema
                    string url = "https://api.dropboxapi.com/2/team_log/get_events";

                    //instantiating a webrequest and passing the target endpoint url from above into it
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));

                    //setting appropriate headers for the HTTP request
                    request.ContentType = "application/json";
                    request.Headers.Add("Authorization", "Bearer U_SIBSKdmXAAAAAAAAAARudSisk36OLSijZmON3B1AlLUg2TMUkRLc-EcoFmzeL-");

                    request.Method = "POST";    //Dropbox's API communicates using the POST method, so that header is set appropriately

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

                            //response written to file
                            string path = @"c:\Users\acoggin16\Desktop\DropboxData" + timeStamp + ".json.gz";

                            if (!File.Exists(path))
                            {
                                File.WriteAllText(path, json);
                                Console.WriteLine("You have written the JSON response to a file successfully!");

                                //gets the new cursor from the file you just created and stores it in the cursor file
                                string cursor = GetNewCursor();
                                StoreCursor(cursor);
                            }
                            else
                            {
                                Console.WriteLine("A file in this path already exists!");
                                Console.ReadLine();
                            }
                            return true;
                        }
                    }
                }
                else
                {
                    //passing the target URL into a string. Target endpoint is below
                    string url = "https://api.dropboxapi.com/2/team_log/get_events/continue";

                    //instantiating a webrequest and passing the target endpoint url from above into it
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));

                    //setting appropriate headers for the HTTP request
                    request.ContentType = "application/json";
                    request.Headers.Add("Authorization", "Bearer U_SIBSKdmXAAAAAAAAAARudSisk36OLSijZmON3B1AlLUg2TMUkRLc-EcoFmzeL-");


                    request.Method = "POST";    //Dropbox's API communicates using the POST methos, so that header is set appropriately

                    //Data requested is then fetched and stored
                    string cursor = GetOldCursor();
                    string postData = "{" + cursor + "}";

                    //section of code that gets the data and writes it out to a stream
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

                            //create the timestamp to be a part of the data's file name
                            //DateTime ts = DateTime.Now;
                            //String timeStamp = String.Format("{0:MM-dd-yyyy}", ts);

                            //response written to file
                            string path = @"c:\Users\acoggin16\Desktop\DropboxData" + timeStamp + ".json.gz";

                            if (!File.Exists(path))
                            {
                                File.WriteAllText(path, json);
                                Console.WriteLine("You have written the JSON response to a file successfully!");

                                //gets the new cursor from the file you just created and stores it in the cursor file
                                cursor = GetNewCursor();
                                StoreCursor(cursor);
                            }
                            else
                            {
                                Console.WriteLine("A file in this path already exists!");
                                Console.ReadLine();
                            }
                            return true;
                        }
                    }
                }
            }

            //opens the cursor file and saves the cursor to the oldCursor variable that is returned when GetOldCursor is called
            public static String GetOldCursor()
            {
                string oldCursor;
                if (File.Exists(cursorPath))
                {
                    oldCursor = File.ReadAllText(cursorPath);
                }
                else   //handles the error if we somehow end up here without the cursor file existing
                {
                    Console.WriteLine("Running GetOldCursor, but no Curson.json.gz file exists");
                    oldCursor = "null";
                }
                return oldCursor;
            }

            //opens the JSON file of all the data we just created and pulls out the cursor
            public static String GetNewCursor()
            {
                string newCursor;

                //create the timestamp to be a part of the data's file name
                //DateTime ts = DateTime.Now;
                //String timeStamp = String.Format("{0:MM-dd-yyyy}", ts);
                string path = @"c:\Users\acoggin16\Desktop\DropboxData" + timeStamp + ".json.gz";
                if (File.Exists(path))
                {
                    List<string> listJSON = new List<string>();
                    foreach (string line in File.ReadLines(path))
                    {
                        listJSON.Add(line);
                    }
                    //gets the second to last line which is where the cursor sits
                    string firstCursor = listJSON[listJSON.Count - 3];
                    //there is a comma at the end of the cursor that must be taken off
                    newCursor = firstCursor.TrimEnd(firstCursor[firstCursor.Length - 1]);
                }
                else
                {
                    Console.WriteLine("Running GetNewCursor but the file with the JSON data does not exist");
                    newCursor = "null";
                }
                return newCursor;
            }

            //saves the new cursor to the cursor file, overwrites if it already exists, or creates the file if it doesn't
            public static void StoreCursor(String newCursor)
            {
                if (File.Exists(cursorPath))
                {
                    //has hit an error before but runs now
                    File.WriteAllText(cursorPath, newCursor);
                }
                else
                {
                    File.WriteAllText(cursorPath, newCursor);
                }
            }
        }
}
