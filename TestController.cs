using Eq.Unity;
using System;

public class TestController : BaseAndroidMainController
{
    private ITest[] mTests = new ITest[]
    {
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
            string apiKey = "正しい値を設定してから試験してください";
            GoogleMapsAPI api = new GoogleMapsAPI(apiKey);
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

            api = new GoogleMapsAPI(apiKey);
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

            api = new GoogleMapsAPI(apiKey);
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

            api = new GoogleMapsAPI(apiKey);
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
