using System.Collections.Generic;
using UnityEngine;

namespace TBStudio.Tools.TaskList
{
    [CreateAssetMenu(fileName = "New Task List", menuName = "TB Studio Tools/Task List")]
    public class TaskListSO : ScriptableObject
    {
        [SerializeField] private List<string> tasks = new();

        public List<string> Tasks => tasks;

        public void AddTasks(List<string> savedTasks)
        {
            tasks.Clear();
            tasks = savedTasks;
        }

        public void AddTask(string taskName)
        {
            tasks.Add(taskName);
        }
    }
}
