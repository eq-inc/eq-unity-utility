using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Eq.Unity
{
    public class GoogleMapsAPI
    {
        public enum TransferMode
        {
            Driving, Walking, Bicycling, Transit
        }

        internal static LogController Logger = new LogController();
        private const string UrlBaseDirections = "https://maps.googleapis.com/maps/api/directions/json?";

        public static void CopyLogController(LogController copyFrom)
        {
            Logger.CopyFrom(copyFrom);
        }

        private string mAPIKey;
        private CommonRoutine mRoutine = new CommonRoutine();

        public GoogleMapsAPI(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentNullException("api == null or api length is 0");
            }

            mAPIKey = apiKey;
        }

        [System.Obsolete("use GetDirectionCoroutine")]
        public ResponseDirections GetDirectionsFromCurrentPosition(TransferMode transferMode, string address)
        {
            Logger.CategoryLog(LogController.LogCategoryMethodIn);
            ResponseDirections ret = GetDirections(transferMode, new UrlParameterOrigin(Input.location.lastData.latitude, Input.location.lastData.longitude), new UrlParameterDestination(address));
            Logger.CategoryLog(LogController.LogCategoryMethodOut);

            return ret;
        }

        [System.Obsolete("use GetDirectionCoroutine")]
        public ResponseDirections GetDirectionsFromCurrentPosition(TransferMode transferMode, float latitude, float longitude)
        {
            Logger.CategoryLog(LogController.LogCategoryMethodIn);
            ResponseDirections ret = GetDirections(transferMode, new UrlParameterOrigin(Input.location.lastData.latitude, Input.location.lastData.longitude), new UrlParameterDestination(latitude, longitude));
            Logger.CategoryLog(LogController.LogCategoryMethodOut);

            return ret;
        }

        public void GetDirectionCoroutine(TransferMode transferMode, UrlParameterOrigin orgParameter, UrlParameterDestination destParameter, RESTApiReceiver<ResponseDirections>.Receiver receiver)
        {
            Logger.CategoryLog(LogController.LogCategoryMethodIn);

            StringBuilder urlBuilder = new StringBuilder(UrlBaseDirections);

            // modeパラメータ
            UrlParameterMode modeParameter = new UrlParameterMode(transferMode);
            urlBuilder.Append(modeParameter.GetName()).Append("=").Append(modeParameter.GetValue()).Append("&");

            // originパラメータ
            urlBuilder.Append(orgParameter.GetName()).Append("=").Append(orgParameter.GetValue()).Append("&");

            // destinationパラメータ
            urlBuilder.Append(destParameter.GetName()).Append("=").Append(destParameter.GetValue()).Append("&");

            // languageパラメータ
            UrlParameterLanguage languageParameter = new UrlParameterLanguage();
            urlBuilder.Append(languageParameter.GetName()).Append("=").Append(languageParameter.GetValue()).Append("&");

            // API key
            UrlParameterAPIKey apiKeyParameter = new UrlParameterAPIKey(mAPIKey);
            urlBuilder.Append(apiKeyParameter.GetName()).Append("=").Append(apiKeyParameter.GetValue());

            RESTApiReceiver<ResponseDirections> responseReceiver = new RESTApiReceiver<ResponseDirections>();
            responseReceiver.Get(urlBuilder.ToString(), receiver);

            Logger.CategoryLog(LogController.LogCategoryMethodOut);
        }

        [System.Obsolete("use GetDirectionCoroutine")]
        public ResponseDirections GetDirections(TransferMode transferMode, UrlParameterOrigin orgParameter, UrlParameterDestination destParameter)
        {
            Logger.CategoryLog(LogController.LogCategoryMethodIn);

            StringBuilder urlBuilder = new StringBuilder(UrlBaseDirections);
            UnityWebRequest request = new UnityWebRequest();

            // modeパラメータ
            UrlParameterMode modeParameter = new UrlParameterMode(transferMode);
            urlBuilder.Append(modeParameter.GetName()).Append("=").Append(modeParameter.GetValue()).Append("&");

            // originパラメータ
            urlBuilder.Append(orgParameter.GetName()).Append("=").Append(orgParameter.GetValue()).Append("&");

            // destinationパラメータ
            urlBuilder.Append(destParameter.GetName()).Append("=").Append(destParameter.GetValue()).Append("&");

            // languageパラメータ
            UrlParameterLanguage languageParameter = new UrlParameterLanguage();
            urlBuilder.Append(languageParameter.GetName()).Append("=").Append(languageParameter.GetValue()).Append("&");

            // API key
            UrlParameterAPIKey apiKeyParameter = new UrlParameterAPIKey(mAPIKey);
            urlBuilder.Append(apiKeyParameter.GetName()).Append("=").Append(apiKeyParameter.GetValue());

            request.url = urlBuilder.ToString();
            byte[] content = GetContent(request);
            string contentTypeHD = request.GetResponseHeader("Content-type");
            string[] headerParams = contentTypeHD.Split(new char[';']);
            Encoding useEncoding = Encoding.UTF8;

            if (headerParams.Length > 0)
            {
                foreach (string headerParam in headerParams)
                {
                    if (headerParam.StartsWith("charset="))
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
            Logger.CategoryLog(LogController.LogCategoryMethodTrace, "Content-type = " + contentTypeHD + ", recognized charset = " + useEncoding);

            ResponseDirections ret = JsonUtility.FromJson<ResponseDirections>(useEncoding.GetString(content));
            Logger.CategoryLog(LogController.LogCategoryMethodOut);
            return ret;
        }

        internal byte[] GetContent(UnityWebRequest request)
        {
            Logger.CategoryLog(LogController.LogCategoryMethodIn);
            byte[] ret = null;

            request.downloadHandler = new DownloadHandlerBuffer();
            request.Send();
            while (!request.isDone)
            {
                System.Threading.Thread.Sleep(1);
            }

            if (request.responseCode == 200)
            {
                ret = request.downloadHandler.data;
            }

            Logger.CategoryLog(LogController.LogCategoryMethodOut);
            return ret;
        }

        abstract public class UrlParameter
        {
            internal string mName;
            internal string mValue;

            public UrlParameter(string name)
            {
                Logger.CategoryLog(LogController.LogCategoryMethodIn);
                mName = name;
                Logger.CategoryLog(LogController.LogCategoryMethodOut);
            }

            public string GetName()
            {
                Logger.CategoryLog(LogController.LogCategoryMethodIn);
                Logger.CategoryLog(LogController.LogCategoryMethodOut, "name = " + mName);
                return mName;
            }

            public string GetValue()
            {
                Logger.CategoryLog(LogController.LogCategoryMethodIn);
                Logger.CategoryLog(LogController.LogCategoryMethodOut, "value = " + mValue);
                return mValue;
            }
        }

        public class UrlParameterOrigin : UrlParameter
        {
            public UrlParameterOrigin() : base("origin")
            {
                Logger.CategoryLog(LogController.LogCategoryMethodIn);
                if (Input.location.lastData.timestamp == 0)
                {
                    throw new Exception("cannot get location");
                }

                mValue = Input.location.lastData.latitude + "," + Input.location.lastData.longitude;
                Logger.CategoryLog(LogController.LogCategoryMethodOut, "value = " + mValue);
            }

            public UrlParameterOrigin(float srcLatitude, float srcLongitude) : base("origin")
            {
                Logger.CategoryLog(LogController.LogCategoryMethodIn);
                mValue = srcLatitude.ToString() + "," + srcLongitude.ToString();
                Logger.CategoryLog(LogController.LogCategoryMethodOut, "value = " + mValue);
            }

            public UrlParameterOrigin(string address) : base("origin")
            {
                Logger.CategoryLog(LogController.LogCategoryMethodIn);
                mValue = address;
                Logger.CategoryLog(LogController.LogCategoryMethodOut, "value = " + mValue);
            }
        }

        public class UrlParameterDestination : UrlParameter
        {
            public UrlParameterDestination(float destLatitude, float destLongitude) : base("destination")
            {
                Logger.CategoryLog(LogController.LogCategoryMethodIn);
                mValue = destLatitude.ToString() + "," + destLongitude.ToString();
                Logger.CategoryLog(LogController.LogCategoryMethodOut, "value = " + mValue);
            }

            public UrlParameterDestination(string address) : base("destination")
            {
                Logger.CategoryLog(LogController.LogCategoryMethodIn);
                mValue = address;
            }
        }

        public class UrlParameterAPIKey : UrlParameter
        {
            public UrlParameterAPIKey(string apiKey) : base("key")
            {
                Logger.CategoryLog(LogController.LogCategoryMethodIn);
                mValue = apiKey;
                Logger.CategoryLog(LogController.LogCategoryMethodOut, "value = XXXXX(not shown)");
            }
        }

        public class UrlParameterLanguage : UrlParameter
        {
            public UrlParameterLanguage() : this(Application.systemLanguage)
            {
            }

            public UrlParameterLanguage(SystemLanguage systemLanguage) : base("language")
            {
                Logger.CategoryLog(LogController.LogCategoryMethodIn, "language = " + systemLanguage);
                switch (systemLanguage)
                {
                    case SystemLanguage.Arabic: // アラビア語
                        mValue = "ar";
                        break;
                    case SystemLanguage.Basque: // バスク語
                        mValue = "eu";
                        break;
                    case SystemLanguage.Bulgarian:  // ブルガリア語
                        mValue = "bg";
                        break;
                    case SystemLanguage.Catalan:    // カタロニア語
                        mValue = "ca";
                        break;
                    case SystemLanguage.Czech:  // チェコ語
                        mValue = "cs";
                        break;
                    case SystemLanguage.Danish: // デンマーク語
                        mValue = "da";
                        break;
                    case SystemLanguage.Dutch:  // オランダ語
                        mValue = "nl";
                        break;
                    case SystemLanguage.English:    // 英語
                        mValue = "en";
                        break;
                    case SystemLanguage.Finnish:    // フィンランド語
                        mValue = "fi";
                        break;
                    case SystemLanguage.French: // フランス語
                        mValue = "fr";
                        break;
                    case SystemLanguage.German: // ドイツ語
                        mValue = "de";
                        break;
                    case SystemLanguage.Greek:  // ギリシャ語
                        mValue = "el";
                        break;
                    case SystemLanguage.Hebrew: // ヘブライ語
                        mValue = "iw";
                        break;
                    case SystemLanguage.Indonesian: // インドネシア語
                        mValue = "id";
                        break;
                    case SystemLanguage.Italian:    // イタリア語
                        mValue = "it";
                        break;
                    case SystemLanguage.Japanese:   // 日本語
                        mValue = "ja";
                        break;
                    case SystemLanguage.Korean: // 韓国語
                        mValue = "ko";
                        break;
                    case SystemLanguage.Latvian:    // ラトビア語
                        mValue = "lv";
                        break;
                    case SystemLanguage.Lithuanian: // リトアニア語
                        mValue = "lt";
                        break;
                    case SystemLanguage.Norwegian:  // ノルウェー語
                        mValue = "no";
                        break;
                    case SystemLanguage.Polish: // ポーランド語
                        mValue = "pl";
                        break;
                    case SystemLanguage.Portuguese: // ポルトガル語
                        mValue = "pt";
                        break;
                    case SystemLanguage.Romanian:   // ルーマニア語
                        mValue = "ro";
                        break;
                    case SystemLanguage.Russian:    // ロシア語
                    case SystemLanguage.Belarusian: // ベラルーシ語
                        mValue = "ru";
                        break;
                    case SystemLanguage.SerboCroatian:  // セルビアクロアチア語
                        mValue = "sr";
                        break;
                    case SystemLanguage.Slovak: // スロバキア語
                        mValue = "sk";
                        break;
                    case SystemLanguage.Slovenian:  // スロベニア語
                        mValue = "sl";
                        break;
                    case SystemLanguage.Spanish:    // スペイン語
                        mValue = "es";
                        break;
                    case SystemLanguage.Swedish:    // スウェーデン語
                        mValue = "sv";
                        break;
                    case SystemLanguage.Thai:   // タイ語
                        mValue = "th";
                        break;
                    case SystemLanguage.Turkish:    // トルコ語
                        mValue = "tr";
                        break;
                    case SystemLanguage.Ukrainian:  // ウクライナ語
                        mValue = "uk";
                        break;
                    case SystemLanguage.Vietnamese: // ベトナム語
                        mValue = "vi";
                        break;
                    case SystemLanguage.Chinese:    // 中国語
                    case SystemLanguage.ChineseSimplified:  // 中国語簡体字(simplified)
                        mValue = "zh-CN";
                        break;
                    case SystemLanguage.ChineseTraditional: // 中国語繁体字(traditional)
                        mValue = "zh-TW";
                        break;
                    case SystemLanguage.Hungarian:  // ハンガリー語
                        mValue = "hu";
                        break;

                    case SystemLanguage.Afrikaans:  // アフリカ語(非サポート)
                    case SystemLanguage.Faroese:    // フェロー語(非サポート)
                    case SystemLanguage.Icelandic:  // アイスランド語(非サポート)
                    case SystemLanguage.Estonian:   // エストニア語(非サポート)
                    case SystemLanguage.Unknown:    // 不明
                    default:
                        // 非サポートの言語は全て英語として表示
                        mValue = "en";
                        break;
                }
                Logger.CategoryLog(LogController.LogCategoryMethodOut, "value = " + mValue);
            }
        }

        public class UrlParameterMode : UrlParameter
        {
            public UrlParameterMode(TransferMode transferMode) : base("mode")
            {
                Logger.CategoryLog(LogController.LogCategoryMethodIn, "transferMode = " + transferMode);
                mValue = transferMode.ToString().ToLower();
                Logger.CategoryLog(LogController.LogCategoryMethodOut, "value = " + mValue);
            }
        }
    }

    public enum DirectionStatus
    {
        INIT, OK, NOT_FOUND, ZERO_RESULTS, MAX_WAYPOINTS_EXCEEDED, INVALID_REQUEST, OVER_QUERY_LIMIT, REQUEST_DENIED, UNKNOWN_ERROR
    }

    [Serializable]
    public class ResponseDirections
    {
        public GeocodedWaypoint[] geocoded_waypoints;
        public Route[] routes;
        public string status;
        private DirectionStatus mStatusEnum = DirectionStatus.INIT;

        public DirectionStatus GetStatus()
        {
            DirectionStatus ret = mStatusEnum;

            if (ret == DirectionStatus.INIT)
            {
                try
                {
                    ret = mStatusEnum = (DirectionStatus)Enum.Parse(ret.GetType(), status);
                }
                catch (Exception e)
                {
                    GoogleMapsAPI.Logger.CategoryLog(LogController.LogCategoryMethodError, e);
                }
            }

            return ret;
        }
    }

    [Serializable]
    public class GeocodedWaypoint
    {
        public string geocoder_status;
        public string place_id;
        public string[] types;
    }

    [Serializable]
    public class Route
    {
        public string summary;
        public Leg[] legs;
        public string copyrights;
        public Points overview_polyline;
        public int[] waypoint_order;
        public Bounds bounds;
    }

    [Serializable]
    public class Leg
    {
        public Step[] steps;
        public IntValue duration;
        public IntValue distance;
        public LatLng start_location;
        public LatLng end_location;
        public string start_address;
        public string end_address;
    }

    [Serializable]
    public class Step
    {
        public string travel_mode;
        public LatLng start_location;
        public LatLng end_location;
        public Points polyline;
        public IntValue duration;
        public string html_instructions;
        public IntValue distance;
    }

    [Serializable]
    public class LatLng
    {
        public float lat;
        public float lng;
    }

    [Serializable]
    public class IntValue
    {
        public int value;
        public string text;
    }

    [Serializable]
    public class Points
    {
        public string points;
    }

    [Serializable]
    public class Bounds
    {
        public LatLng southwest;
        public LatLng northeast;
    }
}
