# TB Studio Tools

[![Unity](https://img.shields.io/badge/Unity-6000.0%2B-blue.svg)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![openupm](https://img.shields.io/npm/v/com.tbstudio.tools?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.tbstudio.tools/)
[![openupm](https://img.shields.io/badge/dynamic/json?color=brightgreen&label=downloads&query=%24.downloads&suffix=%2Fmonth&url=https%3A%2F%2Fpackage.openupm.com%2Fdownloads%2Fpoint%2Flast-month%2Fcom.tbstudio.tools)](https://openupm.com/packages/com.tbstudio.tools/)

A tool with editor tools and ready to use modular systems.

> 📖 **Looking for full guides and details? Check out the [Official GitHub Wiki](https://github.com/TBStudio-TB/TBStudio.Unity-Tools/wiki).**

---

## ✨ Key Features

- **Editor Task List:** Keep track of task in the unity editor.
- **Project Builder:** Build you project as seperated apps testing and public release (2 versions on 1 device).
- **Performance-First:** Designed with low overhead and zero GC allocations in hot loops.

---

## ⚡ Quick Start

### Installation

#### Option 1: Unity Package Manager via OpenUPM (Recommended)

1. Open **Edit > Project Settings > Package Manager**
2. add a new scope Registry (or edit the existing OpenUPM entry):
   - Name: package.openupm.com
   - URL: https://package.openupm.com
   - Scope(s): com.tbstudio.be
3. Click **Save** or **Apply**
4. Open **Window > Package Manager**
5. Click **+**
6. Select **Add package by name...** or **Add package from git URL..**
7. Paste **com.tbstudio.tools** into name
8. Paste The version you want into version (empty give the latest version)
9. click **Add**

#### Option 2: Unity Package Manager via Git URL

1. Open your project in Unity.
2. Go to **Window** > **Package Manager**.
3. Click the **`+`** icon in the top left corner and select **Add package from git URL...**
4. Paste the repository URL:
   ```text
   https://github.com/TBStudio-TB/TBStudio.Unity-Tools.git
   ```
