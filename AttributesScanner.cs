using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ThreadingTimer = System.Threading.Timer;

public static class AttributesScanner
{
    private const string targetModelId = @"HID\VID_FFFF&PID_0035&MI_01";

    // Hook
    private static IntPtr hookId = IntPtr.Zero;
    private static LowLevelKeyboardProc hookCallback = HookCallback;
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;

    // Buffers and timing
    private static readonly object bufferLock = new object();
    private static readonly List<int> vkBuffer = new List<int>();
    private static Stopwatch stopwatch = new Stopwatch();
    private static long lastKeyTime = 0;

    private static bool inScannerBurst = false;
    private static ThreadingTimer resetTimer;

    // Thresholds
    private const int scannerThresholdMs = 15;   // avg inter-key time
    private const int finalizeTimeoutMs = 35;    // idle reset

    // Event: apps subscribe here
    public static event EventHandler<string> OnScannerInput;

    // -------------------
    // Public API
    // -------------------

    public static bool IsScannerConnected()
    {
        return GetConnectedScanners().Count > 0;
    }

    public static void StartScannerMonitor()
    {
        if (hookId == IntPtr.Zero)
        {
            hookId = SetHook(hookCallback);
            stopwatch.Restart();
            lastKeyTime = 0;
        }
    }

    public static void StopScannerMonitor()
    {
        if (hookId != IntPtr.Zero)
        {
            try { UnhookWindowsHookEx(hookId); } catch { }
            hookId = IntPtr.Zero;
        }

        lock (bufferLock)
        {
            vkBuffer.Clear();
            inScannerBurst = false;
            lastKeyTime = 0;
        }
    }

    // -------------------
    // Private helpers
    // -------------------

    private static List<string> GetConnectedScanners()
    {
        var devices = new List<string>();

        try
        {
            using (var searcher = new ManagementObjectSearcher(
                "SELECT * FROM Win32_PnPEntity WHERE DeviceID LIKE 'HID%'"))
            {
                foreach (var device in searcher.Get())
                {
                    string deviceId = device["DeviceID"] != null ? device["DeviceID"].ToString() : string.Empty;
                    if (deviceId.StartsWith(targetModelId, StringComparison.OrdinalIgnoreCase))
                    {
                        devices.Add(deviceId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error detecting scanners: " + ex.Message);
        }

        return devices;
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vk = Marshal.ReadInt32(lParam);

            lock (bufferLock)
            {
                long now = stopwatch.ElapsedMilliseconds;
                if (!stopwatch.IsRunning) stopwatch.Start();

                long delta = (lastKeyTime > 0) ? now - lastKeyTime : 999;
                lastKeyTime = now;

                // reset timer on each key
                resetTimer?.Dispose();
                resetTimer = new ThreadingTimer(_ =>
                {
                    lock (bufferLock)
                    {
                        if (vkBuffer.Count > 0 && inScannerBurst)
                        {
                            // finalize scanner input
                            string scanned = ConvertVkSequenceToString(vkBuffer);
                            if (scanned.EndsWith("\n")) scanned = scanned.TrimEnd('\n');
                            Debug.WriteLine("[Scanner Input] " + scanned);
                            try { OnScannerInput?.Invoke(null, scanned); } catch { }
                        }
                        vkBuffer.Clear();
                        inScannerBurst = false;
                    }
                }, null, finalizeTimeoutMs, Timeout.Infinite);

                // detect if this is scanner speed
                if (delta < scannerThresholdMs)
                    inScannerBurst = true;

                vkBuffer.Add(vk);

                // swallow if scanner burst, otherwise let through
                if (inScannerBurst)
                    return (IntPtr)1;
            }
        }

        return CallNextHookEx(hookId, nCode, wParam, lParam);
    }

    private static string ConvertVkSequenceToString(List<int> vks)
    {
        var sb = new StringBuilder();
        foreach (int vk in vks)
        {
            if (vk >= 0x30 && vk <= 0x39) sb.Append((char)vk);
            else if (vk >= 0x41 && vk <= 0x5A) sb.Append((char)vk);
            else if (vk == (int)Keys.Space) sb.Append(' ');
            else if (vk == (int)Keys.Enter) sb.Append('\n');
            else sb.Append('?');
        }
        return sb.ToString();
    }

    #region WinAPI
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
    #endregion
}
