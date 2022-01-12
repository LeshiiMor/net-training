using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncIO
{
    public static class Tasks
    {


        /// <summary>
        /// Returns the content of required uris.
        /// Method has to use the synchronous way and can be used to compare the performace of sync \ async approaches. 
        /// </summary>
        /// <param name="uris">Sequence of required uri</param>
        /// <returns>The sequence of downloaded url content</returns>
        public static IEnumerable<string> GetUrlContent(this IEnumerable<Uri> uris) 
        {
            using(WebClient client = new WebClient())
            {
                List<string> results = new List<string>();
                foreach (var url in uris)
                {
                    results.Add(client.DownloadString(url));
                }
                return results;
            }
            // TODO : Implement GetUrlContent
        }



        /// <summary>
        /// Returns the content of required uris.
        /// Method has to use the asynchronous way and can be used to compare the performace of sync \ async approaches. 
        /// 
        /// maxConcurrentStreams parameter should control the maximum of concurrent streams that are running at the same time (throttling). 
        /// </summary>
        /// <param name="uris">Sequence of required uri</param>
        /// <param name="maxConcurrentStreams">Max count of concurrent request streams</param>
        /// <returns>The sequence of downloaded url content</returns>
        public static IEnumerable<string> GetUrlContentAsync(this IEnumerable<Uri> uris, int maxConcurrentStreams)
        {
            // TODO : Implement GetUrlContentAsync
            List<string> data = new List<string>();
            WebClient client = new WebClient();
            Parallel.ForEach(uris,new ParallelOptions() {MaxDegreeOfParallelism = maxConcurrentStreams }, async url =>data.Add(await client.DownloadStringTaskAsync(url)));
            return data;
        }


        /// <summary>
        /// Calculates MD5 hash of required resource.
        /// 
        /// Method has to run asynchronous. 
        /// Resource can be any of type: http page, ftp file or local file.
        /// </summary>
        /// <param name="resource">Uri of resource</param>
        /// <returns>MD5 hash</returns>
        public static async Task<string> GetMD5Async(this Uri resource)
        {
            // TODO : Implement GetMD5Async
            //throw new NotImplementedException();
            WebClient client = new WebClient();
            MD5 md5 = MD5.Create();
            var result = await client.DownloadDataTaskAsync(resource);
            var hash = md5.ComputeHash(result);
            StringBuilder sOutput = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sOutput.Append(hash[i].ToString("X2"));
            }
            return sOutput.ToString();
        }
    }



}
