# Flow - Windows Phone 8.1 SQLite Setup Guide

This project is a Windows Phone 8.1 (WPA81) application using SQLite for data persistence. Due to the age and specific constraints of the platform, the SQLite setup requires precise configuration to avoid common runtime errors.

## 🛠 Prerequisites

* **Visual Studio:** 2013 or 2015 with Windows Phone 8.1 development tools installed.
* **SDKs:** Windows Phone 8.1 SDK.
* **Device:** A developer-unlocked Windows Phone 8.1 (if deploying to hardware).

---

## 📦 Database Setup (SQLite)

The project uses the **sqlite-net-pcl (v1.1.2)** NuGet package. 

### 1. Critical Dependencies
The native SQLite engine (`esqlite3.dll`) requires the **Microsoft Visual C++ 2013 Runtime** to be present on the device/emulator. 
* This is included in the project via the `<SDKReference Include="Microsoft.VCLibs, Version=12.0">` entry in `Flow.csproj`.

### 2. Manual Native Library Fix (Failsafe)
Standard NuGet package restore often fails to correctly deploy native binaries on Windows Phone 8.1. We took several manual steps to ensure the database engine is found:

1.  **Direct File Copy:** Manually copied `esqlite3.dll` from the NuGet `packages/SQLitePCL.native.sqlite3.v120_wp81.0.9.2` folder into the `Flow` project root.
2.  **Duplicate for Compatibility:** Created a copy named `sqlite3.dll` in the project root to ensure compatibility with different SQLite providers that might look for the standard filename.
3.  **Project Inclusion:** Both DLLs were added to the `Flow.csproj` as "Content" and set to "Copy to Output Directory: Preserve Newest".

### 3. Dynamic Platform Conditional (Current State)
The project is currently configured to automatically link to the correct DLL version (x86 for emulator, ARM for device) using conditional references in the `.csproj`:
```xml
<Content Include="..\packages\SQLitePCL.native.sqlite3.v120_wp81.0.9.2\build\native\sqlite3\v120_wp81\x86\esqlite3.dll" Condition="'$(Platform)' == 'x86'">
  <Link>esqlite3.dll</Link>
</Content>
```

### 4. Application Initialization
SQLite must be initialized at the earliest possible moment to prevent `TypeInitializationException`.
* This is handled in the `App()` constructor in `App.xaml.cs` using:
  ```csharp
  SQLitePCL.Batteries.Init();
  ```

---

## 🚀 Deployment Instructions

### **CRITICAL: SET BUILD ARCHITECTURE**
**SQLite will NOT run on `Any CPU` in Windows Phone 8.1.** 
1. Open the **Configuration Manager** in Visual Studio.
2. Ensure the active platform is **x86** (for the Emulator) or **ARM** (for a real phone).
3. Do not use `Any CPU`.

### To run on the Emulator (x86):
1. Set the **Solution Platform** dropdown in Visual Studio to **x86**.
2. Select an **Emulator** target.
3. Perform a **Rebuild Solution** before launching.

### To run on a Real Phone (ARM):
1. Set the **Solution Platform** dropdown in Visual Studio to **ARM**.
2. Select **Device** as the target.
3. Ensure your phone is plugged in via USB and the screen is **unlocked**.
4. Perform a **Rebuild Solution** before launching.

---

## 🔍 Troubleshooting

### `TypeInitializationException` in `SQLiteConnection`
* **Cause:** Usually occurs if you are building for `Any CPU` or if `Batteries.Init()` was skipped.
* **Fix:** Change the platform to `x86` or `ARM` and ensure `App()` initializes the batteries.

### `DllNotFoundException` (esqlite3.dll)
* **Cause:** The native engine is missing or its dependencies (C++ Runtime) are not satisfied.
* **Fix:** 
    1. Verify the `Microsoft.VCLibs` reference is in the project.
    2. Check that the build architecture matches the target.
    3. Ensure `esqlite3.dll` is present in the final `bin` output folder.

---

## 📂 Git & Binaries
By default, binary files are ignored. However, the manually placed `Flow/esqlite3.dll` and `Flow/sqlite3.dll` are critical for the project to build correctly on a new machine if NuGet restore behaves differently. If the project fails to build on a new machine, re-copy these DLLs from the NuGet packages into the project root as described in Section 2 above.
