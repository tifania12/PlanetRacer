Add-Type @"
using System;
using System.Runtime.InteropServices;
public class IdleN {
  [StructLayout(LayoutKind.Sequential)]
  public struct LASTINPUTINFO { public uint cbSize; public uint dwTime; }
  [DllImport("user32.dll")] public static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
  [DllImport("kernel32.dll")] public static extern uint GetTickCount();
  public static uint Idle() {
    LASTINPUTINFO l = new LASTINPUTINFO();
    l.cbSize = (uint)Marshal.SizeOf(l);
    GetLastInputInfo(ref l);
    return (GetTickCount() - l.dwTime) / 1000;
  }
}
"@
Write-Output ("IDLE=" + [IdleN]::Idle())
Write-Output ("NOW=" + (Get-Date -Format "yyyy-MM-dd HH:mm:ss"))
$chat = Get-Process -Name "ChatGPT" -ErrorAction SilentlyContinue
if ($chat) { Write-Output "CHATGPT=RUNNING" } else { Write-Output "CHATGPT=NONE" }
