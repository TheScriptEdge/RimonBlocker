using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

class Program
{
    private List<string> processPaths;
    private bool pathsSet = false;
    private TextBox consoleTextBox;
    private Form programForm;

    private delegate void AppendTextDelegate(string text);

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        Program program = new Program();
        program.Initialize();
        Application.Run();
    }

    public void Initialize()
    {
        NotifyIcon notifyIcon = new NotifyIcon();
        notifyIcon.Icon = new System.Drawing.Icon("Visuals\\ic_block_128_28186 .ico");
        notifyIcon.Visible = true;
        notifyIcon.Text = "RimonBlocked";

        ContextMenuStrip contextMenu = CreateStyledContextMenu();
        notifyIcon.ContextMenuStrip = contextMenu;

        Thread backgroundThread = new Thread(() =>
        {
            while (true)
            {
                if (pathsSet)
                {
                    foreach (var path in processPaths)
                    {
                        BlockProcess(path);
                    }
                }

                Thread.Sleep(1000);
            }
        });
        backgroundThread.IsBackground = true;
        backgroundThread.Start();
    }

    private ContextMenuStrip CreateStyledContextMenu()
    {
        ContextMenuStrip contextMenu = new ContextMenuStrip();

        // Create a custom renderer for the context menu
        contextMenu.Renderer = new CustomRenderer();

        // Add menu items
        contextMenu.Items.Add(CreateStyledMenuItem("Custom Paths", (sender, e) => SetCustomPaths()));
        contextMenu.Items.Add(CreateStyledMenuItem("Built-in Paths", (sender, e) => SetBuiltInPaths()));
        contextMenu.Items.Add(CreateStyledMenuItem("Restart Processes", (sender, e) => RestartProcesses()));
        contextMenu.Items.Add(CreateStyledMenuItem("Open Log", (sender, e) => OpenLog()));
        contextMenu.Items.Add(CreateStyledMenuItem("Close Log", (sender, e) => CloseLog()));

        // Add a styled credit popup
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(CreateStyledMenuItem("Credit", (sender, e) => ShowCreditPopup()));
        contextMenu.Items.Add(CreateStyledMenuItem("Exit", (sender, e) => Application.Exit()));

        return contextMenu;
    }

    private ToolStripMenuItem CreateStyledMenuItem(string text, EventHandler onClick)
    {
        ToolStripMenuItem menuItem = new ToolStripMenuItem(text);
        menuItem.Font = new Font("Segoe UI", 12, FontStyle.Regular);
        menuItem.ForeColor = Color.White;
        menuItem.Click += onClick;
        return menuItem;
    }

    private void ShowCreditPopup()
    {
        MessageBox.Show("Developed by TheScriptEdge (GitHub)", "Credit", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // Custom renderer class for the context menu
    public class CustomRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Enabled)
            {
                Rectangle rc = new Rectangle(Point.Empty, e.Item.Size);
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(66, 53, 147)), rc); // Use color #42359A
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(66, 53, 147)), 1, 0, rc.Width - 2, rc.Height - 1);
            }
            else
            {
                base.OnRenderMenuItemBackground(e);
            }
        }
    }

    private void SetCustomPaths()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Text Files|*.txt";
        openFileDialog.Title = "Select a Text File with Paths";

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            string fileContent = File.ReadAllText(openFileDialog.FileName);
            processPaths = new List<string>(fileContent.Split(','));

            pathsSet = true;
        }
    }

    private void SetBuiltInPaths()
    {
        processPaths = new List<string>
        {
            "C:\\Program Files\\Netspark\\NsUpdate\\NsUpdate.exe",
            "C:\\Program Files\\Netspark\\NsUpdate\\NsUpdateTask.exe",
            "C:\\Program Files\\Netspark\\NsGUI\\NsGUI.exe",
            "C:\\Users\\AppData\\Local\\Netspark\\ScreenFilter\\ScreenFilter.exe",
            "C:\\Program Files\\Netspark\\NsUpdate\\net_c.exe",
            "C:\\Program Files\\Netspark\\NsUpdate\\sigcheck.exe",
            "C:\\Program Files\\Netspark\\NsUpdate\\signtool.exe",
            "C:\\Program Files\\Netspark\\NsUpdate\\Uninstall Rimon.exe",
            "C:\\Program Files\\Netspark\\sigcheck.exe",
            "C:\\Program Files\\Netspark\\signtool.exe",
            "C:\\Program Files\\Netspark\\NsGUI\\graphics\\rimon\\floatingIcon.exe"
        };
        pathsSet = true;

        // If the form is already created, show it immediately
        OpenLog();
    }

    private void RestartProcesses()
    {
        processPaths = new List<string>
        {
            "C:\\Program Files\\Netspark\\NsUpdate\\NsUpdate.exe",
            "C:\\Program Files\\Netspark\\NsUpdate\\NsUpdateTask.exe",
            "C:\\Program Files\\Netspark\\NsGUI\\NsGUI.exe",
            "C:\\Users\\AppData\\Local\\Netspark\\ScreenFilter\\ScreenFilter.exe",
            "C:\\Program Files\\Netspark\\NsUpdate\\net_c.exe",
            "C:\\Program Files\\Netspark\\NsGUI\\graphics\\rimon\\floatingIcon.exe"
        };
        OpenLog();
        if (processPaths != null)
        {
            foreach (var path in processPaths)
            {
                StartProcess(path);
                Application.Exit();
            }
        }
    }

    private void OpenLog()
    {
        if (programForm == null)
        {
            programForm = new Form();
            programForm.Size = new System.Drawing.Size(400, 300);

            consoleTextBox = new TextBox();
            consoleTextBox.Multiline = true;
            consoleTextBox.ScrollBars = ScrollBars.Vertical;
            consoleTextBox.Dock = DockStyle.Fill;

            Button closeLogButton = new Button();
            closeLogButton.Text = "Close Log";
            closeLogButton.Click += (sender, e) => CloseLog();

            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.RowCount = 2;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel.Controls.Add(consoleTextBox, 0, 0);
            tableLayoutPanel.Controls.Add(closeLogButton, 0, 1);

            programForm.Controls.Add(tableLayoutPanel);
        }

        programForm.Show();
    }

    private void CloseLog()
    {
        if (programForm != null)
        {
            programForm.Hide();
        }
    }

    private void StartProcess(string processPath)
    {
        try
        {

            Process.Start(processPath);
            AppendText($"Restarted process - Path: {processPath}");
        }
        catch (Exception ex)
        {
            LogError($"Error restarting process - Path: {processPath}", ex);
        }
    }

    private void BlockProcess(string processPath)
    {
        try
        {
            Process[] processes = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(processPath));

            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                    AppendText($"Blocked process with ID {process.Id} - Path: {processPath}");
                }
                catch (Exception ex)
                {
                    LogError($"Error blocking process ID {process.Id} - Path: {processPath}", ex);
                }
            }
        }
        catch (Exception ex)
        {
            LogError($"Error getting processes for path: {processPath}", ex);
        }
    }

    private void AppendText(string text)
    {
        if (consoleTextBox != null && consoleTextBox.InvokeRequired)
        {
            // Invoke method on the UI thread
            consoleTextBox.Invoke(new AppendTextDelegate(AppendText), text);
        }
        else
        {
            consoleTextBox.AppendText(text + Environment.NewLine);
        }
    }

    private void LogError(string message, Exception ex)
    {
        AppendText($"Error: {message}\nException: {ex.Message}");
        // You can expand this to log to a file, database, or use a logging library.
    }
}
