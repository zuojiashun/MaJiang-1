using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using LuaInterface;

namespace Teacher.Manager {
    public class PanelManager : View {
        private Transform parent;

        Transform Parent {
            get {
                if (parent == null) {
                    GameObject go = GameObject.Find("GuiCamera");
                    if (go != null) parent = go.transform;
                }
                return parent;
            }
        }


#if ASYNC_MODE
        /// <summary>
        /// 创建面板，请求资源管理器
        /// </summary>
        /// <param name="type"></param>
        public void CreatePanel(string name, LuaFunction func = null) {
            StartCoroutine(OnCreatePanel(name, func));
        }

        IEnumerator OnCreatePanel(string name, LuaFunction func = null) {
            yield return StartCoroutine(Initialize());

            string assetName = name;
            assetName = assetName.ToLower();
            // Load asset from assetBundle.
            string abName = name.ToLower();
            AssetBundleAssetOperation request = ResourceManager.LoadAssetAsync(abName, assetName, typeof(GameObject));
            if (request == null) yield break;
            yield return StartCoroutine(request);

            // Get the asset.
            GameObject prefab = request.GetAsset<GameObject>();

            if (Parent.FindChild(name) != null || prefab == null) {
                yield break;
            }
            GameObject go = Instantiate(prefab) as GameObject;
            go.name = assetName;

            if (func != null) func.Call(go);
            Debug.Log("CreatePanel::>> " + name);
        }

        IEnumerator Initialize() 
        {
            ResourceManager.BaseDownloadingURL = Util.GetAssetBundlePath();
            // Initialize AssetBundleManifest which loads the AssetBundleManifest object.
            //var request = ResourceManager.Initialize(AppConst.AssetDirname);
            var request = ResourceManager.Initialize(Util.GetMainfestPathName());
            if (request != null)
                yield return StartCoroutine(request);
        }

        public void downLoadFile(LuaInterface.LuaTable evParams,LuaInterface.LuaFunction _UpdateProgress, LuaInterface.LuaFunction _DownLoadFinish)
        {
            ThreadManager.DownloadFile(evParams,_UpdateProgress,_DownLoadFinish);
        }
#else
        /// <summary>
        /// 创建面板，请求资源管理器
        /// </summary>
        /// <param name="type"></param>
        public void CreatePanel(string name, LuaFunction func = null) {
            string assetName = name + "Panel";
            GameObject prefab = ResManager.LoadAsset(name, assetName);
            if (Parent.FindChild(name) != null || prefab == null) {
                return;
            }
            GameObject go = Instantiate(prefab) as GameObject;
            go.name = assetName;
            go.layer = LayerMask.NameToLayer("Default");
            go.transform.SetParent(Parent);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;
            go.AddComponent<LuaBehaviour>();

            if (func != null) func.Call(go);
            Debug.LogWarning("CreatePanel::>> " + name + " " + prefab);
        }
#endif
    }
}