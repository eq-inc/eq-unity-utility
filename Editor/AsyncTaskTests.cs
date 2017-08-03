using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eq.Unity
{
    class AsyncTaskTests
    {
        [Test]
        public void TestDelegateAsyncTask(
            DelegateAsyncTask<string, int, string>.OnPreExecuteDelegate onPreExecuteDelegate,
            DelegateAsyncTask<string, int, string>.DoInBackgroundDelegate doInBackgroundDelegate,
            DelegateAsyncTask<string, int, string>.OnPostExecuteDelegate onPostExecuteDelegate,
            DelegateAsyncTask<string, int, string>.OnProgressUpdateDelegate onProgressUpdateDelegate
        )
        {
            DelegateAsyncTask<string, int, string> task = new DelegateAsyncTask<string, int, string>(doInBackgroundDelegate);
            task.SetPreExecuteDelegator(onPreExecuteDelegate);
            task.SetProgressUpdateDelegator(onProgressUpdateDelegate);
            task.SetPostExecuteDelegator(onPostExecuteDelegate);
            task.Execute();
        }
    }
}
