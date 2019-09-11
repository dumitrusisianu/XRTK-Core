﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XRTK.Definitions;
using XRTK.Extensions.EditorClassExtensions;
using XRTK.Inspectors.Utilities.Packages;
using XRTK.Inspectors.Utilities.SymbolicLinks;
using XRTK.Utilities.Editor;

namespace XRTK.Inspectors
{
    public static class MixedRealityPreferences
    {
        private static readonly string[] XRTK_Keywords = { "XRTK", "Mixed", "Reality" };

        #region Lock Profile Preferences

        private static readonly GUIContent LockContent = new GUIContent("Lock SDK profiles", "Locks the SDK profiles from being edited.\n\nThis setting only applies to the currently running project.");
        private const string LOCK_KEY = "LockProfiles";
        private static bool lockPrefLoaded;
        private static bool lockProfiles;

        /// <summary>
        /// Should the default profile inspectors be disabled to prevent editing?
        /// </summary>
        public static bool LockProfiles
        {
            get
            {
                if (!lockPrefLoaded)
                {
                    lockProfiles = EditorPreferences.Get(LOCK_KEY, true);
                    lockPrefLoaded = true;
                }

                return lockProfiles;
            }
            set => EditorPreferences.Set(LOCK_KEY, lockProfiles = value);
        }

        #endregion Lock Profile Preferences

        #region Ignore startup settings prompt

        private static readonly GUIContent IgnoreContent = new GUIContent("Ignore settings prompt on startup", "Prevents settings dialog popup from showing on startup.\n\nThis setting applies to all projects using XRTK.");
        private const string IGNORE_KEY = "_MixedRealityToolkit_Editor_IgnoreSettingsPrompts";
        private static bool ignorePrefLoaded;
        private static bool ignoreSettingsPrompt;

        /// <summary>
        /// Should the settings prompt show on startup?
        /// </summary>
        public static bool IgnoreSettingsPrompt
        {
            get
            {
                if (!ignorePrefLoaded)
                {
                    ignoreSettingsPrompt = EditorPrefs.GetBool(IGNORE_KEY, false);
                    ignorePrefLoaded = true;
                }

                return ignoreSettingsPrompt;
            }
            set => EditorPrefs.SetBool(IGNORE_KEY, ignoreSettingsPrompt = value);
        }

        #endregion Ignore startup settings prompt

        #region Show Canvas Utility Prompt

        private static readonly GUIContent CanvasUtilityContent = new GUIContent("Canvas world space utility dialogs", "Enable or disable the dialog popups for the world space canvas settings.\n\nThis setting only applies to the currently running project.");
        private const string CANVAS_KEY = "EnableCanvasUtilityDialog";
        private static bool isCanvasUtilityPrefLoaded;
        private static bool showCanvasUtilityPrompt;

        /// <summary>
        /// Should the <see cref="Canvas"/> utility dialog show when updating the <see cref="RenderMode"/> settings on that component?
        /// </summary>
        public static bool ShowCanvasUtilityPrompt
        {
            get
            {
                if (!isCanvasUtilityPrefLoaded)
                {
                    showCanvasUtilityPrompt = EditorPreferences.Get(CANVAS_KEY, true);
                    isCanvasUtilityPrefLoaded = true;
                }

                return showCanvasUtilityPrompt;
            }
            set => EditorPreferences.Set(CANVAS_KEY, showCanvasUtilityPrompt = value);
        }

        #endregion Show Canvas Utility Prompt

        #region Start Scene Preference

        private static readonly GUIContent StartSceneContent = new GUIContent("Start Scene", "When pressing play in the editor, a prompt will ask you if you want to switch to this start scene.\n\nThis setting only applies to the currently running project.");
        private const string START_SCENE_KEY = "StartScene";
        private static SceneAsset sceneAsset;
        private static bool isStartScenePrefLoaded;

        public static SceneAsset StartSceneAsset
        {
            get
            {
                if (!isStartScenePrefLoaded)
                {
                    var scenePath = EditorPreferences.Get(START_SCENE_KEY, string.Empty);
                    sceneAsset = GetSceneObject(scenePath);
                    isStartScenePrefLoaded = true;
                }

                return sceneAsset;
            }
            set
            {
                sceneAsset = value != null ? GetSceneObject(value) : null;
                var scenePath = value != null ? AssetDatabase.GetAssetOrScenePath(value) : string.Empty;
                EditorPreferences.Set(START_SCENE_KEY, scenePath);
            }
        }

        #endregion  Start Scene Preference

        #region Symbolic Link Preferences

        private static bool isSymbolicLinkSettingsPathLoaded;
        private static string symbolicLinkSettingsPath = string.Empty;

        /// <summary>
        /// The path to the symbolic link settings found for this project.
        /// </summary>
        public static string SymbolicLinkSettingsPath
        {
            get
            {
                if (!isSymbolicLinkSettingsPathLoaded)
                {
                    symbolicLinkSettingsPath = EditorPreferences.Get("_SymbolicLinkSettingsPath", string.Empty);
                    isSymbolicLinkSettingsPathLoaded = true;
                }

                if (!EditorApplication.isUpdating &&
                    string.IsNullOrEmpty(symbolicLinkSettingsPath))
                {
                    symbolicLinkSettingsPath = AssetDatabase
                        .FindAssets($"t:{typeof(SymbolicLinkSettings).Name}")
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .OrderBy(x => x)
                        .FirstOrDefault();
                }

                return symbolicLinkSettingsPath;
            }
            set => EditorPreferences.Set("_SymbolicLinkSettingsPath", symbolicLinkSettingsPath = value);
        }

        private static bool isAutoLoadSymbolicLinksLoaded;
        private static bool autoLoadSymbolicLinks = true;

        public static bool AutoLoadSymbolicLinks
        {
            get
            {
                if (!isAutoLoadSymbolicLinksLoaded)
                {
                    autoLoadSymbolicLinks = EditorPreferences.Get("_AutoLoadSymbolicLinks", true);
                    isAutoLoadSymbolicLinksLoaded = true;
                }

                return autoLoadSymbolicLinks;
            }
            set
            {
                EditorPreferences.Set("_AutoLoadSymbolicLinks", autoLoadSymbolicLinks = value);

//                if (autoLoadSymbolicLinks && SymbolicLinker.Settings == null)
//                {
//                    var profile = ScriptableObject.CreateInstance(nameof(SymbolicLinkSettings));
//                    profile.CreateAsset("Assets/XRTK.Generated/CustomProfiles");
//                }
            }
        }

        #endregion Symbolic Link Preferences

        #region Package Preferences

        private static bool isPackageSettingsPathLoaded;
        private static string packageSettingsPath = string.Empty;

        /// <summary>
        /// The path to the package settings found for this project.
        /// </summary>
        public static string PackageSettingsPath
        {
            get
            {
                if (!isPackageSettingsPathLoaded)
                {
                    symbolicLinkSettingsPath = EditorPreferences.Get("_PackageSettingsPath", string.Empty);
                    isPackageSettingsPathLoaded = true;
                }

                if (!EditorApplication.isUpdating &&
                     string.IsNullOrEmpty(packageSettingsPath))
                {
                    packageSettingsPath = AssetDatabase
                        .FindAssets($"t:{typeof(MixedRealityPackageSettings).Name}")
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .OrderBy(x => x)
                        .FirstOrDefault();
                }

                return packageSettingsPath;
            }
            set => EditorPreferences.Set("_PackageSettingsPath", packageSettingsPath = value);
        }

        #endregion Package Prefernces

        #region Debug Packages

        private static readonly GUIContent DebugUpmContent = new GUIContent("Debug package loading", "Enable or disable the debug information for package loading.\n\nThis setting only applies to the currently running project.");
        private const string PACKAGE_DEBUG_KEY = "EnablePackageDebug";
        private static bool isPackageDebugPrefLoaded;
        private static bool debugPackageInfo;

        /// <summary>
        /// Enabled debugging info for the xrtk upm packages.
        /// </summary>
        public static bool DebugPackageInfo
        {
            get
            {
                if (!isPackageDebugPrefLoaded)
                {
                    debugPackageInfo = EditorPreferences.Get(PACKAGE_DEBUG_KEY, Application.isBatchMode);
                    isPackageDebugPrefLoaded = true;
                }

                return debugPackageInfo;
            }
            set => EditorPreferences.Set(PACKAGE_DEBUG_KEY, debugPackageInfo = value);
        }

        #endregion Debug Packages

        [SettingsProvider]
        private static SettingsProvider Preferences()
        {
            return new SettingsProvider("Preferences/XRTK", SettingsScope.User, XRTK_Keywords)
            {
                label = "XRTK",
                guiHandler = OnPreferencesGui,
                keywords = new HashSet<string>(XRTK_Keywords)
            };
        }

        private static void OnPreferencesGui(string searchContext)
        {
            var prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200f;

            EditorGUI.BeginChangeCheck();
            lockProfiles = EditorGUILayout.Toggle(LockContent, LockProfiles);

            // Save the preference
            if (EditorGUI.EndChangeCheck())
            {
                LockProfiles = lockProfiles;
            }

            if (!LockProfiles)
            {
                EditorGUILayout.HelpBox("This is only to be used to update the default SDK profiles. If any edits are made, and not checked into the XRTK's Github, the changes may be lost next time you update your local copy.", MessageType.Warning);
            }

            EditorGUI.BeginChangeCheck();
            ignoreSettingsPrompt = EditorGUILayout.Toggle(IgnoreContent, IgnoreSettingsPrompt);

            // Save the preference
            if (EditorGUI.EndChangeCheck())
            {
                IgnoreSettingsPrompt = ignoreSettingsPrompt;
            }

            EditorGUI.BeginChangeCheck();
            showCanvasUtilityPrompt = EditorGUILayout.Toggle(CanvasUtilityContent, ShowCanvasUtilityPrompt);

            if (EditorGUI.EndChangeCheck())
            {
                ShowCanvasUtilityPrompt = showCanvasUtilityPrompt;
            }

            if (!ShowCanvasUtilityPrompt)
            {
                EditorGUILayout.HelpBox("Be aware that if a Canvas needs to receive input events it is required to have the CanvasUtility attached or the Focus Provider's UIRaycast Camera assigned to the canvas' camera reference.", MessageType.Warning);
            }

            EditorGUI.BeginChangeCheck();
            var startScene = (SceneAsset)EditorGUILayout.ObjectField(StartSceneContent, StartSceneAsset, typeof(SceneAsset), true);

            if (EditorGUI.EndChangeCheck())
            {
                StartSceneAsset = startScene;
            }

            EditorGUI.BeginChangeCheck();
            var scriptLock = EditorGUILayout.Toggle("Is Script Reloading locked?", EditorAssemblyReloadManager.LockReloadAssemblies);

            if (EditorGUI.EndChangeCheck())
            {
                EditorAssemblyReloadManager.LockReloadAssemblies = scriptLock;
            }

            EditorGUI.BeginChangeCheck();
            autoLoadSymbolicLinks = EditorGUILayout.Toggle("Auto Load Symbolic Links", AutoLoadSymbolicLinks);

//            if (EditorGUI.EndChangeCheck())
//            {
//                AutoLoadSymbolicLinks = autoLoadSymbolicLinks;
//
//                if (AutoLoadSymbolicLinks)
//                {
//                    EditorApplication.delayCall += () => SymbolicLinker.RunSync();
//                }
//            }

//            EditorGUI.BeginChangeCheck();
//            var symbolicLinkSettings = EditorGUILayout.ObjectField("Symbolic Link Settings", SymbolicLinker.Settings, typeof(SymbolicLinkSettings), false) as SymbolicLinkSettings;
//
//            if (EditorGUI.EndChangeCheck())
//            {
//                if (symbolicLinkSettings != null)
//                {
//                    var shouldSync = string.IsNullOrEmpty(SymbolicLinkSettingsPath);
//                    SymbolicLinkSettingsPath = AssetDatabase.GetAssetPath(symbolicLinkSettings);
//                    SymbolicLinker.Settings = AssetDatabase.LoadAssetAtPath<SymbolicLinkSettings>(SymbolicLinkSettingsPath);
//
//                    if (shouldSync)
//                    {
//                        EditorApplication.delayCall += () => SymbolicLinker.RunSync();
//                    }
//                }
//                else
//                {
//                    SymbolicLinkSettingsPath = string.Empty;
//                    SymbolicLinker.Settings = null;
//                }
//            }

            EditorGUI.BeginChangeCheck();
            debugPackageInfo = EditorGUILayout.Toggle(DebugUpmContent, DebugPackageInfo);

            if (EditorGUI.EndChangeCheck())
            {
                DebugPackageInfo = debugPackageInfo;
            }

//            EditorGUI.BeginChangeCheck();
//            var packageSettings = EditorGUILayout.ObjectField("Package Settings", MixedRealityPackageUtilities.PackageSettings, typeof(MixedRealityPackageSettings), false) as MixedRealityPackageSettings;
//
//            if (EditorGUI.EndChangeCheck())
//            {
//                if (packageSettings != null)
//                {
//                    var shouldSync = string.IsNullOrEmpty(PackageSettingsPath);
//                    PackageSettingsPath = AssetDatabase.GetAssetPath(packageSettings);
//                    MixedRealityPackageUtilities.PackageSettings = AssetDatabase.LoadAssetAtPath<MixedRealityPackageSettings>(PackageSettingsPath);
//
//                    if (shouldSync)
//                    {
//                        EditorApplication.delayCall += MixedRealityPackageUtilities.CheckPackageManifest;
//                    }
//                }
//                else
//                {
//                    PackageSettingsPath = string.Empty;
//                    MixedRealityPackageUtilities.PackageSettings = null;
//                }
//            }

            EditorGUIUtility.labelWidth = prevLabelWidth;
        }

        private static SceneAsset GetSceneObject(SceneAsset asset)
        {
            return GetSceneObject(asset.name, asset);
        }

        private static SceneAsset GetSceneObject(string sceneName, SceneAsset asset = null)
        {
            if (string.IsNullOrEmpty(sceneName) ||
                EditorBuildSettings.scenes == null)
            {
                return null;
            }

            EditorBuildSettingsScene editorScene = null;

            if (EditorBuildSettings.scenes.Length < 1)
            {
                if (asset == null)
                {
                    Debug.Log($"{sceneName} scene not found in build settings!");
                    return null;
                }

                editorScene = new EditorBuildSettingsScene
                {
                    path = AssetDatabase.GetAssetOrScenePath(asset),
                };

                editorScene.guid = new GUID(AssetDatabase.AssetPathToGUID(editorScene.path));

                EditorBuildSettings.scenes = new[] { editorScene };
            }
            else
            {

                try
                {
                    editorScene = EditorBuildSettings.scenes.First(scene => scene.path.IndexOf(sceneName, StringComparison.Ordinal) != -1);
                }
                catch
                {
                    // ignored
                }
            }

            if (editorScene != null)
            {
                asset = AssetDatabase.LoadAssetAtPath(editorScene.path, typeof(SceneAsset)) as SceneAsset;
            }

            if (asset == null)
            {
                return null;
            }

            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out var guid, out long _);
            var sceneGuid = new GUID(guid);

            if (EditorBuildSettings.scenes[0].guid != sceneGuid)
            {
                editorScene = new EditorBuildSettingsScene(sceneGuid, true);
                var scenes = EditorBuildSettings.scenes
                    .Where(scene => scene.guid != sceneGuid)
                    .Prepend(editorScene)
                    .ToArray();
                EditorBuildSettings.scenes = scenes;
                Debug.Assert(EditorBuildSettings.scenes[0].guid == sceneGuid);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (!EditorBuildSettings.scenes[0].enabled)
            {
                EditorBuildSettings.scenes[0].enabled = true;
            }

            return asset;
        }
    }
}
