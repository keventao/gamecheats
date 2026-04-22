# 06 — Version 调研

> 调研于 2026-04-22

## 当前安装版本

执行命令:

```powershell
Get-FileHash "<LAV_GAME_ROOT>\Lords and Villeins_Data\Managed\Assembly-CSharp.dll" -Algorithm SHA256
(Get-Item "<LAV_GAME_ROOT>\Lords and Villeins.exe").VersionInfo | Format-List
```

填结果:

- 游戏内显示版本(从主菜单或设置页):`1.6.15`(来源: Player.log `Game version:` 字段)
- `Lords and Villeins.exe` FileVersion:`2021.3.45.8976527`(Unity 引擎版本)
- `Assembly-CSharp.dll` SHA256:`0051905181f064cb0487909da5e4898a7988e97c6064ec5402691a232274eebf`

## 兼容白名单(初始)

Phase 4 Plugin.cs 里 `KnownCompatibleVersions` 直接用本字段:

```csharp
private static readonly string[] KnownCompatibleVersions = {
    "1.6.15",
};
```

## 不在白名单时的行为

仅 `Logger.LogWarning(...)`,继续加载。Mod 仍尝试 patch,失败的 patch 会自动降级到 Broken 状态。
