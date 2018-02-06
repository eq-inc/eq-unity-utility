using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Eq.Unity
{
    abstract public class AsyncTask<Param, Progress, Result>
    {
        private Thread mWorkThread;
        private Param[] mParameters;
        private Result mResult;
        private bool mCanceled = false;
        private bool mFinished = false;
        private ManualResetEvent mEvent = new ManualResetEvent(false);
        private Queue<Progress[]> mProgressValueQueue;
        private CommonRoutine mRoutine = new CommonRoutine();
        internal LogController mLogger = new LogController();

        abstract internal Result DoInBackground(params Param[] parameters);

        public void CopyLogController(LogController copyFrom)
        {
            mLogger.CopyFrom(copyFrom);
        }

        public void Execute(params Param[] parameters)
        {
            mLogger.CategoryLog(LogController.LogCategoryMethodIn);

            if (mWorkThread == null)
            {
                mLogger.CategoryLog(LogController.LogCategoryMethodTrace, "call OnPreExecute");
                OnPreExecute();
                mWorkThread = new Thread(this.ParameterizedThreadStart);
                mLogger.CategoryLog(LogController.LogCategoryMethodTrace, "start work thread");
                mWorkThread.Start(parameters);

                mRoutine.StartCoroutine(Looper());
            }

            mLogger.CategoryLog(LogController.LogCategoryMethodOut);
        }

        public void Cancel()
        {
            mLogger.CategoryLog(LogController.LogCategoryMethodIn);
            mCanceled = true;

            if (!mFinished)
            {
                mWorkThread.Abort();
            }
            mLogger.CategoryLog(LogController.LogCategoryMethodOut);
        }

        public Result Get()
        {
            mLogger.CategoryLog(LogController.LogCategoryMethodIn);
            mEvent.Reset();
            if (!mFinished)
            {
                mEvent.WaitOne();
            }
            mEvent.Set();

            mLogger.CategoryLog(LogController.LogCategoryMethodOut, mResult);
            return mResult;
        }

        public bool IsCanceled()
        {
            mLogger.CategoryLog(LogController.LogCategoryMethodIn);
            mLogger.CategoryLog(LogController.LogCategoryMethodOut, mCanceled);
            return mCanceled;
        }

        private void ParameterizedThreadStart(object obj)
        {
            mLogger.CategoryLog(LogController.LogCategoryMethodIn);
            mEvent.Reset();
            mParameters = (Param[])obj;
            mResult = DoInBackground(mParameters);
            mFinished = true;
            mEvent.Set();
            mLogger.CategoryLog(LogController.LogCategoryMethodOut);
        }

        internal virtual void OnCancelled()
        {

        }

        internal virtual void OnCancelled(Result result, params Param[] parameters)
        {

        }

        internal virtual void OnPreExecute()
        {

        }

        internal virtual void OnProgressUpdate(params Progress[] values)
        {

        }

        internal virtual void OnPostExecute(Result result, params Param[] parameters)
        {

        }

        public void PublishProgress(params Progress[] values)
        {
            if (mProgressValueQueue == null)
            {
                mProgressValueQueue = new Queue<Progress[]>();
            }

            mProgressValueQueue.Enqueue(values);
        }

        private IEnumerator Looper()
        {
            mLogger.CategoryLog(LogController.LogCategoryMethodIn);
            while (!mFinished)
            {
                yield return OnProgressUpdateForCoroutine();
                yield return null;
            }

            if (mCanceled)
            {
                if (mFinished)
                {
                    yield return OnCancelledForCoroutine(mResult, mParameters);
                }
                else
                {
                    yield return OnCancelledForCoroutine();
                }
            }
            else
            {
                yield return OnPostExecuteForCoroutine(mResult, mParameters);
            }

            mLogger.CategoryLog(LogController.LogCategoryMethodOut);
            yield return DestroyComponentForCoroutine();
        }

        private System.Object OnProgressUpdateForCoroutine()
        {
            if(mProgressValueQueue != null)
            {
                while (mProgressValueQueue.Count > 0)
                {
                    OnProgressUpdate(mProgressValueQueue.Dequeue());
                }
            }
            return new System.Object();
        }

        private System.Object OnCancelledForCoroutine()
        {
            mLogger.CategoryLog(LogController.LogCategoryMethodIn);
            OnCancelled();
            mLogger.CategoryLog(LogController.LogCategoryMethodOut);
            return new System.Object();
        }

        private System.Object OnCancelledForCoroutine(Result result, params Param[] parameters)
        {
            mLogger.CategoryLog(LogController.LogCategoryMethodIn);
            OnCancelled(result, parameters);
            mLogger.CategoryLog(LogController.LogCategoryMethodOut);
            return new System.Object();
        }

        private System.Object OnPostExecuteForCoroutine(Result result, params Param[] parameters)
        {
            mLogger.CategoryLog(LogController.LogCategoryMethodIn);
            OnPostExecute(result, parameters);
            mLogger.CategoryLog(LogController.LogCategoryMethodOut);
            return new System.Object();
        }

        private System.Object DestroyComponentForCoroutine()
        {
            mLogger.CategoryLog(LogController.LogCategoryMethodIn);
            mRoutine.DestroyComponent();
            mLogger.CategoryLog(LogController.LogCategoryMethodOut);
            return new System.Object();
        }
    }

    public class CallbackAsncTask<Param, Progress, Result> : AsyncTask<Param, Progress, Result>
    {
        private ICallback mCallback;

        public CallbackAsncTask(ICallback callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException();
            }
            mCallback = callback;
        }

        internal override void OnCancelled()
        {
            ICancelCallback callback = mCallback as ICancelCallback;
            if (callback != null)
            {
                callback.OnCancelled();
            }
        }

        internal override void OnCancelled(Result result, params Param[] parameters)
        {
            ICancelCallback callback = mCallback as ICancelCallback;
            if (callback != null)
            {
                callback.OnCancelled(result, parameters);
            }
        }

        internal override void OnPreExecute()
        {
            IResultCallback callback = mCallback as IResultCallback;
            if (callback != null)
            {
                callback.OnPreExecute();
            }
        }

        internal override void OnProgressUpdate(params Progress[] values)
        {
            IResultCallback callback = mCallback as IResultCallback;
            if (callback != null)
            {
                callback.OnProgressUpdate();
            }
        }

        internal override Result DoInBackground(params Param[] parameters)
        {
            return mCallback.DoInBackground(parameters);
        }

        internal override void OnPostExecute(Result result, params Param[] parameters)
        {
            IResultCallback callback = mCallback as IResultCallback;
            if (callback != null)
            {
                callback.OnPostExecute(result, parameters);
            }
        }

        public interface ICallback
        {
            Result DoInBackground(params Param[] parameters);
        }

        public interface IResultCallback : ICallback
        {
            void OnPreExecute();
            void OnProgressUpdate(params Progress[] values);
            void OnPostExecute(Result result, params Param[] parameters);
        }

        public interface ICancelCallback : ICallback
        {
            void OnCancelled();
            void OnCancelled(Result result, params Param[] parameters);
        }

        public interface IFullCallback : ICallback, IResultCallback, ICancelCallback
        {
        }
    }

    public class DelegateAsyncTask<Param, Progress, Result> : AsyncTask<Param, Progress, Result>
    {
        public delegate void OnPreExecuteDelegate();
        public delegate Result DoInBackgroundDelegate(params Param[] parameters);
        public delegate void OnProgressUpdateDelegate(params Progress[] values);
        public delegate void OnPostExecuteDelegate(Result result, params Param[] parameters);
        public delegate void OnCancelledDelegate();
        public delegate void OnCancelledAfterFinishedDelegate(Result result, params Param[] parameters);

        private OnPreExecuteDelegate mOnPreExecute;
        private DoInBackgroundDelegate mDoInBackground;
        private OnProgressUpdateDelegate mOnProgressUpdate;
        private OnPostExecuteDelegate mOnPostExecute;
        private OnCancelledDelegate mOnCancelled;
        private OnCancelledAfterFinishedDelegate mOnCancelledAfterFinished;

        public DelegateAsyncTask(DoInBackgroundDelegate doInBackground)
        {
            if (doInBackground == null)
            {
                throw new ArgumentNullException();
            }
            mDoInBackground = doInBackground;
        }

        public void SetPreExecuteDelegator(OnPreExecuteDelegate onPreExecuteDelegate)
        {
            mOnPreExecute = onPreExecuteDelegate;
        }

        public void SetProgressUpdateDelegator(OnProgressUpdateDelegate onProgressUpdateDelegate)
        {
            mOnProgressUpdate = onProgressUpdateDelegate;
        }

        public void SetPostExecuteDelegator(OnPostExecuteDelegate onPostExecuteDelegate)
        {
            mOnPostExecute = onPostExecuteDelegate;
        }

        public void SetCancelDelegator(OnCancelledDelegate onCancelledDelegate)
        {
            mOnCancelled = onCancelledDelegate;
        }

        public void SetCancelAfterFinishedDelegator(OnCancelledAfterFinishedDelegate onCancelledAfterFinishedDelegate)
        {
            mOnCancelledAfterFinished = onCancelledAfterFinishedDelegate;
        }

        internal override void OnCancelled()
        {
            if (mOnCancelled != null)
            {
                mOnCancelled();
            }
        }

        internal override void OnCancelled(Result result, params Param[] parameters)
        {
            if (mOnCancelledAfterFinished != null)
            {
                mOnCancelledAfterFinished(result, parameters);
            }
        }

        internal override void OnPreExecute()
        {
            if (mOnPreExecute != null)
            {
                mOnPreExecute();
            }
        }

        internal override void OnProgressUpdate(params Progress[] values)
        {
            if (mOnProgressUpdate != null)
            {
                mOnProgressUpdate();
            }
        }

        internal override Result DoInBackground(params Param[] parameters)
        {
            return mDoInBackground(parameters);
        }

        internal override void OnPostExecute(Result result, params Param[] parameters)
        {
            if (mOnPostExecute != null)
            {
                mOnPostExecute(result, parameters);
            }
        }
    }
}
