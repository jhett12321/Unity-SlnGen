namespace SlnGen.Editor
{
    internal static class SlnGen
    {
        public static readonly Platform CurrentPlatform;
        public static readonly Platform[] Platforms;

        public static readonly Configuration DefaultConfig;
        public static readonly Configuration[] Configurations;

        static SlnGen()
        {
            Platforms = new[]
            {
                new Platform("_Current Platform_"),
                new Platform("Windows", "PLATFORM_STANDALONE", "PLATFORM_STANDALONE_WIN", "UNITY_STANDALONE", "UNITY_STANDALONE_WIN", "UNITY_EDITOR_WIN"),
                new Platform("OSX", "PLATFORM_STANDALONE", "PLATFORM_STANDALONE_OSX", "UNITY_STANDALONE", "UNITY_STANDALONE_OSX", "UNITY_EDITOR_OSX"),
                new Platform("Linux", "PLATFORM_STANDALONE", "PLATFORM_STANDALONE_LINUX", "UNITY_STANDALONE", "UNITY_STANDALONE_LINUX"),
                new Platform("Wii", "PLATFORM_WII", "UNITY_WII"),
                new Platform("iOS", "PLATFORM_IOS", "UNITY_IOS"),
                new Platform("Android", "PLATFORM_ANDROID", "UNITY_ANDROID"),
                new Platform("PS4", "PLATFORM_PS4", "UNITY_PS4"),
                new Platform("Xbox One", "PLATFORM_XBOXONE", "UNITY_XBOXONE"),
                new Platform("Tizen", "PLATFORM_TIZEN", "UNITY_TIZEN"),
                new Platform("tvOS", "PLATFORM_TVOS", "UNITY_TVOS"),
                new Platform("WebGL", "PLATFORM_WEBGL", "UNITY_WEBGL")
            };

            Configurations = new[]
            {
                new Configuration("Editor", Configuration.BaseType.Debug, true, null),
                new Configuration("Player (Debug)", Configuration.BaseType.Debug, false, @"\bUNITY_EDITOR\b|\bUNITY_EDITOR_64\b|\bUNITY_EDITOR_WIN\b|\bUNITY_EDITOR_OSX\b|\bUNITY_EDITOR_LINUX\b"),
                new Configuration("Player", Configuration.BaseType.Release, false, @"\bDEBUG\b|\bUNITY_EDITOR\b|\bUNITY_EDITOR_64\b|\bUNITY_EDITOR_WIN\b|\bUNITY_EDITOR_OSX\b|\bUNITY_EDITOR_LINUX\b"),
            };

            DefaultConfig = Configurations[0]; // Editor
            CurrentPlatform = Platforms[0]; // Current Platform
        }
    }
}