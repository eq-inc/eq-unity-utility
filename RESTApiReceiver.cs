using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Eq.Unity
{
    public class RESTApiReceiver<V>
    {
        public delegate void Receiver(V response);
        private CommonRoutine mRoutine = new CommonRoutine();

        public void Get(string url, Receiver delegator)
        {
            CommunicationInfo<V> info = new CommunicationInfo<V>();
            info.url = url;
            info.delegator = delegator;
            mRoutine.StartCoroutine(CommunicateRoutine(info));
        }

        private IEnumerator CommunicateRoutine(CommunicationInfo<V> info)
        {
            UnityWebRequest request = new UnityWebRequest();
            request.url = info.url;
            request.downloadHandler = new DownloadHandlerBuffer();

            if(info.requestHeaders != null && info.requestHeaders.Count > 0)
            {
                foreach(KeyValuePair<string, string> header in info.requestHeaders)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            if(info.entityStream != null)
            {
                using (MemoryStream bufferStream = new MemoryStream())
                {
                    int readSize = 0;
                    byte[] readBuffer = new byte[4 * 1024];
    
                    while ((readSize = info.entityStream.Read(readBuffer, 0, 1)) > 0)
                    {
                        byte[] tempBuffer = readBuffer;
                        if(readSize < readBuffer.Length)
                        {
                            tempBuffer = new byte[readSize];
                            Array.Copy(readBuffer, tempBuffer, readSize);
                        }
                        bufferStream.Write(tempBuffer, 0, 1);
                    }
                    request.uploadHandler = new UploadHandlerRaw(bufferStream.ToArray());
                }
            }

            request.Send();
            while (!request.isDone)
            {
                yield return null;
            }

            if (info.delegator != null)
            {
                string contentTypeHD = request.GetResponseHeader("Content-type");
                string[] headerParams = contentTypeHD.Split(new char[] { ';' });
                Encoding useEncoding = Encoding.UTF8;

                if (headerParams.Length > 0)
                {
                    foreach (string headerParam in headerParams)
                    {
                        if (headerParam.Trim().StartsWith("charset="))
                        {
                            string[] paramKeyValue = headerParam.Split(new char[] { '=' });
                            if (paramKeyValue.Length > 1)
                            {
                                useEncoding = Encoding.GetEncoding(paramKeyValue[1]);
                            }
                            break;
                        }
                    }
                }

                V ret = JsonUtility.FromJson<V>(useEncoding.GetString(request.downloadHandler.data));
                info.delegator(ret);
            }
        }
    }

    internal class CommunicationInfo<V>
    {
        internal string url = null;
        internal Dictionary<string, string> requestHeaders = null;
        internal Stream entityStream = null;
        internal RESTApiReceiver<V>.Receiver delegator;
    }
}
