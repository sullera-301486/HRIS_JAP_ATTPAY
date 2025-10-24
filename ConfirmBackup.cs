using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace HRIS_JAP_ATTPAY
{
    public partial class ConfirmBackup : Form
    {
        private readonly string _firebaseUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/";

        public ConfirmBackup()
        {
            InitializeComponent();
            setFont();
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void buttonConfirm_Click(object sender, EventArgs e)
        {
            
        }

        private async Task CreateFullDatabaseBackupAsync()
        {
            using (var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) })
            {
                try
                {
                    // Step 1: Fetch data from Firebase
                    string url = $"{_firebaseUrl.TrimEnd('/')}/.json";

                    HttpResponseMessage response = await httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Failed to fetch data from Firebase.\nStatus: {response.StatusCode}",
                            "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    labelMessage.Text = "⏬ Downloading data from Firebase...";
                    labelMessage.Refresh();

                    string jsonData = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrWhiteSpace(jsonData) || jsonData == "null")
                    {
                        MessageBox.Show("No data found in Firebase database.",
                            "Empty Database", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Step 2: Prepare backup folder
                    labelMessage.Text = "💾 Preparing backup file...";
                    labelMessage.Refresh();

                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string backupFolder = Path.Combine(desktopPath, "JAPHRIS_Backups");

                    if (!Directory.Exists(backupFolder))
                        Directory.CreateDirectory(backupFolder);

                    string fileName = $"JAPHRIS BackUp file_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                    string filePath = Path.Combine(backupFolder, fileName);

                    // Step 3: Save to file (using Task.Run for async I/O)
                    labelMessage.Text = "✍️ Writing backup file...";
                    labelMessage.Refresh();

                    await Task.Run(() => File.WriteAllText(filePath, jsonData));

                    // Step 4: Get file info
                    FileInfo info = new FileInfo(filePath);
                    double fileSizeKB = info.Length / 1024.0;
                    double fileSizeMB = fileSizeKB / 1024.0;

                    // Step 5: Parse and analyze backup data
                    int collectionCount = 0;
                    string collectionSummary = "";

                    try
                    {
                        var data = JObject.Parse(jsonData);
                        collectionCount = data.Count;

                        foreach (var collection in data.Properties().Take(5))
                        {
                            var count = collection.Value is JObject obj ? obj.Count :
                                       collection.Value is JArray arr ? arr.Count : 1;
                            collectionSummary += $"\n   ✓ {collection.Name}: {count} records";
                        }

                        if (data.Count > 5)
                            collectionSummary += $"\n   ... and {data.Count - 5} more collections";
                    }
                    catch
                    {
                        collectionSummary = "\n   (Structure analysis unavailable)";
                    }

                    // Step 6: Show success message
                    string successMessage = $"✅ Backup completed successfully!\n\n" +
                        $"📁 Location: {backupFolder}\n" +
                        $"📄 File: {fileName}\n" +
                        $"💾 Size: {fileSizeKB:F2} KB ({fileSizeMB:F2} MB)\n" +
                        $"📊 Collections: {collectionCount}" +
                        collectionSummary;

                    MessageBox.Show(successMessage, "Backup Successful",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Step 7: Ask to open folder
                    DialogResult result = MessageBox.Show(
                        "Would you like to open the backup folder?",
                        "Open Folder",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start("explorer.exe", backupFolder);
                    }

                    this.Close();
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show($"Network error: {ex.Message}\n\nPlease check your internet connection.",
                        "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show($"Permission denied: {ex.Message}\n\nPlease check folder permissions.",
                        "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"File error: {ex.Message}\n\nPlease ensure the backup folder is accessible.",
                        "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during backup:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                        "Backup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // Always re-enable buttons
                    buttonConfirm.Enabled = true;
                    buttonCancel.Enabled = true;
                    XpictureBox.Enabled = true;
                    this.Cursor = Cursors.Default;
                }
            }
        }

        private void setFont()
        {
            try
            {
                labelMessage.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelRequestConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private async void buttonConfirm_Click_1(object sender, EventArgs e)
        {
            // Disable buttons during backup
            buttonConfirm.Enabled = false;
            buttonCancel.Enabled = false;
            XpictureBox.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            try
            {
                labelMessage.Text = "📡 Connecting to Firebase and downloading data...";
                labelMessage.Refresh();

                await CreateFullDatabaseBackupAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Backup failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Re-enable buttons on error
                buttonConfirm.Enabled = true;
                buttonCancel.Enabled = true;
                XpictureBox.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }
    }
}