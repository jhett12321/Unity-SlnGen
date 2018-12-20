# Unity-SlnGen
SlnGen is a editor plugin that generates project/solution configurations for different targets in the Unity Editor.

![VS Preview](https://github.com/jhett12321/Unity-SlnGen/raw/master/config-switch.gif)

![VS Build Config](https://github.com/jhett12321/Unity-SlnGen/raw/master/config-build.gif)

## Features
* Adds solution configurations for Editor, Player, and Development Builds.
* Compile-time checking (in IDE) for editor, and platform code usage.

## Compatibility
Unity: 2018.1 or greater

IDEs: Tested with Visual Studio 2017, Rider 2018.

## Installation
### 2018.3 or Higher

Add the following line to your project's "/Packages/manifest.json" file.

```
"com.blackfeatherproductions.slngen": "https://github.com/jhett12321/Unity-SlnGen.git"
```

### Others

See [Releases](https://github.com/jhett12321/Unity-Slngen/releases) for a .unitypackage.
