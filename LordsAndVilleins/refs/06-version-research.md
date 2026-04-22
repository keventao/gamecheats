# 06 — Version 调研

> 调研于 2026-04-XX

## 当前安装版本

执行命令:

```powershell
Get-FileHash "E:\SteamLibrary\steamapps\common\Lords & Villeins\Lords and Villeins_Data\Managed\Assembly-CSharp.dll" -Algorithm SHA256
(Get-Item "E:\SteamLibrary\steamapps\common\Lords & Villeins\Lords and Villeins.exe").VersionInfo | Format-List
```

填结果:

- 游戏内显示版本(从主菜单或设置页):`<FILL_GAME_VERSION>`(例 `"1.4.2"`)
- `Lords and Villeins.exe` FileVersion:`<FILL_EXE_FILEVERSION>`
- `Assembly-CSharp.dll` SHA256:`<FILL_DLL_SHA256>`

## 兼容白名单(初始)

Phase 4 Plugin.cs 里 `KnownCompatibleVersions` 直接用本字段:

```csharp
private static readonly string[] KnownCompatibleVersions = {
    "<FILL_GAME_VERSION>",   // 例:"1.4.2"
};
```

## 不在白名单时的行为

仅 `Logger.LogWarning(...)`,继续加载。Mod 仍尝试 patch,失败的 patch 会自动降级到 Broken 状态。
