using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HRIS_JAP_ATTPAY
{
    public class EmployeeLeaveSyncService
    {
        private readonly FirebaseClient firebase;

        public EmployeeLeaveSyncService()
        {
            firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        }

        // Start realtime listeners
        public void StartSync()
        {
            // EmployeeDetails changes (key is typically the employeeId: JAP-xxx)
            firebase.Child("EmployeeDetails")
                .AsObservable<object>()
                .Subscribe(async change =>
                {
                    try
                    {
                        string empId = ExtractEmployeeIdFromChange(change.Key, change.Object);
                        Console.WriteLine($"[Listener] EmployeeDetails change. Key: {change.Key}, ResolvedEmpId: {empId}");
                        if (!string.IsNullOrEmpty(empId))
                            await SyncOneEmployeeAsync(empId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Sync Error EmployeeDetails] {ex}");
                    }
                });

            // EmploymentInfo changes (key may be index or auto key; object contains employee_id)
            firebase.Child("EmploymentInfo")
                .AsObservable<object>()
                .Subscribe(async change =>
                {
                    try
                    {
                        string empId = ExtractEmployeeIdFromChange(change.Key, change.Object);
                        Console.WriteLine($"[Listener] EmploymentInfo change. Key: {change.Key}, ResolvedEmpId: {empId}");
                        if (!string.IsNullOrEmpty(empId))
                            await SyncOneEmployeeAsync(empId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Sync Error EmploymentInfo] {ex}");
                    }
                });

            Console.WriteLine("✅ Employee–LeaveCredits realtime sync started.");
        }

        // Extract employee_id robustly from feed change
        private string ExtractEmployeeIdFromChange(string key, object obj)
        {
            try
            {
                // 1) If key itself looks like the employee id (JAP-xxx), use it
                if (!string.IsNullOrEmpty(key) && key.StartsWith("JAP-", StringComparison.OrdinalIgnoreCase))
                    return key;

                // 2) Otherwise, try parse object and read employee_id field
                if (obj != null)
                {
                    var j = obj as JObject ?? JObject.FromObject(obj);
                    var idToken = j["employee_id"] ?? j["employeeId"] ?? j["employeeID"];
                    if (idToken != null)
                        return idToken.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ExtractEmpId Error] key={key} err={ex.Message}");
            }

            return null;
        }

        // Sync one employee => update their matching Leave Credits record (if exists)
        public async Task SyncOneEmployeeAsync(string employeeId)
        {
            try
            {
                Console.WriteLine($"[SyncOne] Starting sync for {employeeId}");

                // Get EmployeeDetails
                var emp = await firebase.Child("EmployeeDetails").Child(employeeId).OnceSingleAsync<JObject>();
                if (emp == null)
                {
                    Console.WriteLine($"[SyncOne] EmployeeDetails not found for {employeeId}");
                    return;
                }

                // Compose full name
                string first = emp["first_name"]?.ToString() ?? "";
                string middle = emp["middle_name"]?.ToString() ?? "";
                string last = emp["last_name"]?.ToString() ?? "";
                string fullName = $"{first} {middle} {last}".Replace("  ", " ").Trim();

                // Get EmploymentInfo (prefer keyed by employeeId, but that may be an array/collection)
                JObject empInfo = null;

                // Try direct child employeeId first
                try
                {
                    empInfo = await firebase.Child("EmploymentInfo").Child(employeeId).OnceSingleAsync<JObject>();
                }
                catch { empInfo = null; }

                // If empInfo null, try scanning EmploymentInfo collection
                if (empInfo == null)
                {
                    var empInfoList = await firebase.Child("EmploymentInfo").OnceAsync<object>();
                    foreach (var item in empInfoList)
                    {
                        if (item.Object == null) continue;
                        var j = item.Object as JObject ?? JObject.FromObject(item.Object);
                        if (j["employee_id"] != null && j["employee_id"].ToString().Equals(employeeId, StringComparison.OrdinalIgnoreCase))
                        {
                            empInfo = j;
                            break;
                        }
                    }
                }

                string department = empInfo?["department"]?.ToString() ?? (emp["department"]?.ToString() ?? "");
                string position = empInfo?["position"]?.ToString() ?? (emp["position"]?.ToString() ?? "");

                // Read entire Leave Credits list (because keys are auto-generated)
                var credits = await firebase.Child("Leave Credits").OnceAsync<object>();

                // Find the Leave Credits record by employee_id field inside each record
                var existing = credits.FirstOrDefault(c =>
                {
                    if (c.Object == null) return false;
                    var j = c.Object as JObject ?? JObject.FromObject(c.Object);
                    var id = j["employee_id"] ?? j["employeeId"];
                    return id != null && id.ToString().Equals(employeeId, StringComparison.OrdinalIgnoreCase);
                });

                if (existing == null)
                {
                    Console.WriteLine($"[SyncOne] No Leave Credits entry found for {employeeId} (will skip update).");
                    // If you want to create a Leave Credits entry automatically if missing, uncomment the block below:
                    /*
                    var newCredit = new
                    {
                        employee_id = employeeId,
                        full_name = fullName,
                        department = department,
                        position = position,
                        sick_leave = 6,
                        vacation_leave = 6,
                        created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    await firebase.Child("Leave Credits").PostAsync(newCredit);
                    Console.WriteLine($"[SyncOne] Created Leave Credits for {employeeId}");
                    */
                    return;
                }

                // Prepare updated object (preserve existing fields and only change the ones we need)
                var existingJ = existing.Object as JObject ?? JObject.FromObject(existing.Object);

                bool changed = false;
                if ((existingJ["full_name"]?.ToString() ?? "") != fullName)
                {
                    existingJ["full_name"] = fullName;
                    changed = true;
                }
                if ((existingJ["department"]?.ToString() ?? "") != department)
                {
                    existingJ["department"] = department;
                    changed = true;
                }
                if ((existingJ["position"]?.ToString() ?? "") != position)
                {
                    existingJ["position"] = position;
                    changed = true;
                }

                if (changed)
                {
                    // Update using the actual key (existing.Key)
                    await firebase.Child("Leave Credits").Child(existing.Key).PutAsync(existingJ);
                    Console.WriteLine($"[SyncOne] Updated Leave Credits ({existing.Key}) for {employeeId} -> {fullName}");
                }
                else
                {
                    Console.WriteLine($"[SyncOne] No changes detected for {employeeId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SyncOne Error] {employeeId}: {ex}");
            }
        }

        // One-time full scan to sync all Leave Credits now (useful to immediately bring DB in sync)
        public async Task FullSyncAllAsync()
        {
            try
            {
                Console.WriteLine("[FullSync] Starting full sync of Leave Credits from EmployeeDetails/EmploymentInfo...");

                var empList = await firebase.Child("EmployeeDetails").OnceAsync<object>();
                foreach (var empItem in empList)
                {
                    string empId = null;
                    try
                    {
                        // empItem.Key is usually the employeeId when EmployeeDetails keyed by JAP-xxx
                        if (!string.IsNullOrEmpty(empItem.Key) && empItem.Key.StartsWith("JAP-", StringComparison.OrdinalIgnoreCase))
                            empId = empItem.Key;
                        else
                        {
                            var j = empItem.Object as JObject ?? JObject.FromObject(empItem.Object);
                            empId = j["employee_id"]?.ToString();
                        }
                    }
                    catch { empId = null; }

                    if (!string.IsNullOrEmpty(empId))
                    {
                        await SyncOneEmployeeAsync(empId);
                        // small delay to avoid hammering (optional)
                        await Task.Delay(50);
                    }
                }

                Console.WriteLine("[FullSync] Full sync complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FullSync Error] {ex}");
            }
        }
    }
}
