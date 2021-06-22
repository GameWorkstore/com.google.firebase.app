using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Firebase.Editor.ZipExtractor
{
    public class PackageZipExtrator : AssetPostprocessor
    {
        private const string _package = "com.google.firebase.app";

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var path = Path.Combine(Application.dataPath, "../Library/PackageCache/");
            foreach (var dir in Directory.GetDirectories(path))
            {
                if (!dir.Contains(_package)) continue;
                foreach (var file in Directory.GetFiles(dir, "*.zip", SearchOption.AllDirectories))
                {
                    var original = file.Substring(0,file.Length - 4);
                    if (File.Exists(original)) continue;

                    var sdir = Path.GetDirectoryName(original);
                    UnityEngine.Debug.Log(sdir);
                    var processInfo = new ProcessStartInfo()
                    {
                        FileName = "unzip",
                        Arguments = file + " -d " + sdir,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    };
                    var proc = new Process() { StartInfo = processInfo };
                    proc.Start();
                    //string output = proc.StandardOutput.ReadToEnd();
                    string error = proc.StandardError.ReadToEnd();
                    proc.WaitForExit();
                    if (!string.IsNullOrEmpty(error))
                    {
                        UnityEngine.Debug.LogError("PackageZipExtrator:" + error);
                        break;
                    }

                    if (File.Exists(original)) File.Delete(file);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                    //AssetDatabase.ImportAsset(AbsoluteToRelative(original));
                }
                break;
            }
        }

        /// <summary>
        /// Converts absolute path (System.File) to relative path (UnityEditor.AssetDatabase)
        /// </summary>
        /// <param name="absolutePath">Absolute path</param>
        /// <returns>Relative path or string.Empty if absolutePath is invalid.</returns>
        public static string AbsoluteToRelative(string absolutePath)
        {
            var fileUri = new System.Uri(absolutePath);
            var referenceUri = new System.Uri(Application.dataPath);
            return System.Uri.UnescapeDataString(referenceUri.MakeRelativeUri(fileUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
        }
    }
}
