namespace SlnGen.Editor
{
    internal class Project
    {
        public string FullProjectString;
        public string ProjectId;
        public bool IsEditorProject;

        public Project(string projectString)
        {
            FullProjectString = projectString;

            // double quote (1) + GUID Length with parenthesis (38)
            ProjectId = projectString.Substring(projectString.Length - 39, 38);
            IsEditorProject = projectString.ToLower().Contains("editor");
        }
    }
}