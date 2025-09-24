using DocumentFormat.OpenXml.CustomProperties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

public static class AttributesClass
{
    private static Dictionary<string, FontFamily> fontVariants = new Dictionary<string, FontFamily>(StringComparer.OrdinalIgnoreCase);
    private static bool fontsLoaded = false;

    public static void LoadFonts()
    {
        if (fontsLoaded) return;

        var assembly = Assembly.GetExecutingAssembly();
        string[] resourceNames = assembly.GetManifestResourceNames();

        foreach (string resourceName in resourceNames)
        {
            if (!resourceName.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase))
                continue;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) continue;

                byte[] fontData = new byte[stream.Length];
                stream.Read(fontData, 0, fontData.Length);

                IntPtr fontPtr = Marshal.AllocCoTaskMem(fontData.Length);
                Marshal.Copy(fontData, 0, fontPtr, fontData.Length);

                try
                {
                    var fontCollection = new PrivateFontCollection();
                    fontCollection.AddMemoryFont(fontPtr, fontData.Length);

                    FontFamily family = fontCollection.Families[0];

                    string[] parts = resourceName.Split('.');
                    if (parts.Length < 2) continue;

                    string variantName = parts[parts.Length - 2]; 

                    fontVariants[variantName] = family;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show($"Font load error ({resourceName}):\n{ex.Message}");
                }
                finally
                {
                    Marshal.FreeCoTaskMem(fontPtr);
                }
            }
        }

        fontsLoaded = true;
    }

    public static Font GetFont(string variantName, float size, FontStyle style = FontStyle.Regular)
    {
        LoadFonts();

        if (!fontVariants.TryGetValue(variantName, out var family))
        {
            throw new ArgumentException($"Roboto variant '{variantName}' not found. Available: {string.Join(", ", fontVariants.Keys)}");
        }

        return new Font(family, size, style);
    }

    public static void SetMinSize(Form form, int clientWidth, int clientHeight)
    {
        form.ClientSize = new Size(clientWidth, clientHeight);
        Size fullSize = form.Size;
        form.MinimumSize = fullSize;
    }

    public static void ShowWithOverlay(Form owner, Form dialog)
    {
        Form formBackground = null;

        try
        {
            // Create the background overlay
            formBackground = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                Opacity = 0.4d,
                BackColor = Color.Black,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                TopMost = false,
                Enabled = false
            };

            Rectangle workingArea = owner.Bounds;
            Point screenTopLeft = owner.PointToScreen(Point.Empty);

            formBackground.Location = screenTopLeft;
            formBackground.Size = owner.ClientSize;

            // Show the dimming form behind the dialog
            formBackground.Owner = owner;

            // Update both overlay and dialog when the owner moves or resizes
            void Owner_LocationOrSizeChanged(object s, EventArgs e)
            {
                UpdateOverlayPosition(owner, formBackground);
                CenterDialogOnOwner(owner, dialog);
            }

            // Attach shared handler instead of anonymous lambdas
            owner.LocationChanged += Owner_LocationOrSizeChanged;
            owner.SizeChanged += Owner_LocationOrSizeChanged;

            dialog.FormClosed += (s, e) =>
            {
                formBackground.BeginInvoke(new Action(() =>
                {
                    // Unhook the event handlers before disposing
                    owner.LocationChanged -= Owner_LocationOrSizeChanged;
                    owner.SizeChanged -= Owner_LocationOrSizeChanged;

                    if (formBackground != null && !formBackground.IsDisposed)
                    {
                        formBackground.Close();
                        formBackground.Dispose();
                    }
                }));
            };

            // Configure the dialog
            dialog.Owner = formBackground;
            dialog.TopMost = false;
            dialog.StartPosition = FormStartPosition.Manual;
            dialog.Location = new Point(
                owner.Location.X + (owner.Width - dialog.Width) / 2,
                owner.Location.Y + (owner.Height - dialog.Height) / 2
            );

            formBackground.Show();
            dialog.Show(); 
            dialog.BringToFront();

        }
        catch (Exception ex)
        {
            MessageBox.Show("Error showing modal dialog with overlay: " + ex.Message);
        }
        finally
        {
            //nothing
        }
    }


    private static void UpdateOverlayPosition(Form owner, Form overlay)
    {
        if (owner == null || overlay == null) return;

        Point screenTopLeft = owner.PointToScreen(Point.Empty);
        overlay.Location = screenTopLeft;
        overlay.Size = owner.ClientSize;
    }

    private static void CenterDialogOnOwner(Form owner, Form dialog)
    {
        dialog.Location = new Point(
            owner.Location.X + (owner.Width - dialog.Width) / 2,
            owner.Location.Y + (owner.Height - dialog.Height) / 2
        );
    }
    public static Form GetRealOwnerForm(Form startingForm)
    {
        Form current = startingForm;

        // Walk up the ownership chain to skip dim background forms
        while (current?.Owner != null && current.Owner.Enabled == false)
        {
            current = current.Owner;
        }

        return current ?? startingForm;
    }

    public static void ShowFullCover(Form owner, Form child)
    {
        if (owner == null || child == null) return;

        // Configure child form to behave like a fullscreen overlay on the parent
        child.TopLevel = false;
        child.FormBorderStyle = FormBorderStyle.None;
        child.Dock = DockStyle.Fill;

        // Add child to parent and bring to front
        owner.Controls.Add(child);
        child.BringToFront();
        child.Show();
    }

    public static void TextboxPlaceholder(TextBox textBox, string placeholder) //act as textbox hint
    {
        // Initial setup
        textBox.Text = placeholder;

        // Handle Enter (focus)
        textBox.Enter += (s, e) =>
        {
            if (textBox.Text == placeholder)
            {
                textBox.Text = "";
            }
        };

        // Handle Leave (focus lost)
        textBox.Leave += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = placeholder;
            }
        };
    }

}
