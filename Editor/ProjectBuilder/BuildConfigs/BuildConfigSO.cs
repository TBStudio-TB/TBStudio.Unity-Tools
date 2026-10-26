using UnityEngine;

namespace TBStudio.Tools.Editor.ProjectBuilder.BuildConfig
{
    [CreateAssetMenu(fileName = "BuildConfig", menuName = "TB Studio Tools/Build Config")]
    public class BuildConfigSO : ScriptableObject
    {
        [SerializeField] private BuildType buildtype;
        [Tooltip("Leave empty for originalName_buildType")]
        [SerializeField] private string buildFileName;
        [SerializeField] private Texture2D buildIcon;

        public BuildType BuildType => buildtype;
        public string BuildFileName => buildFileName;
        public Texture2D BuildIcon => buildIcon;
    }
}
