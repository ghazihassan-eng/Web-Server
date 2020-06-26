using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;


namespace webserver
{
    class Program
    {
        public static string file = null;
        static void Main(string[] args)
        {
            Console.Title = "HTTP SERVER";

            Console.Write("Enter index path :");
            file = Console.ReadLine();

            Console.Write("enter working port :");

            int port = Convert.ToInt32(Console.ReadLine());

            string server = "http://localhost:"+port+"/";
            Console.Clear();

            webserver ws = new webserver(sendresponce ,server);
            ws.run();

            Console.WriteLine("http server is running :"+server);
            Console.ReadKey(true);

            ws.stop();
        }

        public static string sendresponce(HttpListenerRequest request)
        {
            return File.ReadAllText(file);
            
        }
    }

   public class webserver
    {
       private readonly HttpListener _listner = new HttpListener();
       private readonly Func<HttpListenerRequest, string> _responderMethod;

       public webserver(string [] prefixes, Func<HttpListenerRequest,string> method)
       {
           if(prefixes == null || prefixes.Length == 0)
           {
               throw new ArgumentException("prefixes");
           }
           if(method == null)
           {
               throw new ArgumentException("method");
           }

           foreach(string s in prefixes)
           {
               _listner.Prefixes.Add(s);
           }
           _responderMethod = method;
           _listner.Start();
       }

       public webserver(Func<HttpListenerRequest, string> method, params string[] prefixes) : this(prefixes, method) { }
       public void run()
       {
           ThreadPool.QueueUserWorkItem((o)=>
               {
                   try
                   {
                       while (_listner.IsListening)
                       {
                           ThreadPool.QueueUserWorkItem((c) =>
                               {
                                   var ctx = c as HttpListenerContext;
                                   try
                                   {
                                       string rstr = _responderMethod(ctx.Request);

                                       byte[] buf = Encoding.UTF8.GetBytes(rstr);

                                       ctx.Response.ContentLength64 = buf.Length;

                                       ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                                   }

                                   catch
                                   {

                                   }

                                   finally
                                   {
                                       ctx.Response.OutputStream.Close();
                                   }

                               }, _listner.GetContext());
                       }
                   }
                   catch
                   {

                   }

               });
       } 

           public void stop()
           {
               _listner.Stop();
               _listner.Close();
           }
    
   }

}
