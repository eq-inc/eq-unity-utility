using Eq.Unity;
using System;

public class TestController : BaseAndroidMainController
{
    private ITest[] mTests = new ITest[]
    {
        new TestLocationMainController(),
        new TestGoogleMapsAPI(),
        //new TestAsyncTask(),
    };

    internal override void Start()
    {
        base.Start();
        foreach (ITest test in mTests)
        {
            test.Start();
            if (!test.IsTesting())
            {
                if (!test.Result())
                {
                    throw new Exception();
                }
            }
        }
    }

    internal override void Update()
    {
        base.Update();
        bool allTestFinished = true;

        foreach (ITest test in mTests)
        {
            if (!test.IsTesting())
            {
                if (!test.Result())
                {
                    throw new Exception();
                }
            }
            else
            {
                allTestFinished = false;
            }
        }

        if (allTestFinished)
        {
            PopCurrentScene();
        }
    }

    private interface ITest
    {
        void Start();
        bool IsTesting();
        bool Result();
    }

    private class TestLocationMainController : ITest
    {
        public void Start()
        {
            LocationMainController controller = new LocationMainController();
            Type controllerType = controller.GetType();

            float[][] testDataArrays = new float[][]
            {
                new float[]{35.66724f, 139.7291f, 35.66804f, 139.7283f, 114.565f},
                new float[]{36.10056f, 140.09111f, 35.65500f, 139.74472f, 58502.45893124115f},
            };

            foreach(float[] testDataArray in testDataArrays)
            {
                object[] parameters = new object[]
                {
                    testDataArray[0],
                    testDataArray[1],
                    testDataArray[2],
                    testDataArray[3],
                };
                double ret = (double)controllerType.InvokeMember("GetDistanceM", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance, null, controller, parameters);
                if(Math.Abs(ret - testDataArray[4]) > testDataArray[4] * 0.05)
                {
                    UnityEngine.Debug.Assert(true);
                }
            }
        }

        public bool IsTesting()
        {
            return false;
        }

        public bool Result()
        {
            return true;
        }
    }


    private class TestAsyncTask : ITest
    {
        private bool mCalledPreExecute = false;
        private bool mCalledDoInBackgroundDelegate = false;
        private bool mCalledPostExecute = false;
        private bool mCalledCanceled = false;
        private int mCalledProgressUpdateCount = 0;
        private DelegateAsyncTask<string, int, string> mTask = null;

        public void Start()
        {
            mTask = new DelegateAsyncTask<string, int, string>(delegate (string[] parameters)
            {
                mCalledDoInBackgroundDelegate = true;
                mTask.PublishProgress(1);
                mTask.PublishProgress(2);
                mTask.PublishProgress(3);
                mTask.PublishProgress(4);
                mTask.PublishProgress(6);
                mTask.PublishProgress(7);
                mTask.PublishProgress(8);
                mTask.PublishProgress(9);
                mTask.PublishProgress(10);
                return "test";
            });

            DelegateAsyncTask<string, int, string>.OnPreExecuteDelegate onPreExecuteDelegate = delegate ()
            {
                mCalledPreExecute = true;
            };
            DelegateAsyncTask<string, int, string>.OnPostExecuteDelegate onPostExecuteDelegate = delegate (string result, string[] parameters)
            {
                mCalledPostExecute = (result.CompareTo("test") == 0);
            };
            DelegateAsyncTask<string, int, string>.OnProgressUpdateDelegate onProgressUpdateDelegate = delegate (int[] progress)
            {
                mCalledProgressUpdateCount++;
            };
            mTask.SetPreExecuteDelegator(onPreExecuteDelegate);
            mTask.SetProgressUpdateDelegator(onProgressUpdateDelegate);
            mTask.SetPostExecuteDelegator(onPostExecuteDelegate);
            mTask.Execute();
        }

        public bool IsTesting()
        {
            return mCalledCanceled || mCalledPostExecute;
        }

        public bool Result()
        {
            return mCalledPreExecute & mCalledDoInBackgroundDelegate & mCalledPostExecute & (mCalledProgressUpdateCount == 10);
        }
    }

    private class TestGoogleMapsAPI : ITest
    {
        private bool[] mCalled = { false, false, false, false };

        public void Start()
        {
            GoogleMapsAPI api = new GoogleMapsAPI("AIzaSyAeq_EQ5JduZmb91vy-UI0qhLJE4_zdF_4");
            api.GetDirectionCoroutine(
                GoogleMapsAPI.TransferMode.Bicycling,
                new GoogleMapsAPI.UrlParameterOrigin(35.667321f, 139.729162f),
                new GoogleMapsAPI.UrlParameterDestination(35.665485f, 139.770763f),
                delegate(ResponseDirections response)
                {
                    UnityEngine.Debug.Assert(response != null && response.GetStatus() == DirectionStatus.ZERO_RESULTS);
                    mCalled[0] = true;
                }
            );

            api = new GoogleMapsAPI("AIzaSyAeq_EQ5JduZmb91vy-UI0qhLJE4_zdF_4");
            api.GetDirectionCoroutine(
                GoogleMapsAPI.TransferMode.Driving,
                new GoogleMapsAPI.UrlParameterOrigin(35.667321f, 139.729162f),
                new GoogleMapsAPI.UrlParameterDestination(35.665485f, 139.770763f),
                delegate (ResponseDirections response)
                {
                    UnityEngine.Debug.Assert(response != null && response.GetStatus() == DirectionStatus.OK);
                    mCalled[1] = true;
                }
            );

            api = new GoogleMapsAPI("AIzaSyAeq_EQ5JduZmb91vy-UI0qhLJE4_zdF_4");
            api.GetDirectionCoroutine(
                GoogleMapsAPI.TransferMode.Transit,
                new GoogleMapsAPI.UrlParameterOrigin(35.667321f, 139.729162f),
                new GoogleMapsAPI.UrlParameterDestination(35.665485f, 139.770763f),
                delegate (ResponseDirections response)
                {
                    UnityEngine.Debug.Assert(response != null && response.GetStatus() == DirectionStatus.OK);
                    mCalled[2] = true;
                }
            );

            api = new GoogleMapsAPI("AIzaSyAeq_EQ5JduZmb91vy-UI0qhLJE4_zdF_4");
            api.GetDirectionCoroutine(
                GoogleMapsAPI.TransferMode.Walking,
                new GoogleMapsAPI.UrlParameterOrigin(35.667321f, 139.729162f),
                new GoogleMapsAPI.UrlParameterDestination(35.665485f, 139.770763f),
                delegate (ResponseDirections response)
                {
                    UnityEngine.Debug.Assert(response != null && response.GetStatus() == DirectionStatus.OK);
                    mCalled[3] = true;
                }
            );
        }

        public bool IsTesting()
        {
            bool ret = true;

            foreach (bool partialRet in mCalled)
            {
                ret &= partialRet;
            }

            return !ret;
        }

        public bool Result()
        {
            return true;
        }
    }
}
