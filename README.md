# Chireiden.TShock.Omni & Misc

- 作者: SGKoishi
- 原版仓库: [sgkoishi/yaaiomni](https://github.com/sgkoishi/yaaiomni)
- 本分支适配: **TShock 6.1.0 / Terraria 1.4.5.6 / .NET 9.0**

TShock 的又一多功能插件集合，包含修复补丁、功能增强、实用工具、调试命令等。

> **注意**
> 如果你使用 Linux 且不确定下载哪个版本，请下载 tarball 压缩包。

---

## 版本适配说明

本分支基于原版 `yaaiomni` 适配了 **TShock 6.1.0**（适用于 Terraria 1.4.5.6 / .NET 9.0）。

### 主要适配改动

1. **项目目标框架升级**：net6.0 → net9.0，TShock 依赖更新至 6.1.0
2. **实例方法 Detour 签名修复**：`BanManager.CheckBan` 等实例方法的 Detour 委托需包含类实例作为首个参数
3. **文化重定向修复**：`RedirectLanguage` 事件需在 `ResetGameLocale()` 之前注册，确保 zh-Hans 正确重定向为 zh-CN
4. **PlayerData 构造函数更新**：`new PlayerData(TSPlayer)` 已过时，改用 `new PlayerData()`
5. **CheckBan 反射精确匹配**：方法名匹配从 `Contains("CheckBan")` 改为 `== "CheckBan"`
6. **ipa: 子网封禁逻辑修复**：修复了网络位/主机位检查颠倒的问题
7. **IPv4/IPv6 地址族一致性检查**：在 BitArray.Xor 前添加 AddressFamily 验证，防止越界异常
8. **PlayerSlot 包格式适配**：Terraria 1.4.5.6 在 PlayerSlot 包中新增了 `slotFlags` 字节（favorited + blockedSlot），包长度从 9 变为 10。修复 `InventorySlotPE` 的长度检查从 `== 9` 改为 `>= 9`，否则所有 PlayerSlot 包会被取消，导致库存不同步、Bouncer 拒绝所有图格放置
9. **多项稳定性改进**：
   - Dispose 中正确注销 AppDomain 事件，防止内存泄漏
   - PacketSpam 异常处理优化（throw → 日志+跳过）
   - FirstChance 异常集合大小限制（上限 1000 条）
   - 后台任务异常处理（防止进程崩溃）
   - Ping 回调异常日志记录

---

### 常用功能

- `/whynot` 查看玩家最近的权限查询记录，终极解决"需要什么权限"类问题
- `/setlang`, `/maxplayers` 设置服务器语言和最大玩家数
- `/settimeout`, `/setinterval`, `/clearinterval`, `/showdelay` 基于定时器自动执行命令
- `/runas` 以其他玩家身份执行命令
- `/resetcharacter`, `/exportcharacter` 重置或导出角色数据
- 聊天防刷屏限制：3条/5秒，5条/20秒（配置项 `.Mitigation.ChatSpamRestrict`）

### 更多特性

- `.PlayerWildcardFormat`: 支持`/g zenith *all*`式通配符
- `.HideCommands`和`.StartupCommands`可隐藏命令或设置启动时自动执行
- `.Enhancements.AlternativeCommandSyntax`支持`/命令1 ; 命令2 ; 命令3...`和`/命令1 && 命令2 && 命令3...`语法
- `.Mode.Vanilla.Enabled`会为玩家添加原版游戏体验所需权限
- `.CommandRenames`: 支持命令别名配置，如`{"Chireiden.TShock.Omni.Plugin.Command_PermissionCheck": ["whynot123", "whynot456"]}`
- **IP 子网封禁**：支持 `ipa:` 前缀的 CIDR 封禁规则（如 `ipa:192.168.1.0/24`）
- **正则封禁**：封禁规则支持正则表达式匹配

### 高级选项

执行`/genconfig`可生成完整配置文件。隐藏选项将显示（未修改的条目会在下次启动/重载时恢复隐藏状态）。

> **警告**
> **保持默认设置。除非您明确知道修改后果，否则请勿更改**

你可以访问所有隐藏功能并控制它们的行为。更多详情请参考 [`Config.cs` 中的注释](Core/Config.cs)。

### 扩展功能

`Chireiden.TShock.Omni.Misc`插件包含多项随机功能：

- 基于权限限制特定Boss召唤、队伍状态和PVP状态
- `.LavaHandler`防止岩浆刷屏（不阻止岩浆生成，但会在可能生成后立即清除）
- 可在其他插件的小游戏中使用`/echo`、`/_pvp`、`/_team`等命令

---

## 指令列表

### 管理命令

| 语法 | 权限 | 说明 |
|------|------|------|
| `/_gc` / `/_gc -f` | `chireiden.omni.admin.gc` | 触发垃圾回收（`-f` 强制完整GC） |
| `/_sv` | `chireiden.omni.admin.sv` | 执行SQLite数据库压缩（VACUUM） |
| `/rbc <消息>` / `/rawbroadcast <消息>` | `chireiden.omni.admin.rawbroadcast` | 发送原始广播消息（无格式） |
| `/listclients` | `chireiden.omni.admin.listclients` | 列出所有连接的客户端信息 |
| `/dumpbuffer <玩家ID> [文件名]` | `chireiden.omni.admin.dumpbuffer` | 导出玩家网络缓冲区数据到文件 |
| `/whereis <命令名>` | `chireiden.omni.admin.whereis` | 查找命令所属插件和程序集 |
| `/kc <玩家ID>` | `chireiden.omni.admin.terminatesocket` | 强制关闭玩家网络连接 |
| `/_ups` / `/_ups bench` | `chireiden.omni.admin.upscheck` | 检查服务器每秒更新次数（`bench` 运行性能测试） |
| `/_csf` | `chireiden.omni.admin.callstackframe` | 显示当前调用堆栈（调试用） |
| `/genconfig` | `chireiden.omni.admin.genconfig` | 生成完整配置文件（显示隐藏选项） |
| `/tileprovider <类型>` | `chireiden.omni.admin.tileprovider` | 切换地图图格提供器（内存优化） |
| `/maxplayers [数量]` | `chireiden.omni.admin.maxplayers` | 查看/设置最大玩家数 |
| `/runas <玩家> <命令> [-f]` | `chireiden.omni.admin.sudo` | 以其他玩家身份执行命令（-f: 跳过权限检查） |
| `/_setperm` | `chireiden.omni.admin.setupperm` | 应用默认权限设置 |
| `/_qbg <命令> [-t]` | `chireiden.omni.admin.runbackground` | 后台执行命令（-t: 使用Task运行） |
| `/_locked <命令>` | `chireiden.omni.admin.locked` | 锁定模式执行命令（防止递归） |
| `/_debugstat` | `chireiden.omni.admin.debugstat` | 输出调试统计信息 |
| `/trytileframe [x] [y]` | `chireiden.omni.admin.trytileframe` | 测试TileFrame计算（可能造成卡顿） |
| `/inspecttileframe` | `chireiden.omni.admin.inspecttileframe` | 启用TileFrame检查（高级调试） |

### 玩家命令

| 语法 | 权限 | 说明 |
|------|------|------|
| `/_pvp [玩家名] <true/false>` | `chireiden.omni.setpvp` / `chireiden.omni.admin.setpvp` | 设置PvP状态（管理员可指定其他玩家） |
| `/_team [玩家名] <队伍ID>` | `chireiden.omni.setteam` / `chireiden.omni.admin.setteam` | 设置队伍（0无，1红，2绿，3蓝，4黄，5粉） |
| `/_chat <消息>` | `chireiden.omni.chat` | 模拟发送游戏内聊天消息 |
| `/ghost [-v / -a / -u]` | `chireiden.omni.ghost` | 切换幽灵状态（-v: 客户端幽灵 -a: 活动状态 -u: 取消） |
| `/setlang [-g / -t] [语言代码]` | `chireiden.omni.setlang` | 设置游戏/TShock语言（-g: 仅游戏 -t: 仅TShock） |
| `/echo <消息>` | `chireiden.omni.echo` | 回显消息 |
| `/whynot [-t / -f / -v]` | `chireiden.omni.whynot` | 查看权限检查历史（-t: 成功 -f: 失败 -v: 详细堆栈） |
| `/_ping` | `chireiden.omni.ping` | 测试玩家延迟 |
| `/resetcharacter [-f] [玩家]` | `chireiden.omni.resetcharacter` | 重置角色数据（需确认，支持通配符） |
| `/exportcharacter [玩家]` | `chireiden.omni.admin.exportcharacter` | 导出角色数据为.plr文件 |

### 延迟命令

| 语法 | 权限 | 说明 |
|------|------|------|
| `/settimeout <命令> <间隔>` | `chireiden.omni.timeout` | 延迟执行命令（单位: 游戏帧） |
| `/setinterval <命令> <间隔>` | `chireiden.omni.interval` | 循环执行命令 |
| `/clearinterval <ID>` | `chireiden.omni.cleartimeout` | 取消延迟/循环命令 |
| `/showdelay` | `chireiden.omni.showtimeout` | 查看待执行命令列表 |

---

## 配置

### 主插件配置

配置文件位置：`tshock/chireiden.omni.json`

主要配置区域：

```json
{
  "ShowConfig": false,
  "LogFirstChance": false,
  "DateTimeFormat": "yyyy-MM-dd HH:mm:ss.fff",
  "PrioritizedPacketHandle": true,
  "PlayerWildcardFormat": ["*all*"],
  "ServerWildcardFormat": ["*server*", "*console*"],
  "HideCommands": ["whynot", "_debugstat", "resetcharacter", "_ping", "echo", "_setperm"],
  "StartupCommands": [],
  "CommandRenames": {},
  "Enhancements": {
    "TrimMemory": true,
    "AlternativeCommandSyntax": true,
    "DefaultLanguageDetect": true,
    "BanPattern": true,
    "TileProvider": "AsIs",
    "Socket": "AnotherAsyncSocketAsFallback"
  },
  "DebugPacket": {
    "In": false,
    "Out": false
  },
  "Soundness": {
    "ProjectileKillMapEditRestriction": true,
    "QuickStackRestriction": true,
    "SignEditRestriction": true
  },
  "Permission": {
    "Log": { "Enabled": true, "LogCount": 50 },
    "Preset": { "Enabled": true }
  },
  "Mitigation": {
    "ChatSpamRestrict": ["1.6/5", "4/20"],
    "ConnectionLimit": ["3/5", "15/60"],
    "ConnectionStateTimeout": { "0": 1.0, "1": 4.0 }
  }
}
```

### Misc 插件配置

配置文件位置：`tshock/chireiden.omni.misc.json`

```json
{
  "Enhancements": {
    "SyncVersion": false
  },
  "LavaHandler": {
    "Enabled": false,
    "AllowHellstone": false
  },
  "Permission": {
    "Restrict": {
      "Enabled": false,
      "ToggleTeam": true,
      "TogglePvP": true,
      "SummonBoss": true
    }
  }
}
```

---

## 构建

### 环境要求

- .NET 9.0 SDK
- TShock 6.1.0 开发依赖

### 构建命令

```bash
dotnet build Plugin.sln -c Release
```

构建产物位置：
- `Core/bin/Release/net9.0/Chireiden.TShock.Omni.dll`
- `Misc/bin/Release/net9.0/Chireiden.TShock.Omni.Misc.dll`

将两个 DLL 复制到 TShock 的 `ServerPlugins` 文件夹即可。

---

## 许可证

遵循上游项目 [sgkoishi/yaaiomni](https://github.com/sgkoishi/yaaiomni) 的许可证。
