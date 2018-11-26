using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebWhatsappBotCore
{
    public class MicroServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly Func<string, string> _responderMethod;

        public MicroServer(IReadOnlyCollection<string> prefixes, Func<string,string> method)
        {
            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("Old operating system");
            }

            if (prefixes == null || prefixes.Count == 0)
            {
                throw new ArgumentException("URI prefixes are required");
            }

            if (method == null)
            {
                throw new ArgumentException("responder method required");
            }

            foreach (var s in prefixes)
            {
                _listener.Prefixes.Add(s);
            }

            _responderMethod = method;
            _listener.Start();
        }

        public MicroServer(Func<String, string> method, params string[] prefixes)
         : this(prefixes, method)
      {
        }

        private string ProcessRequest(HttpListenerContext context)
        {
            var body = new StreamReader(context.Request.InputStream).ReadToEnd();

            return body;
        }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                Console.WriteLine("Webserver running...");
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem(c =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                if (ctx == null)
                                {
                                    return;
                                }

                                var body = ProcessRequest(ctx);

                                var rstr = _responderMethod(body);
                                var buf = Encoding.UTF8.GetBytes(body);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch
                            {
                              
                            }
                            finally
                            {
                               
                                if (ctx != null)
                                {
                                    ctx.Response.OutputStream.Close();
                                }
                            }
                        }, _listener.GetContext());
                    }
                }
                catch (Exception ex)
                {
                    // msg lost or bad request
                }
            });
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}
