using System.Collections.Generic;
using System.Linq;
using TBStudio.Tools.Editor.Helpers;
using TBStudio.Tools.Editor.ProjectBuilder.BuildConfig;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TBStudio.Tools.Editor.ProjectBuilder
{
    public class ProjectBuilderEditor : EditorWindow
    {
        private VisualElement container;

        private readonly List<Label> overviewInfoLabels = new();

        #region Referances Window Items
        private ObjectField objBuildConfig;
        private DropdownField selBuildPlatform;
        private Button btnChangeBuildPlatform;
        private Label lblPlatform;
        private Label lblBuildType;
        private Label lblBuildFileName;
        private Label lblNoBuildIcon;
        private Image imgBuildIcon;
        private Button btnBuild;
        private Label lblNotification;
        #endregion

        private BuildConfigSO buildConfig;
        private BuildTarget currentTarget;

        private const string rootPath = "Packages/com.tbstudio.tools/Editor/SharedEditorLayouts/";
        private const string path = "Packages/com.tbstudio.tools/Editor/ProjectBuilder/EditorWindow/";

        [MenuItem("TB Studio/Window/Project Builder")]
        private static void ShowWindow()
        {
            ProjectBuilderEditor window = GetWindow<ProjectBuilderEditor>("Project Builder");
            window.minSize = new Vector2(400, 650);
            window.maxSize = new Vector2(1200, 800);
        }

        private void CreateGUI()
        {
            LoadWindowLayout();
            GetWindowItemsReferances();
            SetObjectFieldType();
            AddButtonEventListeners();
            AddCustomEvents();
            SeedDropdown();
            OnLoadBuildTarget();
            UpdateNotifications("Please load a build config to start.");
        }

        private void LoadWindowLayout()
        {
            container = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path + "ProjectBuilderEditor.uxml");
            visualTree.CloneTree(container);

            var rootStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(rootPath + "root.uss");
            container.styleSheets.Add(rootStyleSheet);

            var pagesStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path + "ProjectBuilderEditor.uss");
            container.styleSheets.Add(pagesStyleSheet);
        }

        private void GetWindowItemsReferances()
        {
            objBuildConfig = container.Q<ObjectField>("select-build-config-object-field");
            selBuildPlatform = container.Q<DropdownField>("change-build-dropdown");
            btnChangeBuildPlatform = container.Q<Button>("change-build-button");
            lblPlatform = container.Q<Label>("build-platform-value");
            lblBuildType = container.Q<Label>("build-type-value");
            lblBuildFileName = container.Q<Label>("build-file-name-value");
            lblNoBuildIcon = container.Q<Label>("build-icon-value");
            imgBuildIcon = container.Q<Image>("build-icon-image");
            btnBuild = container.Q<Button>("build-button");
            lblNotification = container.Q<Label>("notification-label");

            overviewInfoLabels.Add(lblBuildType);
            overviewInfoLabels.Add(lblBuildFileName);
            overviewInfoLabels.Add(lblNoBuildIcon);
        }

        private void SetObjectFieldType()
        {
            objBuildConfig.objectType = typeof(BuildConfigSO);
        }

        private void AddButtonEventListeners()
        {
            btnBuild.clicked += OnBuild;
            btnChangeBuildPlatform.clicked += UpdateBuildPlatform;
        }

        private void AddCustomEvents()
        {
            objBuildConfig.RegisterValueChangedCallback(OnBuildConfigChanged);
            selBuildPlatform.RegisterValueChangedCallback(OnBuildPlatfomChanged);

            BuildTargetChangeNotifier.TargetChanged += OnBuildTargetChanged;
        }

        private void SeedDropdown()
        {
            var buildtypes = GetInstalledBuildTargets();
            selBuildPlatform.choices = buildtypes.Select(bt => GetPrettyPlatformName(bt))
                .OrderBy(bt => bt)
                .ToList();
        }

        private void OnLoadBuildTarget()
        {
            BuildTarget currentTarget = EditorUserBuildSettings.activeBuildTarget;
            OnBuildTargetChanged(BuildTarget.NoTarget, currentTarget);
        }

        private void OnBuildConfigChanged(ChangeEvent<Object> evt)
        {
            buildConfig = (BuildConfigSO)evt.newValue;
            if (buildConfig == null)
            {
                foreach (var label in overviewInfoLabels)
                {
                    label.text = "No Build config";
                }
                lblNoBuildIcon.RemoveFromClassList("hide");
                imgBuildIcon.image = null;
                btnBuild.SetEnabled(false);
                UpdateNotifications("Please load a build config to start.");
                return;
            }

            // update window
            lblBuildType.text = buildConfig.BuildType.ToString();

            if (buildConfig.BuildFileName != string.Empty)
            {
                lblBuildFileName.text = buildConfig.BuildFileName;
            }
            else
            {
                lblBuildFileName.text = "Default file path";
            }

            if (buildConfig.BuildIcon != null)
            {
                lblNoBuildIcon.AddToClassList("hide");
                imgBuildIcon.image = buildConfig.BuildIcon;
            }
            else
            {
                lblNoBuildIcon.RemoveFromClassList("hide");
                lblNoBuildIcon.text = "Default icon";
                imgBuildIcon.image = null;
            }

            btnBuild.SetEnabled(true);

            // send new notification
            UpdateNotifications("Build config loaded.");
        }

        private void OnBuildTargetChanged(BuildTarget _, BuildTarget newTarget)
        {
            currentTarget = newTarget;
            bool isMobile = newTarget == BuildTarget.Android || newTarget == BuildTarget.iOS;
            lblPlatform.text = $"{GetPrettyPlatformName(newTarget)} (Is Mobile: {isMobile})";
            selBuildPlatform.value = GetPrettyPlatformName(newTarget);
        }

        private void OnBuildPlatfomChanged(ChangeEvent<string> evt)
        {
            var selectedBuildTarget = GetEnumFormat(evt.newValue);
            if (currentTarget != selectedBuildTarget)
            {
                btnChangeBuildPlatform.SetEnabled(true);
            }
            else
            {
                btnChangeBuildPlatform.SetEnabled(false);
            }
        }

        private void OnBuild()
        {
            // 1. Cache original values
            string originalProductName = PlayerSettings.productName;
            string originalAndroidId = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
            string originaliOSId = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS);
            Texture2D[] originalIcons = PlayerSettings.GetIcons(NamedBuildTarget.Unknown, IconKind.Application);

            // apply config icon if available
            if (buildConfig.BuildIcon != null)
            {
                PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new Texture2D[] { buildConfig.BuildIcon }, IconKind.Application);
            }
            else
            {
                Debug.LogWarning($"No icon in build config. Using default icon instead.");
            }

            // Apply profile names and bundle identifiers
            PlayerSettings.productName = buildConfig.BuildFileName == "" ? originalProductName : buildConfig.BuildFileName;
            var suffix = "." + buildConfig.BuildType.ToString().ToLower();
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, originalAndroidId + suffix);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, originaliOSId + suffix);

            // Confige build targets and paths
            BuildPlayerOptions options = new()
            {
                scenes = GetProjectScenes(),
                target = currentTarget,
                options = buildConfig.BuildType == BuildType.Dev ? BuildOptions.Development | BuildOptions.AllowDebugging : BuildOptions.None,
                locationPathName = GetBuildPath(currentTarget, PlayerSettings.productName, buildConfig.BuildType.ToString())
            };

            Debug.Log($"[Profile Builder] Starting {buildConfig.BuildType} build for {currentTarget}...");

            // Run the compilation
            BuildPipeline.BuildPlayer(options);

            // Restore everything back to normal
            PlayerSettings.productName = originalProductName;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, originalAndroidId);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, originaliOSId);
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, originalIcons, IconKind.Application);

            Debug.Log($"[Profile Builder] {buildConfig.BuildType} build finished. Settings restored.");
            UpdateNotifications("Build is completed");
        }

        private void UpdateBuildPlatform()
        {
            var selectedBuildTarget = GetEnumFormat(selBuildPlatform.value);

            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(selectedBuildTarget);
            if (group == BuildTargetGroup.Unknown)
            {
                UpdateNotifications($"Could not find a valid BuildTargetGroup for {GetPrettyPlatformName(selectedBuildTarget)}.");
                return;
            }

            if (EditorUserBuildSettings.activeBuildTarget == selectedBuildTarget)
            {
                UpdateNotifications($"Target is already set to {GetPrettyPlatformName(selectedBuildTarget)}.");
                return;
            }

            bool success = EditorUserBuildSettings.SwitchActiveBuildTarget(group, selectedBuildTarget);

            if (success)
            {
                UpdateNotifications($"Successfully initiated switch to {GetPrettyPlatformName(selectedBuildTarget)}.");
            }
            else
            {
                UpdateNotifications($"Failed to switch build target to {GetPrettyPlatformName(selectedBuildTarget)}.");
            }

            btnChangeBuildPlatform.SetEnabled(false);
        }

        private void UpdateNotifications(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            lblNotification.text = text;
        }

        private static string[] GetProjectScenes()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            string[] scenePaths = new string[scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
            {
                if (!scenes[i].enabled) continue;
                scenePaths[i] = scenes[i].path;
            }
            return scenePaths;
        }

        private static string GetBuildPath(BuildTarget target, string baseName, string folderName)
        {
            string root = $"Builds/{folderName}";
            return target switch
            {
                BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64 => $"{root}/Windows/{baseName}.exe",
                BuildTarget.StandaloneOSX => $"{root}/Mac/{baseName}.app",
                BuildTarget.StandaloneLinux64 => $"\"{{root}}/Linux/{baseName}x86_64",
                BuildTarget.Android => $"{root}/Android/{baseName}.apk",
                BuildTarget.iOS => $"{root}/iOS_XcodeProject",
                BuildTarget.WebGL => $"{root}/Web/",
                _ => $"{root}/Unknown_{target}/{baseName}",
            };
        }

        private static BuildTarget[] GetInstalledBuildTargets()
        {
            List<BuildTarget> installedTargets = new();

            foreach(BuildTarget target in (BuildTarget[])System.Enum.GetValues(typeof(BuildTarget)))
            {
                if (target == BuildTarget.NoTarget) continue;

                BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);
                if (group == BuildTargetGroup.Unknown) continue;

                bool isSupported = BuildPipeline.IsBuildTargetSupported(group, target);

                if (isSupported)
                {
                    installedTargets.Add(target);
                }
            }

            return installedTargets.ToArray();
        }

        private string GetPrettyPlatformName(BuildTarget target)
        {
            return target switch
            {
                BuildTarget.StandaloneWindows64 => "Windows (64-bit)",
                BuildTarget.StandaloneWindows => "Windows (32-bit)",
                BuildTarget.StandaloneLinux64 => "Linux (64-bit)",
                BuildTarget.StandaloneOSX => "macOS",
                BuildTarget.Android => "Android",
                BuildTarget.iOS => "iOS",
                BuildTarget.WebGL => "WebGL",
                _ => target.ToString()
            };
        }

        private BuildTarget GetEnumFormat(string prettyName)
        {
            return prettyName switch
            {
                "Windows (64-bit)" => BuildTarget.StandaloneWindows64,
                "Windows (32-bit)" => BuildTarget.StandaloneWindows,
                "Linux (64-bit)" => BuildTarget.StandaloneLinux64,
                "macOS" => BuildTarget.StandaloneOSX,
                "Android" => BuildTarget.Android,
                "iOS" => BuildTarget.iOS,
                "WebGL" => BuildTarget.WebGL,
                _ => System.Enum.Parse<BuildTarget>(prettyName)
            };
        }
    }
}
