using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace MORPH3D {
    [InitializeOnLoad]
    public class MCSUpgrade : Editor
    {
        public static string pathToPackages = "Assets/MORPH3D/Content/Packages";
        public static string pathToPackagesCore = "Assets/MORPH3D/Content/Packages/Core.unitypackage";
        public static string pathToPackagesBase = "Assets/MORPH3D/Content/Packages/M3DBaseContent.unitypackage";
        protected static string pathToConfig = "Assets/MORPH3D/MCSUpgrade.json";

        [Serializable]
        public struct UpgradeMeta
        {
            [SerializeField]
            public bool hideOneDotSixPopup;
            [SerializeField]
            public bool autoInstallPackages;
        }

        public static UpgradeMeta GetMeta()
        {
            UpgradeMeta? meta = null;
            if (File.Exists(pathToConfig))
            {
                try
                {
                    string raw = File.ReadAllText(pathToConfig);
                    meta = JsonUtility.FromJson<UpgradeMeta>(raw);
                } catch 
                {

                }
            }

            if(meta == null)
            {
                meta = new UpgradeMeta();
            }

            return (UpgradeMeta)meta;
        }

        public static void SaveMeta(UpgradeMeta meta)
        {
            try
            {
                string raw = JsonUtility.ToJson(meta);
                File.WriteAllText(pathToConfig, raw);
            } catch (Exception e)
            {
                UnityEngine.Debug.LogError("Unable to save meta configuration for upgrade");
                UnityEngine.Debug.LogException(e);
            }
        }
    
        static MCSUpgrade()
        {
            //Have we shown the popup for 1.0->1.6?
            UpgradeMeta meta = GetMeta();

            if (!meta.hideOneDotSixPopup)
            {
                MCSUpgradeWindow window = MCSUpgradeWindow.Instance;
            }
        }

        public static void AcceptInstall()
        {
            UpgradeMeta meta = GetMeta();
            meta.hideOneDotSixPopup = true;
            meta.autoInstallPackages = true;
            SaveMeta(meta);
            if (File.Exists(pathToPackagesCore))
            {
                AssetDatabase.ImportPackage(pathToPackagesCore, false);
            }
            if (File.Exists(pathToPackagesBase))
            {
                AssetDatabase.ImportPackage(pathToPackagesBase, false);
            }

            if (InstallPackage(pathToPackagesCore) == false)
            {
                UnityEngine.Debug.LogError("Failed to install critical code package: " + pathToPackagesCore);
                return;
            }
            if(InstallPackage(pathToPackagesBase) == false)
            {
                UnityEngine.Debug.LogError("Failed to install critical base package: " + pathToPackagesCore);
                return;
            }

            string[] paths = Directory.GetFiles(pathToPackages, "*.unitypackage", SearchOption.AllDirectories);
            foreach(string path in paths)
            {
                if(path.Contains(pathToPackagesCore) || path.Contains(pathToPackagesBase))
                {
                    continue;
                }

                InstallPackage(path);
            }
        }

        public static bool? InstallPackage(string path)
        {
            if (!path.EndsWith(".unitypackage") || !File.Exists(path) || !path.Contains(pathToPackages))
            {
                return null;
            }

            UnityEngine.Debug.Log("MCSUpgrade installing: " + path);
            try
            {
                AssetDatabase.ImportPackage(path, false);
                AssetDatabase.DeleteAsset(path);
            } catch(Exception e)
            {
                UnityEngine.Debug.LogException(e);
                return false;
            }

            return true;
        }

        [MenuItem("MORPH 3D/Show 1.0 to 1.6 Upgrade Window")]
        public static void MenuItemShowUpgradeWindow()
        {
           MCSUpgradeWindow window = MCSUpgradeWindow.Instance;
        }

    }

    public class MCSUpgradeWindow : EditorWindow
    {

        public void OnGUI()
        {
            GUILayout.Label(
                "Upgrading from 1.0 to 1.6\n"
                + "\n\n"
                + "Please note, if you're upgrading from 1.0 to 1.6\n"
                + "this content and code is *NOT* compatible with 1.0 of MCS!\n"
                + "\n"
                + "Your project may break.\n"
                + "\n"
                + "For more info, visit https://morph3d.com"
                + "\n\n"
            );

            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.padding.top = 10;
            style.padding.bottom = 10;
            style.margin.top = 50;

            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Cancel, do NOT install",style))
            {
                Close();
                return;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Install, I'm ready to upgrade.\nDo not warn me again.",style))
            {
                MCSUpgrade.AcceptInstall();
                Close();
                return;
            }
            EditorGUILayout.EndHorizontal();
        }

        private static MCSUpgradeWindow _instance;
        public static MCSUpgradeWindow Instance
        {
            get {
                if (_instance == null)
                {
                    Rect rect = new Rect(50, 50, 600, 600);
                    _instance = (MCSUpgradeWindow)GetWindowWithRect(typeof(MCSUpgradeWindow),rect, false, "1.0 to 1.6 Upgrade");
                }
                return _instance;
            }
        }
        public static void RepaintWindow()
        {
            if (_instance != null)
                _instance.Repaint();
        }

        void OnEnable()
        {
            _instance = this;
        }
        void OnDisable()
        {
            _instance = null;
        }
    }

    public class MCSUpgradePackageImporter : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool hasPacakgesToInstall = false;
            
            foreach(string path in importedAssets)
            {
                if (path.Contains(MCSUpgrade.pathToPackages))
                {
                    hasPacakgesToInstall = true;
                    break;
                }
            }

            if (hasPacakgesToInstall)
            {
                MCSUpgrade.UpgradeMeta meta = MCSUpgrade.GetMeta();
                if (meta.autoInstallPackages)
                {
                    foreach(string path in importedAssets)
                    {
                        if (!path.Contains(MCSUpgrade.pathToPackages))
                        {
                            continue;
                        }

                        MCSUpgrade.InstallPackage(path);
                    }
                }
            }
        }
    }
}