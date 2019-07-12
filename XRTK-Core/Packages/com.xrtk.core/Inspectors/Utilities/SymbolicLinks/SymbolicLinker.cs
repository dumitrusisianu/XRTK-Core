﻿//// Copyright (c) XRTK. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.
//
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using UnityEditor;
//using UnityEditor.Callbacks;
//using UnityEngine;
//using XRTK.Extensions;
//using XRTK.Inspectors.Utilities.Packages;
//using Debug = UnityEngine.Debug;
//
//namespace XRTK.Inspectors.Utilities.SymbolicLinks
//{
//    [InitializeOnLoad]
//    internal static class SymbolicLinker
//    {
//        /// <summary>
//        /// Constructor.
//        /// </summary>
//        static SymbolicLinker()
//        {
//            RunSync(Application.isBatchMode);
//            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGui;
//        }
//
//        private const string LINK_ICON_TEXT = "<=link=>";
//        private const string REGEX_BRACKETS = @"\{(.*?)\}";
//        private const FileAttributes FOLDER_SYMLINK_ATTRIBUTES = FileAttributes.ReparsePoint;
//
//        private static readonly Regex BracketRegex = new Regex(REGEX_BRACKETS);
//
//        // Style used to draw the symlink indicator in the project view.
//        private static GUIStyle SymlinkMarkerStyle => symbolicLinkMarkerStyle ?? (symbolicLinkMarkerStyle = new GUIStyle(EditorStyles.label)
//        {
//            normal = { textColor = new Color(.2f, .8f, .2f, .8f) },
//            alignment = TextAnchor.MiddleRight
//        });
//
//        private static GUIStyle symbolicLinkMarkerStyle;
//
//        internal static string ProjectRoot => GitUtilities.RepositoryRootDir;
//
//        private static bool isRunningSync;
//
//        /// <summary>
//        /// Is the sync task running?
//        /// </summary>
////        public static bool IsSyncing => isRunningSync || MixedRealityPackageUtilities.IsRunningCheck;
//
//        /// <summary>
//        /// Debug the symbolic linker utility.
//        /// </summary>
//        public static bool DebugEnabled
//        {
//            get => MixedRealityPreferences.DebugPackageInfo;
//            set => MixedRealityPreferences.DebugPackageInfo = value;
//        }
//
//        /// <summary>
//        /// The current settings for the symbolic links.
//        /// </summary>
//        public static SymbolicLinkSettings Settings
//        {
//            get
//            {
//                if (settings == null &&
//                    !string.IsNullOrEmpty(MixedRealityPreferences.SymbolicLinkSettingsPath))
//                {
//                    settings = AssetDatabase.LoadAssetAtPath<SymbolicLinkSettings>(MixedRealityPreferences.SymbolicLinkSettingsPath);
//                }
//
//                return settings;
//            }
//            internal set => settings = value;
//        }
//
//        private static SymbolicLinkSettings settings;
//
//        [DidReloadScripts]
//        private static void OnScriptsReloaded() => RunSync();
//
//        /// <summary>
//        /// Synchronizes the project with any symbolic links defined in the settings.
//        /// </summary>
//        /// <param name="forceUpdate">Bypass the auto load check and force the packages to be updated, even if they're up to date.</param>
//        public static async void RunSync(bool forceUpdate = false)
//        {
//            if (IsSyncing || EditorApplication.isPlayingOrWillChangePlaymode)
//            {
//                return;
//            }
//
//            if (!MixedRealityPreferences.AutoLoadSymbolicLinks && !forceUpdate)
//            {
//                MixedRealityPackageUtilities.CheckPackageManifest();
//                return;
//            }
//
//            if (Settings == null)
//            {
//                if (EditorApplication.isUpdating)
//                {
//                    EditorApplication.delayCall += () => RunSync(forceUpdate);
//                    return;
//                }
//            }
//
//            if (Settings == null)
//            {
//                if (!string.IsNullOrEmpty(MixedRealityPreferences.SymbolicLinkSettingsPath))
//                {
//                    Debug.LogWarning("Symbolic link settings not found! Auto load links has been turned off.\nYou can enable this again in the xrtk preferences.");
//                }
//
//                MixedRealityPreferences.AutoLoadSymbolicLinks = false;
//                MixedRealityPackageUtilities.CheckPackageManifest();
//                return;
//            }
//
//            isRunningSync = true;
//            EditorApplication.LockReloadAssemblies();
//
//            if (DebugEnabled)
//            {
//                Debug.Log("Verifying project's symbolic links...");
//            }
//
//            var symbolicLinks = new List<SymbolicLink>(Settings.SymbolicLinks);
//
//            foreach (var link in symbolicLinks)
//            {
//                if (string.IsNullOrEmpty(link.SourceRelativePath)) { continue; }
//
//                bool isValid = false;
//                var targetAbsolutePath = $"{ProjectRoot}{link.TargetRelativePath}";
//                var sourceAbsolutePath = $"{ProjectRoot}{link.SourceRelativePath}";
//
//                if (link.IsActive)
//                {
//                    isValid = await VerifySymbolicLink(targetAbsolutePath, sourceAbsolutePath);
//                }
//
//                if (isValid)
//                {
//                    // If we already have the directory in our project, then skip.
//                    if (link.IsActive) { continue; }
//
//                    // Check to see if there are any directories that don't belong and remove them.
//                    DisableLink(link.TargetRelativePath);
//                    continue;
//                }
//
//                if (!link.IsActive) { continue; }
//
//                if (Directory.Exists(sourceAbsolutePath))
//                {
//                    if (link.IsActive)
//                    {
//                        AddLink(sourceAbsolutePath, targetAbsolutePath);
//                    }
//
//                    continue;
//                }
//
//                // Make sure all of our Submodules are initialized and updated.
//                // if they didn't get updated then we probably have some pending changes so we will skip.
//                if (!GitUtilities.UpdateSubmodules()) { continue; }
//
//                if (Directory.Exists(sourceAbsolutePath))
//                {
//                    if (link.IsActive)
//                    {
//                        AddLink(sourceAbsolutePath, targetAbsolutePath);
//                    }
//
//                    continue;
//                }
//
//                Debug.LogError($"Unable to find symbolic link source path: {sourceAbsolutePath}");
//                RemoveLink(link.SourceRelativePath, link.TargetRelativePath);
//            }
//
//            if (DebugEnabled)
//            {
//                Debug.Log("Project symbolic links verified.");
//            }
//
//            EditorUtility.SetDirty(Settings);
//            AssetDatabase.SaveAssets();
//            AssetDatabase.Refresh();
//
//            EditorApplication.UnlockReloadAssemblies();
//            MixedRealityPackageUtilities.CheckPackageManifest();
//            isRunningSync = false;
//        }
//
//        /// <summary>
//        /// Adds or Enables a symbolic link in the settings.
//        /// </summary>
//        /// <param name="sourceAbsolutePath"></param>
//        /// <param name="targetAbsolutePath"></param>
//        public static void AddLink(string sourceAbsolutePath, string targetAbsolutePath)
//        {
//            if (string.IsNullOrEmpty(sourceAbsolutePath) || string.IsNullOrEmpty(targetAbsolutePath))
//            {
//                Debug.LogError("Unable to write to the symbolic link settings with null or empty parameters.");
//                return;
//            }
//
//            targetAbsolutePath = AddSubfolderPathToTarget(sourceAbsolutePath, targetAbsolutePath);
//
//            // Fix the directory character separator characters.
//            var sourceRelativePath = sourceAbsolutePath.ToBackSlashes();
//            var targetRelativePath = targetAbsolutePath.ToBackSlashes();
//
//            // Strip URI for relative path.
//            sourceRelativePath = sourceRelativePath.Replace(ProjectRoot, string.Empty);
//            targetRelativePath = targetRelativePath.Replace(ProjectRoot, string.Empty);
//
//            if (!CreateSymbolicLink(sourceAbsolutePath, targetAbsolutePath))
//            {
//                return;
//            }
//
//            SymbolicLink symbolicLink;
//
//            // Check if the symbolic link is already registered.
//            if (!Settings.SymbolicLinks.Exists(link => link.TargetRelativePath == targetRelativePath))
//            {
//                symbolicLink = new SymbolicLink
//                {
//                    SourceRelativePath = sourceRelativePath,
//                    TargetRelativePath = targetRelativePath,
//                    IsActive = true
//                };
//
//                Settings.SymbolicLinks.Add(symbolicLink);
//            }
//            else // Overwrite the registered symbolic link
//            {
//                symbolicLink = Settings.SymbolicLinks.Find(link => link.TargetRelativePath == targetRelativePath);
//
//                if (symbolicLink != null)
//                {
//                    symbolicLink.IsActive = true;
//                }
//            }
//        }
//
//        /// <summary>
//        /// Disables a symbolic link in the settings.
//        /// </summary>
//        /// <param name="targetRelativePath"></param>
//        public static void DisableLink(string targetRelativePath)
//        {
//            var symbolicLink = Settings.SymbolicLinks.Find(link => link.TargetRelativePath == targetRelativePath);
//
//            if (symbolicLink != null)
//            {
//                if (DeleteSymbolicLink($"{ProjectRoot}{targetRelativePath}"))
//                {
//                    Debug.Log($"Disabled symbolic link to \"{symbolicLink.SourceRelativePath}\" in project.");
//                    symbolicLink.IsActive = false;
//                }
//            }
//            else
//            {
//                Debug.LogError($"Unable to find the specified symbolic link at: {targetRelativePath}");
//            }
//        }
//
//        /// <summary>
//        /// Remove a symbolic link in the settings.
//        /// </summary>
//        /// <param name="sourceRelativePath"></param>
//        /// <param name="targetRelativePath"></param>
//        public static void RemoveLink(string sourceRelativePath, string targetRelativePath)
//        {
//            DisableLink(targetRelativePath);
//
//            var symbolicLink = Settings.SymbolicLinks.Find(link => link.SourceRelativePath == sourceRelativePath);
//
//            if (symbolicLink != null)
//            {
//                Debug.Log($"Removed symbolic link to \"{symbolicLink.SourceRelativePath}\" from project.");
//                Settings.SymbolicLinks.Remove(symbolicLink);
//            }
//            else
//            {
//                Debug.LogError($"Unable to find the specified symbolic link source at: {sourceRelativePath}");
//            }
//        }
//
//        private static void OnProjectWindowItemGui(string guid, Rect rect)
//        {
//            try
//            {
//                var path = AssetDatabase.GUIDToAssetPath(guid);
//
//                if (string.IsNullOrEmpty(path)) { return; }
//
//                var attributes = File.GetAttributes(path);
//
//                if ((attributes & FOLDER_SYMLINK_ATTRIBUTES) != FOLDER_SYMLINK_ATTRIBUTES) { return; }
//
//                GUI.Label(rect, LINK_ICON_TEXT, SymlinkMarkerStyle);
//
//                if (Settings.SymbolicLinks.Any(link => link.TargetRelativePath.Contains(path))) { return; }
//
//                var fullPath = Path.GetFullPath(path);
//
//                if (DeleteSymbolicLink(fullPath))
//                {
//                    Debug.Log($"Removed \"{fullPath}\" symbolic link from project.");
//                }
//            }
//            catch
//            {
//                // ignored
//            }
//        }
//
//        private static bool CreateSymbolicLink(string sourceAbsolutePath, string targetAbsolutePath)
//        {
//            if (string.IsNullOrEmpty(targetAbsolutePath) || string.IsNullOrEmpty(sourceAbsolutePath))
//            {
//                Debug.LogError("Unable to create symbolic link with null or empty args");
//                return false;
//            }
//
//            if (!sourceAbsolutePath.Contains(ProjectRoot))
//            {
//                Debug.LogError($"The symbolic link you're importing needs to be in your project's git repository.\n{sourceAbsolutePath}\n{ProjectRoot}");
//                return false;
//            }
//
//            targetAbsolutePath = AddSubfolderPathToTarget(sourceAbsolutePath, targetAbsolutePath);
//            var ignorePath = targetAbsolutePath;
//
//            if (ignorePath.Contains("Assets/ThirdParty"))
//            {
//                // Check if we need to create the ThirdParty folder.
//                if (!Directory.Exists($"{Application.dataPath}/ThirdParty"))
//                {
//                    Directory.CreateDirectory($"{Application.dataPath}/ThirdParty");
//                }
//
//                // Initialize gitIgnore if needed.
//                GitUtilities.WritePathToGitIgnore("Assets/ThirdParty");
//                GitUtilities.WritePathToGitIgnore("Assets/ThirdParty.meta");
//            }
//            else // If we're not importing into the ThirdParty Directory, we should add the path to the git ignore file.
//            {
//                GitUtilities.WritePathToGitIgnore($"{ignorePath}");
//
//                // If the imported path target resides in the Assets folder, we should also ignore the path .meta file.
//                if (ignorePath.Contains("Assets"))
//                {
//                    GitUtilities.WritePathToGitIgnore($"{ignorePath}.meta");
//                }
//            }
//
//            // Check if the directory exists to ensure the parent directory paths are also setup
//            // then delete the target directory as the mklink command will create it for us.
//            if (!Directory.Exists(targetAbsolutePath))
//            {
//                Directory.CreateDirectory(targetAbsolutePath);
//                Directory.Delete(targetAbsolutePath);
//            }
//
//            // --------------------> /C mklink /D "C:\Link To Folder" "C:\Users\Name\Original Folder"
//            if (!new Process().Run($"/C mklink /D \"{targetAbsolutePath}\" \"{sourceAbsolutePath}\"", out var error))
//            {
//                Debug.LogError($"{error}");
//                return false;
//            }
//
//            Debug.Log($"Successfully created symbolic link to {sourceAbsolutePath}");
//            AssetDatabase.Refresh();
//            return true;
//        }
//
//        private static bool DeleteSymbolicLink(string path)
//        {
//            if (!Directory.Exists(path))
//            {
//                Debug.LogError($"Unable to find the linked folder at: {path}");
//                return false;
//            }
//
//            if (new Process().Run($"/C rmdir /q \"{path}\"", out _))
//            {
//                if (File.Exists($"{path}.meta"))
//                {
//                    File.Delete($"{path}.meta");
//                }
//
//                AssetDatabase.Refresh();
//            }
//
//            return true;
//        }
//
//        private static async Task<bool> VerifySymbolicLink(string targetAbsolutePath, string sourceAbsolutePath)
//        {
//            var pathToVerify = targetAbsolutePath.Substring(0, targetAbsolutePath.LastIndexOf("/", StringComparison.Ordinal));
//
//            if (DebugEnabled)
//            {
//                Debug.Log($"Attempting to validate {targetAbsolutePath}");
//            }
//
//            var args = $"/C powershell.exe \"& dir '{pathToVerify}' | select Target, LinkType | where {{ $_.LinkType -eq 'SymbolicLink' }} | Format-Table -HideTableHeaders -Wrap\"";
//
//            bool success;
//            string[] processOutput;
//
//            if (Application.isBatchMode)
//            {
//                success = new Process().Run(args, out var output);
//                processOutput = new[] { output };
//            }
//            else
//            {
//                var result = await new Process().RunAsync(args);
//                success = result.ExitCode == 0;
//                processOutput = result.Output;
//            }
//
//            if (!success)
//            {
//                Debug.LogError($"Failed to enumerate symbolic links associated to {targetAbsolutePath}");
//                return false;
//            }
//
//            var isValid = false;
//            var matches = BracketRegex.Matches(string.Join("\n", processOutput));
//
//            foreach (Match match in matches)
//            {
//                var targetPath = match.Value.Replace("{", string.Empty).Replace("}", string.Empty);
//
//                if (!string.IsNullOrEmpty(targetPath) && sourceAbsolutePath.ToForwardSlashes().Equals(targetPath))
//                {
//                    isValid = true;
//                }
//            }
//
//            if (!isValid &&
//                Directory.Exists(targetAbsolutePath))
//            {
//                if (DebugEnabled)
//                {
//                    Debug.Log($"Removing invalid link for {targetAbsolutePath}");
//                }
//
//                DeleteSymbolicLink(targetAbsolutePath);
//            }
//
//            return isValid && Directory.Exists(targetAbsolutePath);
//        }
//
//        private static string AddSubfolderPathToTarget(string sourcePath, string targetPath)
//        {
//            var subFolder = sourcePath.Substring(sourcePath.LastIndexOf("/", StringComparison.Ordinal) + 1);
//
//            // Check to see if our target path already has the sub folder reference.
//            if (!targetPath.Substring(targetPath.LastIndexOf("/", StringComparison.Ordinal) + 1).Equals(subFolder))
//            {
//                targetPath = $"{targetPath}/{subFolder}";
//            }
//
//            return targetPath;
//        }
//    }
//}
