// <copyright company="INSTEON">
// Copyright (c) 2012 All Right Reserved, http://www.insteon.net
//
// This source is subject to the Common Development and Distribution License (CDDL). 
// Please see the LICENSE.txt file for more information.
// All other rights reserved.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// </copyright>
// <author>Dave Templin</author>
// <email>info@insteon.net</email>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Insteon.Network
{
    // This class is responsible communicating with the Smarthome web service to obtain the list of registered devices on the local network.
    internal static class SmartLincFinder
    {
        public static SmartLincInfo[] GetRegisteredSmartLincs()
        {
            List<SmartLincInfo> list = new List<SmartLincInfo>();

            string html = GetHtml("http://smartlinc.smarthome.com/getinfo.asp");
            if (!string.IsNullOrEmpty(html))
            {
                Regex hrefPattern = new Regex(@"href=""(?<url>[^""]+)""");
                Regex addressPattern = new Regex(@"<font[^>]*>(?<address>[0-9A-Fa-f]{2}\.[0-9A-Fa-f]{2}\.[0-9A-Fa-f]{2})</font>");
                Match m1 = hrefPattern.Match(html);
                while (m1.Success)
                {
                    Match m2 = addressPattern.Match(html, m1.Index);
                    if (m2.Success)
                    {
                        string url = m1.Groups["url"].ToString();
                        string address = m2.Groups["address"].ToString();
                        try
                        {
                            list.Add(new SmartLincInfo(url, address));
                        }
                        catch (FormatException)
                        {
                        }
                        catch (ArgumentException)
                        {
                        }
                    }
                    m1 = m1.NextMatch();
                }
            }
                        
            return list.ToArray();
        }

        public static string GetSmartLincName(string url)
        {
            string html = GetHtml(url);
            if (!string.IsNullOrEmpty(html))
            {
                Regex namePattern = new Regex(@"HA\[0\]\s*=\s*""(?<name>[^""]+)""");
                Match m = namePattern.Match(html);
                if (m.Success)
                    return m.Groups["name"].ToString();
            }
            return string.Empty;
        }

        private static string GetHtml(string url)
        {
            try
            {
                string html;
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Timeout = Constants.webRequestTimeout;
                WebResponse response = request.GetResponse();
                using (StreamReader r = new StreamReader(response.GetResponseStream()))
                {
                    html = r.ReadToEnd();
                    r.Close();
                }
                return html;
            }
            catch (WebException)
            {
            }
            catch (IOException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (FormatException)
            {
            }            
            return null;
        }
    }

    internal class SmartLincInfo
    {
        internal SmartLincInfo(string url, string address)
        {
            this.Uri = new Uri(url);
            this.InsteonAddress = InsteonAddress.Parse(address);
        }
        public InsteonAddress InsteonAddress { get; private set; }
        public Uri Uri { get; private set; }
    }
}
