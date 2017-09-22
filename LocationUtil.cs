using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eq.Unity
{
    public class LocationUtil
    {
        private const double LongRadiusM = 6378137.000;     // a
        private const double ShortRadiusM = 6356752.314245;    // b
        private const double MajorEccentricityPow2 = 0.00669437999019758;   // 第一離心率e^2

        public static double GetDistanceM(float latDg1, float lngDg1, float latDg2, float lngDg2)
        {
            float lat1 = (float)((latDg1 * Math.PI) / 180);
            float lng1 = (float)((lngDg1 * Math.PI) / 180);
            float lat2 = (float)((latDg2 * Math.PI) / 180);
            float lng2 = (float)((lngDg2 * Math.PI) / 180);

            float dx = lng2 - lng1;             // dx: 緯度差
            float dy = lat2 - lat1;             // dy: 経度差
            double uy = (lat1 + lat2) / 2;      // uy: 緯度の平均
                                                // W = √{1 - e^2 * sin^2(uy)}
            double W = Math.Sqrt(1 - MajorEccentricityPow2 * Math.Pow(Math.Sin(uy), 2));
            // M = {a * (1 - e^2)} / W^3
            double M = (LongRadiusM * (1 - MajorEccentricityPow2)) / Math.Pow(W, 3);
            // N = a / W
            double N = LongRadiusM / W;

            // d = √{(dy * M)^2 + (dx * N * cos(uy))^2}
            return Math.Sqrt(Math.Pow(dy * M, 2) + Math.Pow(dx * N * Math.Cos(uy), 2));
        }

        public static float GetRadianByLatitudeLine(double lat1, double lng1, double lat2, double lng2)
        {
            double lat12 = GetDistanceM((float)lat1, (float)lng1, (float)lat2, (float)lng1);
            double lng12 = GetDistanceM((float)lat1, (float)lng1, (float)lat1, (float)lng2);

            if (lat1 > lat2)
            {
                lat12 *= -1;
            }
            if (lng1 > lng2)
            {
                lng12 *= -1;
            }

            float tempRet = (float)Math.Atan2(lat12, lng12);
            float ret = tempRet;
            if ((lat12 >= 0) && (lng12 >= 0))
            {
                // 補正なし
            }
            else
            {
                if ((lat12 < 0) && (lng12 < 0))
                {
                    ret += (float)Math.PI;
                }
                else if (lat12 < 0)
                {
                    ret = (float)(Math.PI - tempRet);
                }
                else if (lng12 < 0)
                {
                    ret = (float)(2 * Math.PI - tempRet);
                }
            }

            return tempRet;
        }

        public static float DegreeToRadian(float degree)
        {
            return (float)(degree * Math.PI / 180);
        }

        public static float RadianToDegree(float radian)
        {
            return (float)(radian * 180 / Math.PI);
        }
    }
}
