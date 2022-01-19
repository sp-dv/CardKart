using CardKartShared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CardKartServer.Vitaliks
{
    internal class TokenMetadataEndpoint
    {
        public static void XD()
        {
            if (true)
            {
                var rp = new TcpListener(new IPEndPoint(IPAddress.Any, 80));
                rp.Start();

                while (true)
                {
                    var cl = rp.AcceptTcpClient();

                    var s = cl.GetStream();
                    var rpbuf = new byte[1024];
                    var read = s.Read(rpbuf, 0, rpbuf.Length);
                    var str1 = Encoding.UTF8.GetString(rpbuf.Take(read).ToArray());

                    var ss = str1.Split('\n').Select(sx => sx.Trim()).ToArray();

                    var s1 = ss[0];
                    var s1split = s1.Split(' ');
                    if (s1split.Length != 3) { throw new NotImplementedException(); }

                    var method = s1split[0];
                    var page = s1split[1];
                    var httpVersion = s1split[2];

                    var args = new Dictionary<string, string>();

                    foreach (var sx in ss.Skip(1))
                    {
                        var split = sx.Split(':');
                        if (split.Length == 2)
                        {
                            args[split[0]] = split[1];
                        }
                    }

                    var code = 200;
                    var codeStr = "OK";

                    var text = "{\"EXAMPLE_BOOLEAN\": true, \"TEST_VAL\": \"XdD\"}";
                    var contentLength = text.Length;

                    var now = DateTime.UtcNow;
                    var dateString = $"{now.ToLongDateString()} {now.ToShortTimeString()} GMT";

                    var sb = new StringBuilder();
                    sb.Append($"{httpVersion} {code} {codeStr}\r\n");
                    sb.Append($"Content-Length: {contentLength}\r\n");
                    sb.Append($"Server: yerdad\r\n");
                    sb.Append($"Date: {dateString}\r\n");
                    sb.Append("\r\n");
                    sb.Append(text);

                    var rsx = sb.ToString();
                    rpbuf = Encoding.UTF8.GetBytes(rsx);

                    s.Write(rpbuf, 0, rpbuf.Length);
                    var str2 = Encoding.UTF8.GetString(rpbuf.Take(read).ToArray());
                    cl.Close();

                }
            }
            else
            {
                var hl = new HttpListener();
                hl.Prefixes.Add("http://localhost:8081/");
                if (!HttpListener.IsSupported) { Logging.Log(LogLevel.Error, "HttpListener is not supported."); }
                hl.Start();

                var rp = new TcpListener(new IPEndPoint(IPAddress.Any, 80));
                rp.Start();

                while (true)
                {
                    var cl = rp.AcceptTcpClient();

                    var s = cl.GetStream();
                    var rpbuf = new byte[1024];
                    var read = s.Read(rpbuf, 0, rpbuf.Length);
                    var str1 = Encoding.UTF8.GetString(rpbuf.Take(read).ToArray());
                    var rcon = new TcpClient("localhost", 8081);
                    var rcons = rcon.GetStream();
                    var rbuf = Encoding.UTF8.GetBytes(str1);
                    rcons.Write(rbuf, 0, rbuf.Length);

                    var context = hl.GetContext();

                    var request = context.Request;
                    var response = context.Response;
                    string responseString = "<HTML><BODY> We did it reddit </BODY></HTML>";
                    byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    var output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();

                    read = rcons.Read(rpbuf, 0, rpbuf.Length);
                    s.Write(rpbuf, 0, read);
                    var str2 = Encoding.UTF8.GetString(rpbuf.Take(read).ToArray());
                    cl.Close();


                    
                    Logging.Log(LogLevel.Debug, str1);
                    Logging.Log(LogLevel.Debug, str2);
                }
            }
        }
    }
}
