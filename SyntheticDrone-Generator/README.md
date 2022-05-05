# This is the readme of SyntheticDrone Generator (SDG)

## Attention
- The project is build and tested under Microsoft Visual Studio 2017.
- All mods must be used in the offline version of GTA V.

## Some variable definitions
- `$CODEROOT` : Where you put the VS project Drone. (Where the `Drone.sln` file locate.)
- `$GMAEROOT` : The root path of your GTAV game. (For example `E:\Grand Theft Auto V`, **Pay Attention** the `GTA5.exe` bin file is directly under this path.)
- `$DATAROOT` : Where you put your collected data. (For example `D:\DroneData`.)

## Build SDG:

### Request Libs:
- [Microsoft Visual Studio 2017 (VS2017)](https://my.visualstudio.com/Downloads?q=visual%20studio%202017)
- [ScriptHookVDotNet](https://github.com/crosire/scripthookvdotnet) (The library files `ScriptHookVDotNet3.dll` and `ScriptHookVDotNet3.xml`has been included in the project file.)

### Installation and Compilation:
1. Install [Microsoft Visual Studio 2017](https://my.visualstudio.com/Downloads?q=visual%20studio%202017) with .Net tools, and [.Net Framework 4.8 2017 SDK](https://dotnet.microsoft.com/zh-cn/download/visual-studio-sdks?utm_source=getdotnetsdk&utm_medium=referral)
2. Download the `DroneNew` folder, which is the VS2017 project.
3. Open the `Drone.sln` with VS2017, and select `Debug(调试)` --> `Drone property(Drone 属性)`. 
    - Select `Application(应用程序)`, set `Target Framework(目标框架)` to `.Net Framework 4.8`, set `Output Type(输出类型)` to 'Class Libs(类库)'
    - Select `Generation(生成)`, make sure the Platform is Release, Any CPU, x64.
4. Select `Project(项目)` --> `Add Reference(添加引用)`. 
    - Select `Program Sets(程序集)` and select following libs:`System`,`System.Data.DataSetExtensions`,`System.Windows.Forms`,`System.Net.Http`,`System.Drawing`,`Microsoft.CSharp`,`System.Xml.Linq`,`System.Xml`,`System.Data`,`System.Core`
    - Select `Browse(B) (浏览,右下角的那个)`, and open the `ScriptHookVDotNet3.dll` inside the `$CODEPATH`
5. Modify main file `Drone.cs` to customize mod functions you need.
6. **PPPPPay AAAAttention:** Modify the variable `$dataDir` in line 30 of `Drone.cs` to where you put collected files(`$DATAPATH`).
7. Ust hot key `ctrl + shift + B` to compile the project. You will get `Drone.dll` in folder `$CODEPATH\Drone\bin\Release`.

## Use SDG:

### Request Libs:
- [ScriptHookV](http://www.dev-c.com/gtav/scripthookv/)
- [ScriptHookVDotNet](https://github.com/crosire/scripthookvdotnet)
- [GTAVisionNative](https://github.com/umautobots/GTAVisionExport/tree/master/native)
- [No Chromatic aberration & Lens distortion Mod](https://www.gta5-mods.com/misc/no-chromatic-aberration-lens-distortion-1-41)
- [Drone.dll](#build-sdg): The mod you compiled by the Build SDG part.

### Installation:
1. Follow the [ScriptHookV](http://www.dev-c.com/gtav/scripthookv/) install steps, copy `ScriptHookV.dll` and `dinput6.dll` into `$GAMEROOT`.
2. Install [OpenIV](https://openiv.com/).
3. Install this [No Chromatic aberration & Lens distortion Mod](https://www.gta5-mods.com/misc/no-chromatic-aberration-lens-distortion-1-41) to avoid chromatic aberration and lens distortion. 
   - Open `OpenIV` and select `Tools`-->`ASI Manger` to install mod. 
   - Use `OpenIV` open the `$GAMEROOT\update\update.rpf` and copy `timecycle_mods_1.xml`、`timecycle_mods_3.xml`、`timecycle_mods_4.xml` into `/common/data/timecycle`)
4. Compile or directly use the compiled plugin-in [unlimitedLife](unlimitedLife),[GTAVisionNative](https://github.com/umautobots/GTAVisionExport/tree/master/native). For directly use, just copy the `unlimitedLife.asi`, `GTAVisionNative.asi`, `GTAVisionNative.lib` three files to the `$GAMEROOT` folder.
5. Copy the compiled SDG files `Drone.dll`, `Drone.pdb` and also the lib files `ScriptHookVDotNet3.dll`,'ScriptHookVDotNet3.xml' into the `$GAMEROOT\scripts` folder.
6. Run GTAV in the offline version

### Location Collection
TODO

### Data Collection
TODO
