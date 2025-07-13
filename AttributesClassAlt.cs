using System.Windows.Forms;

public class AttributesClassAlt
{
    private Panel targetPanel;

    // Constructor: binds this helper to a specific panel
    public AttributesClassAlt(Panel panel)
    {
        targetPanel = panel;
    }

    // Public method to load a UserControl into the panel
    public void LoadUserControl(UserControl control)
    {
        // Avoid reloading the same control type
        if (targetPanel.Controls.Count > 0 &&
            targetPanel.Controls[0].GetType() == control.GetType())
            return;

        targetPanel.Controls.Clear();
        control.Dock = DockStyle.Fill;
        targetPanel.Controls.Add(control);
    }

}
