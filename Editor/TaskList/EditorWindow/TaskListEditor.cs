using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TBStudio.Tools.Editor.TaskList
{
    public class TaskListEditor : EditorWindow
    {
        private VisualElement container;

        #region References MenuItems
        private ObjectField objTaskList;
        private Button btnLoadList;
        private ToolbarSearchField searchTasks;
        private TextField txtTask;
        private Button btnAddTask;
        private ScrollView scrollTaskList;
        private Button btnSaveProgress;
        private ProgressBar pbTasks;
        private Label lblNotification;
        #endregion

        private TaskListSO taskListSO;
        private const string rootPath = "Packages/com.tbstudio.tools/Editor/SharedEditorLayouts/";
        private const string path = "Packages/com.tbstudio.tools/Editor/TaskList/EditorWindow/";

        [MenuItem("TB Studio/Window/Task List")]
        private static void ShowWindow()
        {
            TaskListEditor window = GetWindow<TaskListEditor>("Task List");
            window.minSize = new Vector2(400, 600);
            window.maxSize = new Vector2(1200, 800);
        }

        private void CreateGUI()
        {
            LoadWindowLayout();

            GetMenuItemsReferances();

            SetObjectFieldType();

            AddButtonEventListeners();

            AddCustomEvents();

            UpdateNotifications("Please load a task list to continue.");
        }

        private void LoadWindowLayout()
        {
            container = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path + "TaskListEditor.uxml");
            visualTree.CloneTree(container);

            var rootStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(rootPath + "root.uss");
            container.styleSheets.Add(rootStyleSheet);

            var windowStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path + "TaskListEditor.uss");
            container.styleSheets.Add(windowStyleSheet);

            var taskItemStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path + "TaskItem.uss");
            container.styleSheets.Add(taskItemStyleSheet);
        }

        private void GetMenuItemsReferances()
        {
            objTaskList = container.Q<ObjectField>("load-tasks-object-field");
            btnLoadList = container.Q<Button>("load-tasks-button");
            searchTasks = container.Q<ToolbarSearchField>("search-task-search-field");
            txtTask = container.Q<TextField>("add-tast-text-field");
            btnAddTask = container.Q<Button>("add-task-button");
            scrollTaskList = container.Q<ScrollView>("display-tasks-scroll-view");
            btnSaveProgress = container.Q<Button>("save-tasks-button");
            pbTasks = container.Q<ProgressBar>("tasks-progress-bar");
            lblNotification = container.Q<Label>("notifications-label");
        }

        private void SetObjectFieldType()
        {
            objTaskList.objectType = typeof(TaskListSO);
        }

        private void AddButtonEventListeners()
        {
            btnLoadList.clicked += LoadTasks;
            btnAddTask.clicked += AddTask;
            btnSaveProgress.clicked += SaveProgress;
        }

        private void AddCustomEvents()
        {
            searchTasks.RegisterValueChangedCallback(OnSearchTextChanged);
            txtTask.RegisterCallback<KeyDownEvent>(AddTask, TrickleDown.TrickleDown);
        }

        private void LoadTasks()
        {
            taskListSO = objTaskList.value as TaskListSO;

            if (taskListSO == null)
            {
                UpdateNotifications("Failed to load task list.");
                return;
            }

            scrollTaskList.Clear();

            List<string> tasks = taskListSO.Tasks;

            foreach (string taskName in tasks)
            {
                scrollTaskList.Add(CreateTask(taskName));
            }

            UpdateProgress();

            UpdateNotifications($"{taskListSO.name} loaded successfully!");
        }

        private void AddTask()
        {
            if (string.IsNullOrEmpty(txtTask.text)) return;

            scrollTaskList.Add(CreateTask(txtTask.text));

            SaveTask(txtTask.text);

            txtTask.value = string.Empty;

            txtTask.schedule.Execute(() => txtTask.Focus()).ExecuteLater(10);

            UpdateProgress();

            UpdateNotifications($"Task added successfully!");
        }

        private void AddTask(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return 
                || evt.keyCode == KeyCode.KeypadEnter)
            {
                AddTask();

                evt.StopImmediatePropagation();
            }
        }

        private void SaveProgress()
        {
            if (taskListSO == null) return;

            List<string> tasks = new();
            foreach (TaskItem taskItem in scrollTaskList.Children().Cast<TaskItem>())
            {
                Toggle toggle = taskItem.TGLTaskStatus;
                if (!toggle.value)
                {
                    tasks.Add(taskItem.LBLTaskName.text);
                }
            }

            taskListSO.AddTasks(tasks);

            EditorUtility.SetDirty(taskListSO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            LoadTasks();

            UpdateNotifications($"Task saved successfully!");
        }

        private void OnSearchTextChanged(ChangeEvent<string> evt)
        {
            string searchText = evt.newValue.ToLower();
            foreach (TaskItem taskItem in scrollTaskList.Children().Cast<TaskItem>())
            {
                Label taskLabel = taskItem.LBLTaskName;
                string taskText = taskLabel.text.ToLower();
                if (!string.IsNullOrEmpty(searchText) && taskText.Contains(searchText))
                {
                    taskLabel.AddToClassList("highlight-label");
                }
                else
                {
                    taskLabel.RemoveFromClassList("highlight-label");
                }
            }
        }

        private TaskItem CreateTask(string taskName)
        {
            TaskItem taskItem = new(taskName);
            taskItem.TGLTaskStatus.RegisterValueChangedCallback(UpdateProgress);
            return taskItem;
        }

        private void UpdateProgress()
        {
            if (taskListSO == null) return;

            int totalTasks = scrollTaskList.childCount;
            int completedTasks = scrollTaskList
                .Children()
                .Cast<TaskItem>()
                .Count(t => t.TGLTaskStatus.value);
            float progress = totalTasks > 0 ? (float)completedTasks / totalTasks : 0f;

            pbTasks.value = progress;
            pbTasks.title = $"{Mathf.Round(progress * 1000) / 10f} %";

            if (progress > 0) UpdateNotifications("Progress updated. Don't forget to save!");
        }

        private void UpdateProgress(ChangeEvent<bool> _) => UpdateProgress();

        private void SaveTask(string taskName)
        {
            if (taskListSO == null) return;

            taskListSO.AddTask(taskName);

            EditorUtility.SetDirty(taskListSO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void UpdateNotifications(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            lblNotification.text = text;
        }
    }
}
