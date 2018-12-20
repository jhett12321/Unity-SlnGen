namespace SlnGen.Editor
{
    internal class Configuration
    {
        public readonly string Name;
        public readonly string InvalidDefines;
        public readonly BaseType BaseConfigType;

        public readonly bool UseEditorReferences;

        public Configuration(string name, BaseType baseConfigType, bool useEditorReferences, string invalidDefines)
        {
            Name = name;
            BaseConfigType = baseConfigType;
            InvalidDefines = invalidDefines;
            UseEditorReferences = useEditorReferences;
        }

        public enum BaseType
        {
            Debug,
            Release
        }
    }
}