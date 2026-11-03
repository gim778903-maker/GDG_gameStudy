# Unity Editor CLI

This project uses the official `unity` CLI and its existing
`com.unity.pipeline` package. Keep the project open in Unity 6000.4.5f1
and wait for compilation to finish before issuing commands.

The project launcher finds `unity` on PATH or in `%LOCALAPPDATA%/Unity/bin`:

```powershell
.\Tools\unity.ps1 --version
.\Tools\unity.ps1 command editor status
.\Tools\unity.ps1 command scene list_open
.\Tools\unity.ps1 command scene get_hierarchy
.\Tools\unity.ps1 command editor.playmode start
.\Tools\unity.ps1 command editor.playmode stop
```

To browse available commands, run `.\Tools\unity.ps1 command`.
With `unity` on PATH, the same commands work directly from this project's directory:

```powershell
unity command editor status
```

For another working directory, pass `--project-path` after `command`.
If PowerShell's script policy blocks the launcher, invoke the installed executable
directly instead:

```powershell
& "$env:LOCALAPPDATA\Unity\bin\unity.exe" command --project-path 'C:\Users\Lenovo\Card\GDGoC_game\GDG_gameStudy' editor status
```

Pipeline automatically discovers the running Editor using
`Library/Pipeline/.unity-pipeline-port`. That file contains a local authentication
token; do not commit or share it. If connection fails, check Unity's Console for
compilation errors and the `Pipeline > Start Server` menu.

Package documentation: `Library/PackageCache/com.unity.pipeline@*/Documentation~/index.md`.
Official package: https://docs.unity3d.com/Packages/com.unity.pipeline@0.9/manual/index.html
