# 会话记录：本机 LLM 环境 + Linux 双系统（2026-09-27）

> 下次继续时，对 pi 说：读一下 `~/session-2026-09-27-local-llm-linux.md`
>
> **此文件同步在私有仓库：https://github.com/keventao/gamecheats（main 分支根目录），换电脑/Mac 的 pi 可从仓库拉取续接。**

---

## 一、本机配置

| 部件 | 规格 | 备注 |
|---|---|---|
| GPU | **RTX 4060 Ti 8GB**（sm_89 / Ada，可用约 7.5GB） | 推理主要瓶颈 |
| CPU | i7-13700KF，12 核 24 线程 | |
| 物理内存 | **32GB**（海盗船 2×16GB DDR4-4000） | WSL2 默认只分到 16GB |
| 磁盘 0 | Samsung 970 512GB → E 盘（464GB） | |
| 磁盘 1 | Samsung 980 1TB → C 盘(149GB) + D 盘(781GB) | C/D 同物理盘 |
| 系统 | Windows + WSL2，Ubuntu 26.04，内核 6.6，gcc 15.2 / cmake 4.2 | |

- `.wslconfig`（`C:\Users\keven\.wslconfig`）：`networkingMode=mirrored` + TUN，WSL 流量由 Windows 侧 Clash 透明接管（出口实测新加坡）。**WSL 内无需装代理**。
- 待办建议：`.wslconfig` 加 `memory=24GB`。

## 二、模型调研结论（HF，1-9B）

- 官方 **Qwen3.8 无小尺寸成员**：主线 27B；Flash-Next 实为 125B/6B 激活的 n-gram+MoE（Q8 达 188GB），均排除。
- **已选用并下载**：`empero-ai/Qwen3.8-9B-Distill-GGUF`
  - 名字叫 3.8，实际架构是 **Qwen3.5-9B**（Gated DeltaNet 混合注意力 + 多模态），以 Qwen3.8-27B 为教师蒸馏。
  - 文件：`~/models/Qwen3.8-9B-Q4_K_M.gguf`（5.78GB，已完整下载，5.37 GiB 占用）。
- 备选（未测）：`Qwen/Qwen3-8B-GGUF` Q4_K_M（5.03GB），标准架构，预计 60-70 tok/s。
- 其他候选：openbmb/MiniCPM5-2B；google/gemma-3-1b-it。

## 三、llama.cpp 编译（已完成）

- 仓库：`~/llama.cpp`（git clone，commit 9adc7f4）
- CUDA：apt 装的 `cuda-toolkit-13`（13.1），nvcc 在 `/usr/local/cuda/bin/nvcc`（不在 PATH）
- **踩坑 1**：gcc 15 太新 nvcc 不支持 → 装 `gcc-13/g++-13` 作宿主
- **踩坑 2（关键）**：CUDA 13.1 头文件与 Ubuntu 26.04 glibc 2.43 冲突（`rsqrt/rsqrtf` 的 noexcept 不兼容）
  - 解决：复制 CUDA 头文件到 `~/cuda-patch/`，给 `crt/math_functions.h` 第 629/653 行声明加 `noexcept(true)`
- 配置命令（复现用）：
  ```bash
  export PATH=/usr/local/cuda/bin:$PATH
  cd ~/llama.cpp
  cmake -S . -B build -DGGML_CUDA=ON \
    -DCMAKE_CUDA_ARCHITECTURES=89 \
    -DCMAKE_CUDA_COMPILER=/usr/local/cuda/bin/nvcc \
    -DCMAKE_C_COMPILER=gcc-13 -DCMAKE_CXX_COMPILER=g++-13 \
    -DCMAKE_CUDA_FLAGS="-I$HOME/cuda-patch" -DLLAMA_CURL=OFF
  cmake --build build --config Release -j 24
  ```
- 产物：`build/bin/llama-cli`、`llama-bench`（CUDA .so 在 build/bin 内）

### Benchmark（9B Q4_K_M，全量进显存，ngl 99）
| 测试 | 结果 |
|---|---|
| pp512（prompt 处理） | 2653 t/s |
| tg128（生成） | **45.24 t/s** |

45 t/s 可用但低于预期（60-90），原因是 qwen3.5 混合架构太新、专用 CUDA kernel 未优化。想对比可测 Qwen3-8B。

运行示例：
```bash
~/llama.cpp/build/bin/llama-cli -m ~/models/Qwen3.8-9B-Q4_K_M.gguf -ngl 99 -p "你好"
```

## 四、Linux 双系统调研（为了玩 Steam）

**结论：装 Windows + Linux 双系统。**
- 2026 现状：30,006 个 Steam 游戏 Verified/Playable（Proton 11 / Wine 11）；单机和多数合作游戏体验≈Windows。
- **唯一硬伤是反作弊**：1166 个带反作弊游戏中 640 个 Linux 跑不了（需开发商手动开）。
  - ❌ 瓦罗兰特(Vanguard)、PUBG、Apex、使命召唤等；✅ CS2、原神、命运2 等。
- 发行版选 **Bazzite**（游戏向不可变 Fedora，事实标准）；备选 Nobara / Pop!_OS。
- 装前：关 Windows 快速启动、备份、查 protondb.com。
- Steam 游戏迁移：复制整个 `steamapps`（含 `common/` + `appmanifest_*.acf`）→ Steam 验证完整性，只补少量文件；云存档自动同步。NTFS 上直接跑 Proton 不推荐，常玩的复制到 Linux 分区。
- Clash Verge：找 **Clash Verge Rev**，支持 Linux（deb/rpm/AppImage）；WSL 内不需要。

## 五、磁盘扫描结果

### D 盘（781GB，游戏盘，650GB 几乎全是游戏）
- SteamLibrary 453.6GB | Ubisoft 92.6GB（在 Program Files (x86)）| WoW 89.5GB | Games 9.2GB（今古群侠传）
- 非游戏仅 ~2.1GB：Bionic、百度拼音、无影云电脑、网易DD

### E 盘（970 512GB）扫描时状态
- Games 244.2GB（暗黑4 180.7 + v1.5.5 55.8 + 今古群侠传 7.3 等）
- Linux/ext4.vhdx 26.7GB（**就是当前 WSL Ubuntu-26.04 的虚拟盘**，注册路径 E:\Linux）
- Steam 客户端 22.7GB、SteamLibrary 17.2GB、黑神话下载+MOD ~15GB
- Unity/Editor 5.6GB、JetBrains 1.6GB、Python39、KeePassXC
- 哈希名目录 b93221e2...（6GB，HLS 视频分片缓存，可清）、根目录散落 6.3GB

## 六、本次已执行的磁盘操作

1. ✅ **永久删除 `E:\Games`（244.2GB）**（D 盘 Games 保留未动）
   - 删除后 E 盘：已用 351→107GB，空闲 114→358GB（占用率 23%）
2. ⏳ **切盘 300GB（进行中/待确认）**
   - 障碍：E 盘虽有 358GB 空闲，但 ext4.vhdx 运行中被锁、位于盘尾，最初只能收缩 26GB。
   - 方案脚本已生成：`C:\Users\keven\split-e.ps1`
     - 步骤：wsl --shutdown → diskpart compact vhdx → 碎片整理 E → 收缩 300GB → 新建 NTFS 卷标 Data
     - 若收缩量仍不足 300GB 会安全退出不动分区。
   - 用户需在**管理员 PowerShell** 执行：
     ```powershell
     Set-ExecutionPolicy Bypass -Scope Process -Force
     C:\Users\keven\split-e.ps1
     ```

## 七、下次待办

- [x] ~~确认 split-e.ps1 是否已执行、新 300GB Data 分区是否建立、WSL 是否正常~~（已完成，见第八节）
- [ ] （可选）下载 Qwen3-8B Q4_K_M 跑 bench 对比 45 t/s
- [ ] （可选）`.wslconfig` 加 `memory=24GB`
- [x] ~~决定双系统装哪块盘~~（定了：970 整块盘）
- [x] ~~清理 E 盘~~（E 卷整体已删除）
- [ ] 查常玩游戏清单的 ProtonDB 兼容性

## 八、Bazzite 安装进展（2026-09-27 当晚续）

### 已完成
1. ✅ **split-e.ps1 已执行**：原 E（464GB）切成 E:164GB + F:300GB
2. ✅ **WSL 已迁移**：注册表 BasePath = `D:\WSL\Ubuntu-26.04`，ext4.vhdx（24GB）在 D 盘，970 盘上已无 WSL 数据
3. ✅ **E、F 分区已删除**（管理员 PowerShell `Remove-Partition`），970 现状：
   - 0.5GB System(**EFI，Windows 启动分区，必须保留**) + 0.1GB Reserved + 0.6GB Recovery
   - **464.6GB 未分配空间**
   - 磁盘 1（980）的 C/D 完好无损
4. ✅ `powercfg /h off` 已执行（关快速启动）
5. ✅ 四个盘 BitLocker 均为 Off

### U 盘问题与最终路线（重要）
- Bazzite nvidia 镜像 **7.9GB**，手头 SanDisk Cruzer Edge 标称 8GB 实际只有 7.36GB → **装不下**，也没有更大 U 盘。
- **采用官方 Alternative Install：先装 Fedora Kinoite（小镜像）→ 在线 rebase 成 Bazzite**
  - 文档：https://docs.bazzite.gg/General/Installation_Guide/alternate-install-guide/
- 已下载并校验：
  - `C:\Users\keven\Downloads\Fedora-Kinoite-ostree-x86_64-44-1.7.iso`（4,092,461,056 字节）
  - SHA256 `4a944312b4e861ab625fd9786957174ef122a8a406bbb54caba7665e0d9f0e92` 与官方一致 ✅
  - FedoraMediaWriter-win64-5.3.2.exe 也已下载
- U 盘盘符：SanDisk 现为 **G:**（7.36GB，exFAT）；注意删 E/F 后它一度抢占了 E 盘符，已用 `Set-Partition -NewDriveLetter G` 改回。

### 待执行步骤
1. [ ] Fedora Media Writer → Custom Image 选 Kinoite ISO → 写入 SanDisk（G:）
2. [ ] U 盘启动 → 装 Kinoite 到 970 空闲空间；**安装时不要设置 root 账户**
3. [ ] 进系统后 rebase（无进度条，耗时较长）：
   ```bash
   rpm-ostree rebase ostree-unverified-registry:ghcr.io/ublue-os/bazzite-nvidia:stable
   ```
4. [ ] 重启后补默认软件 + 签名校验：
   ```bash
   ujust _install-system-flatpaks   # 选 Flathub / System
   ujust verify-image              # 完后再重启
   ```
5. [ ] 若开 Secure Boot：rebase 前按 Bazzite README 的 Secure Boot 说明做 enrollment

### 备注
- 970 标称 500GB（十进制），系统显示 465.8GB（GiB）属正常，非缺容量。
- 桌面环境选 KDE（bazzite-nvidia 即 KDE）；Kinoite 正是 KDE Atomic，对应正确。

### 安装时“不要设置 root 账户”详解
- **root** = Linux 超级管理员，权限最高。安装器（Anaconda）除了创建日常用户（keven），还有一个单独的 **Root Account（根账户）** 页面。
- Fedora/Bazzite 默认设计：**root 账户保持锁定/禁用**，需要管理员权限时用日常账户执行 `sudo 命令`（输自己的密码）。
- 安装时创建的第一个用户自动加入 **wheel 组**，自带 sudo 权限，无需 root 密码即可做所有管理操作。
- 手动设 root 密码会偏离 Bazzite 默认配置，rebase 后可能引入不必要差异。
- 具体操作：在 **Root Account** 界面选 **Disable/锁定根账户**（或跳过、保持默认），不勾 “Allow root login with password”，密码框**留空**。
- ⚠️ 区分：**root 密码留空 ≠ 用户密码留空**；自己的用户（keven）用户名和密码必须照常设置。

### 上海网络与更新问题（重要）
- Bazzite 系统更新从 **ghcr.io（GitHub 容器仓库）** 拉 OCI 镜像层，国内直连基本被墙/极不稳定；软件走 **Flathub**（能连但一般）。
- 双系统 Bazzite 与 Windows 独立，**不会共享 Windows 侧 Clash**，需在 Linux 里单独装代理。
- Flathub 可换国内镜像缓解，但 **ghcr.io 无国内镜像，系统更新/rebase 必须走代理**。

### Clash Verge Rev（Linux 版已备好）
- 官网有 Linux 版（deb/rpm，v2.5.6 无 AppImage）。
- ✅ 已下载：`C:\Users\keven\Downloads\Clash.Verge-2.5.6-1.x86_64.rpm`（98MB，借 WSL 代理先下好，避免装完系统裸连 GitHub 太慢）。
- Bazzite/Kinoite 里安装（不可变系统装本地 rpm）：
  ```bash
  rpm-ostree install ~/Clash.Verge-2.5.6-1.x86_64.rpm
  systemctl reboot
  ```
- 启动后导入订阅，设置开 **TUN Mode**（系统级接管流量，终端 rpm-ostree/flatpak 才能走代理），`curl ipinfo.io` 验证出口新加坡。
- **调整后的执行顺序：Kinoite 装好 → 立刻装 Clash 开 TUN → 再 rebase Bazzite**（rebase 本身也从 ghcr.io 下载，量大）。

### 关于图形界面（给第一次用 Linux 桌面的自己）
- Kinoite/Bazzite 是**完整图形桌面（KDE Plasma）**，不是命令行系统：有桌面、任务栏、开始菜单、系统托盘、文件管理器 Dolphin、Firefox、图形设置、Steam、应用商店。
- U 盘启动选 Start Fedora Kinoite 直接进图形 Live 桌面，双击桌面图标开始图形化安装。
- 终端只是可选快捷工具，rebase/装 Clash 用命令是因为复制粘贴更快不易错；日常使用可全程鼠标。

### 常玩游戏 Linux 兼容性（2026-09-27 核实）
| 游戏 | 等级 | 运行方式 | 备注 |
|---|---|---|---|
| RimWorld | 🟢 原生 | Steam 直装 | 官方 Linux 版，创意工坊 MOD 正常 |
| Valheim | 🟢 原生 | Steam 直装 | 官方 Linux 版；BepInEx 有 Linux 版 |
| Project Zomboid | 🟢 原生 | Steam 直装 | 官方 Linux 版，工坊 MOD/联机正常 |
| 黑神话：悟空 | 🟡 Proton Gold | Steam + Proton | 无反作弊；4060 Ti 台式机体验接近 Windows（Deck 才跑不动） |
| 魔兽世界 | 🟡 Lutris Gold | Lutris + 战网脚本 | DX11/DX12 均可，多数人帧数更高；暴雪大版本后偶尔需等脚本更新 |
| 辐射 76（误打误撞查到） | 🟡 Proton Gold | Steam + Proton | 网游、无强制 Linux 反作弊问题，96GB |

- 前三个零配置；黑神话 Bazzite 默认开 Proton 直接启动。
- WoW 装法：Lutris（商店可装）搜 World of Warcraft 一键脚本自动配战网。D 盘已有 89.5GB WoW 文件，可尝试复制到 Linux 分区让战网验证复用。
- 网游通病：暴雪更新后偶尔短暂抽风，一般几天内 Lutris 脚本更新修复。

## 九、最新进度（当晚 22:40）与 Mac 续接点

- ✅ Fedora Media Writer 已把 Kinoite ISO 写入 SanDisk U 盘（G:），写入+校验完成（用户看到 76% 后继续等待完成）。
- ⏭️ **下一步（从这里继续）**：
  1. 重启 → 启动菜单（华硕 F8 / 微星 F11 / 技嘉 F12）→ 选 **UEFI: SanDisk**
  2. Start Fedora Kinoite 44 → Live 桌面双击 Install to Hard Drive
  3. 安装目标 **Samsung 970（466GB）空闲空间**，自动分区；**Root 账户留空/禁用**；设自己的用户密码
  4. 装完重启进 Kinoite → **先装 Clash Verge（rpm 在 D 盘 Downloads）开 TUN → 再 rebase bazzite-nvidia:stable**
  5. rebase 重启后 `ujust _install-system-flatpaks`、`ujust verify-image`
- Mac 上续接：`gh repo clone keventao/gamecheats`（或 git pull）→ 让 pi 读其中的本 md 即可。

### 主板与启动按键
- 主板：**华硕 ASUS TUF GAMING B760M-PLUS**（mATX，BIOS 1820，纯 UEFI/GPT）。
- **F8**：启动设备菜单（选 UEFI: SanDisk）；**Del/F2**：进 BIOS。
- U 盘不出现：BIOS 里 CSM=Disabled，Secure Boot 保持开启。
- 默认启动项调整：BIOS Boot Priority，或 Bazzite 内 `efibootmgr`。
