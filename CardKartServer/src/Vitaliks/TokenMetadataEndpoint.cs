﻿using CardKartShared.Util;
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


            var hl = new HttpListener();
            hl.Prefixes.Add("http://localhost:8081/");
            if (!HttpListener.IsSupported) { Logging.Log(LogLevel.Error, "HttpListener is not supported."); }
            Logging.Log(LogLevel.Debug, "D");
            hl.Start();

            var rp = new TcpListener(new IPEndPoint(IPAddress.Any, 80));
            rp.Start();
            var cl = rp.AcceptTcpClient();
            var s = cl.GetStream();
            var rpbuf = new byte[1024];
            var read = s.Read(rpbuf, 0, rpbuf.Length);
            var str = Encoding.UTF8.GetString(rpbuf.Take(read).ToArray());
            int xd = 4;

            var rcon = new TcpClient("localhost", 8081);
            var rcons = rcon.GetStream();
            var rbuf = Encoding.UTF8.GetBytes(str);
            rcons.Write(rbuf, 0, rbuf.Length);

            var context = hl.GetContext();

            var request = context.Request;
            var response = context.Response;
            string responseString = "<HTML><BODY> We did it reddit </BODY></HTML>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // Must close the output stream.
            output.Close();

            var read2 = rcons.Read(rpbuf, 0, rpbuf.Length);
            s.Write(rpbuf, 0, read2);
            cl.Close();

            int i = 4;
        }
    }
}