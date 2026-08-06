using UnityEditor;
using UnityEngine.UIElements;

namespace TBStudio.Tools.Editor.TaskList
{
    public class TaskItem : VisualElement
    {
        #region Referances MenuItems
        public Toggle TGLTaskStatus { get; private set; }
        public Label LBLTaskName { get; private set; }
        #endregion

        private const string path = "Packages/com.tbstudio.tools/Editor/TaskList/Editor Window/";

        public TaskItem(string taskName)
        {
            LoadWindowLayout();

            GetMenuItemsReferances();

            SetTaskName(taskName);
        }

        private void LoadWindowLayout()
        {
            var original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path + "TaskItem.uxml");
            original.CloneTree(this);
        }

        private void GetMenuItemsReferances()
        {
            TGLTaskStatus = this.Q<Toggle>("tglTaskStatus");
            LBLTaskName = this.Q<Label>("lblTaskName");
        }

        private void SetTaskName(string taskName)
        {
            LBLTaskName.text = taskName;
        }
    }
}
