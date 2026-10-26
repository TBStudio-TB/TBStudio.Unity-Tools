using UnityEditor;
using UnityEditor.Build;
using System;

namespace TBStudio.Tools.Editor.Helpers
{
    internal class BuildTargetChangeNotifier : IActiveBuildTargetChanged
    {
        public int callbackOrder => 0;

        public static event Action<BuildTarget, BuildTarget> TargetChanged;

        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            TargetChanged?.Invoke(previousTarget, newTarget);
        }
    }
}
