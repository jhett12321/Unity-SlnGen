namespace SlnGen.Editor
{
    internal class Platform
    {
        // TODO Platform References
        public readonly string Name;
        public readonly string[] Defines;

        public Platform(string name, params string[] defines)
        {
            Name = name;
            Defines = defines;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}