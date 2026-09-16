using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ExportRgbColors
{
    /// <summary>
    /// Simple CONNECT-style export dialog. Color table (all codes) is on by
    /// default; ByLevel / layer rows are optional.
    /// </summary>
    internal sealed class ExportForm : Form
    {
        private readonly TextBox _pathBox;
        private readonly CheckBox _tableBox;
        private readonly CheckBox _levelsBox;

        public string CsvPath
        {
            get { return _pathBox.Text.Trim(); }
        }

        public bool IncludeColorTable
        {
            get { return _tableBox.Checked; }
        }

        public bool IncludeLevels
        {
            get { return _levelsBox.Checked; }
        }

        public ExportForm(string suggestedPath)
        {
            Text = "Export RGB Colors";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(560, 196);
            Font = new Font("Segoe UI", 9F);

            var pathLabel = new Label
            {
                AutoSize = true,
                Text = "CSV file:",
                Location = new Point(12, 18)
            };

            _pathBox = new TextBox
            {
                Location = new Point(80, 14),
                Size = new Size(372, 23),
                Text = suggestedPath ?? string.Empty
            };

            var browse = new Button
            {
                Text = "Browse…",
                Location = new Point(458, 13),
                Size = new Size(86, 25)
            };
            browse.Click += OnBrowse;

            _tableBox = new CheckBox
            {
                AutoSize = true,
                Checked = true,
                Text = "All color codes (table 0–255 → RGB, including unused)",
                Location = new Point(80, 52)
            };

            _levelsBox = new CheckBox
            {
                AutoSize = true,
                Checked = false,
                Text = "Also append ByLevel rows (ColorIndex −1 + layer name)",
                Location = new Point(80, 78)
            };

            var hint = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(460, 0),
                ForeColor = Color.DimGray,
                Text = "Exports every attached color-table index with RGB, not only colors assigned to a layer.",
                Location = new Point(80, 104)
            };

            var export = new Button
            {
                Text = "Export",
                DialogResult = DialogResult.OK,
                Location = new Point(378, 156),
                Size = new Size(86, 27)
            };
            export.Click += OnExportClick;

            var cancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(470, 156),
                Size = new Size(74, 27)
            };

            AcceptButton = export;
            CancelButton = cancel;

            Controls.AddRange(new Control[]
            {
                pathLabel, _pathBox, browse, _tableBox, _levelsBox, hint, export, cancel
            });
        }

        private void OnBrowse(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Export MicroStation colors";
                dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                dialog.FileName = Path.GetFileName(CsvPath);
                string folder = Path.GetDirectoryName(CsvPath);
                if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
                    dialog.InitialDirectory = folder;
                dialog.OverwritePrompt = true;
                dialog.AddExtension = true;
                dialog.DefaultExt = "csv";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    _pathBox.Text = dialog.FileName;
            }
        }

        private void OnExportClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CsvPath))
            {
                MessageBox.Show(this, "Choose a CSV file path.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            if (!IncludeColorTable && !IncludeLevels)
            {
                MessageBox.Show(this, "Select the color table (all color codes) and/or level colors.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
            }
        }
    }
}
