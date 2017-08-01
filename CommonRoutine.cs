using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eq.Unity
{
    public class CommonRoutine
    {
        private static GameObject sRoutineGameObject;
        private static AsyncTaskComponent sRoutineComponent;
        private static Dictionary<object, object> sUserDic = new Dictionary<object, object>();

        public void StartCoroutine(IEnumerator enumerator)
        {
            lock (sUserDic)
            {
                if (sRoutineGameObject == null)
                {
                    sRoutineGameObject = new GameObject();
                    sRoutineGameObject.name = "GameObject_for_Routine";
                    sRoutineComponent = sRoutineGameObject.AddComponent<AsyncTaskComponent>();
                }
                sUserDic.Add(this, this);
                sRoutineComponent.StartCoroutine(enumerator);
            }
        }

        public void DestroyComponent()
        {
            lock (sUserDic)
            {
                sUserDic.Remove(this);

                if((sUserDic.Count == 0) && (sRoutineGameObject != null))
                {
                    UnityEngine.Object.Destroy(sRoutineGameObject);
                    sRoutineGameObject = null;
                    sRoutineComponent = null;
                }
            }
        }
    }

    class AsyncTaskComponent : BaseAndroidBehaviour
    {
        // 処理なし
    }
}
