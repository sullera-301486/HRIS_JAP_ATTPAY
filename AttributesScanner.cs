using System;
using System.Collections.Generic;
using System.Management; 

public static class AttributesScanner
{
    private const string targetModelId = @"HID\VID_FFFF&PID_0035&MI_01";

    public static bool IsScannerConnected()
    {
        return GetConnectedScanners().Count > 0;
    }

    public static void CheckScannerStatus()
    {
        if (IsScannerConnected())
        {
            Console.WriteLine("Scanner is connected.");
        }
        else
        {
            Console.WriteLine("No scanner detected.");
        }
    }

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
                    string deviceId = device["DeviceID"]?.ToString() ?? string.Empty;

                    // Match against the model prefix only
                    if (deviceId.StartsWith(targetModelId, StringComparison.OrdinalIgnoreCase))
                    {
                        devices.Add(deviceId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error detecting scanners: " + ex.Message);
        }

        return devices;
    }
}
