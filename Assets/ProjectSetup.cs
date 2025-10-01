using System.IO;
using UnityEditor;
using UnityEngine;
using static System.IO.Path;
using static UnityEditor.AssetDatabase;

public static class ProjectSetup
{
    [MenuItem("Tools/Setup/Import Essantial Assets")]
    static void ImportEssantialAssets()
    {
        Assets.ImportAsset("vTabs 2.unitypackage", "kubacho lab/Editor ExtensionsUtilities");
        Assets.ImportAsset("vInspector 2.unitypackage", "kubacho lab/Editor ExtensionsUtilities");
        Assets.ImportAsset("vFavorites 2.unitypackage", "kubacho lab/Editor ExtensionsUtilities");
        Assets.ImportAsset("vHierarchy 2.unitypackage", "kubacho lab/Editor ExtensionsUtilities");
        Assets.ImportAsset("vFolders 2.unitypackage", "kubacho lab/Editor ExtensionsUtilities");
        Assets.ImportAsset("Editor Console Pro.unitypackage", "FlyingWorm/Editor ExtensionsSystem\"");
    }
    [MenuItem("Tools/Setup/Create Folders")]
    public static void CreateFolders() {
        Folders.Create("_Project", "Animation", "Art", "Materials", "Prefabs", "Scripts", "Sounds");
        Refresh();
        Folders.Move("_Project", "Scenes");
        Folders.Move("_Project", "Settings");
        Folders.Delete("TutorialInfo");
        Refresh();

        MoveAsset("Assets/InputSystem_Actions.inputactions", "Assets/_Project/Settings/InputSystem_Actions.inputactions");
        DeleteAsset("Assets/Readme.asset");
        Refresh();
        
        // EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
    }
    
    static class Assets
    {
        public static void ImportAsset(string asset, string folder)
        {
            string basePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string assetFolder = Combine(basePath, "Unity/Asset Store-5.x");
            ImportPackage(Combine(assetFolder, folder, asset), false);
        }
    }
    
    static class Folders {
        public static void Create(string root, params string[] folders) {
            var fullpath = Combine(Application.dataPath, root);
            if (!Directory.Exists(fullpath)) {
                Directory.CreateDirectory(fullpath);
            }

            foreach (var folder in folders) {
                CreateSubFolders(fullpath, folder);
            }
        }
        
        static void CreateSubFolders(string rootPath, string folderHierarchy) {
            var folders = folderHierarchy.Split('/');
            var currentPath = rootPath;

            foreach (var folder in folders) {
                currentPath = Combine(currentPath, folder);
                if (!Directory.Exists(currentPath)) {
                    Directory.CreateDirectory(currentPath);
                }
            }
        }
        
        public static void Move(string newParent, string folderName) {
            var sourcePath = $"Assets/{folderName}";
            if (IsValidFolder(sourcePath)) {
                var destinationPath = $"Assets/{newParent}/{folderName}";
                var error = MoveAsset(sourcePath, destinationPath);

                if (!string.IsNullOrEmpty(error)) {
                    Debug.LogError($"Failed to move {folderName}: {error}");
                }
            }
        }
        
        public static void Delete(string folderName) {
            var pathToDelete = $"Assets/{folderName}";

            if (IsValidFolder(pathToDelete)) {
                DeleteAsset(pathToDelete);
            }
        }
    }
}
