# NITRO OPTIMISER 🚀⚡

**The ultimate AI-Powered Windows performance optimization tool.**  
Experience Ultra-Smooth PC performance, professional aesthetics, real-time system monitoring, and complete control over your Windows ecosystem.

![Nitro Optimiser](Assets/icon.png)

🚀 **Latest Release**: v1.0.1

## ✨ Key Features

### 🖥️ Dashboard & Monitoring
- **Live System Health**: Quick overview of your PC's status.
- **Dynamic Drive Tracking**: View real-time capacity and usage for all connected drives (C:, D:, E:, etc.).
- **Real-Time Temperatures**: Live ACPI CPU and GPU temperature query to monitor thermal levels.
- **Floating Stats Widget**: Drag-and-drop topmost borderless overlay showing live CPU, GPU, RAM, and network speeds.

### ⚙️ Performance Tuning
- **Power Plans**: Switch instantly between Balanced, High Performance, and Ultimate Performance modes.
- **Smart Optimize**: One-click AI-driven optimization that cleans RAM and optimizes power settings in seconds.
- **One-Click Revert**: Instantly undo all tweaks and restore Windows default settings.

### 🧹 Deep System Cleaning
- **Temp Files**: Wipes deep-level Windows and user temporary files.
- **DirectX Shader Cache**: Flushes out old DirectX caches.
- **NVIDIA & AMD Cache**: Specific cache wiping for NV_Cache, GLCache, DXCache, and DxCache.
- **RAM Cleaner**: Empties working sets and clears the hidden Standby Memory List for massive RAM recovery.
- **Windows Update Cache**: Clears old SoftwareDistribution downloads.

### 🎮 Gaming Optimization
- **Auto-Boost (Smart Gaming Booster)**: Background thread monitors process launches, elevating game priority to High, setting Ultimate power mode, and clearing standby list on game start, returning to default on close.
- **Mouse Raw Input Fix**: Applies the MarkC registry fix for 1:1 hardware pixel-perfect mouse precision.
- **Stretched Resolution Utility**: Toggle common stretched screen resolutions (e.g. 1720x1080) directly using physical Win32 Display Settings API.
- **Tuning Profiles**: Customized presets for PUBG Mobile, Valorant/CS2, and General Gaming.
- **Xbox Game Bar**: Disable background recording to prevent micro-stutters.
- **Network Throttling**: Remove Windows network throttling index to stabilize ping in multiplayer games.
- **GPU Scheduling**: Hardware-Accelerated GPU Scheduling toggle.

### 🌐 Network Stabilizer
- **Smart DNS Benchmarker**: Ping Google, Cloudflare, Quad9, and AdGuard DNS, applying the fastest latency config instantly.
- **Advanced TCP/IP Tweaks**: Netsh optimizations for TCP Auto-Tuning, ECN Capability, and Chimney Offload.
- **Flush DNS**: Clear the DNS cache instantly.
- **Network Reset**: Deep TCP/IP and Winsock reset to fix stubborn connection issues.

### 🗑️ Windows Debloater
- **App Uninstaller**: Easily remove pre-installed Windows bloatware (Cortana, Xbox speech, Mixed Reality, etc.).
- **Disable Telemetry**: Disables telemetry services, tracking, and diagnostics background processes.

### 🛠️ System Repair
- **SFC /Scannow**: Scan and repair corrupted Windows system files.
- **DISM RestoreHealth**: Repair the Windows image using Windows Update.
- **Rebuild Icon Cache**: Fix blank or corrupted desktop/system icons instantly.
- **Context Menu Cleaner**: Instantly disable heavy Right-Click shell extensions in the registry.

### 🔌 Drivers Manager
- **Drivers Backup**: Exports all third-party Windows drivers (graphics, audio, network) to `C:\DriversBackup` using `dism`.
- **Drivers Restore**: Re-installs drivers from backups using `pnputil`.

### 🚀 Startup Manager
- **Enable / Disable Apps**: Take control of your boot time. Disable or enable startup applications by securely managing the registry without deleting entries completely.

### 📊 Process Manager
- **Live Task Manager**: View currently running processes, their exact path, and RAM usage in MB.
- **Kill Process**: Force close unresponsive or heavy background tasks.

### 🖨️ Reports
- **Shareable Specs Card**: Generates a stylized cyberpunk PC specifications image and saves it to Desktop (`NitroOptimizer_Specs.png`).
- **Detailed System Report**: Generate and view a comprehensive text report of your CPU, Motherboard, RAM (Speed & Capacity), GPU, and Disks directly inside the app, with an option to save to Desktop.

### 🔔 System Tray & Auto-Cleaner
- **System Tray Integration**: Run the app in the background with double-click window restoring.
- **Smart Auto-Cleaner**: Background RAM monitor that automatically flushes memory caches when RAM usage exceeds 80%, triggering balloon toast notifications.

---

## 📥 Download & Install

**Step 1 — Download**  
⬇️ [Download NITRO OPTIMISER v1.0.1 (Portable EXE)](https://github.com/Nitrodz00/NITRO-OPTIMISER/releases/download/v1.0.1/NITRO_OPTIMISER_v1.0.1.exe)  
⬇️ [Download NITRO OPTIMISER v1.0.1 (ZIP Package with Assets)](https://github.com/Nitrodz00/NITRO-OPTIMISER/releases/download/v1.0.1/NITRO_OPTIMISER_v1.0.1.zip)

**Step 2 — Install**  
- Right-click the executable → **Run as Administrator** (Required for deep registry tweaks, RAM clearing, and driver export/restore).
- Follow the setup wizard or run the portable EXE.
- Enjoy an instantly smoother Windows experience!

---

## 🎨 Cyberpunk Cosmic UI
- **Futuristic Theme**: Dark cosmic gradients with neon Magenta and Cyan accents.
- **Responsive Design**: Auto-adjusting UI elements, scaling text, and smooth WPF drop shadows.
- **System Tray Support**: Fully integrated!

---

## 🛠️ Build from Source

This project is built using **C# .NET 8 (WPF) + CommunityToolkit.Mvvm**.

1. Install Visual Studio 2022 with the .NET desktop development workload.
2. Clone the repository.
3. Build the project:
   ```ps1
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
   ```

## 📋 Requirements
- **Windows 10 / Windows 11** (64-bit)
- **Run as Administrator** (Required for advanced WMI and Registry access)

---

📬 **Contact**  
Discord: https://discord.gg/6QpCWeAbyC  
GitHub: Nitrodz00  
Repository: NITRO-TOOLS
