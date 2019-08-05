using System;
using System.Collections.Generic;

namespace BYTDownloader
{
    class Queue
    {
        public Queue()
        {
            Console.WriteLine("Add links to the queue!");
            Console.WriteLine("If you're done than type: Done");
            
            List<string> tmp = new List<string>();

            while (true)
            {
                string y = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(y))
                {
                    if (y.Contains("Done"))
                    {
                        break;
                    }
                    else
                    {
                        tmp.Add(y);
                    }
                }
            }

            new DownloadQueue(tmp);
        }
    }
}
