using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ExportRgbColors
{
    /// <summary>
    /// Add-in export dialog. Preview shows ColorIndex and RGB together, matching
    /// the MicroStation Color / Index dialog.
    /// </summary>
    internal sealed class ExportForm : Form
    {
        private readonly TextBox _pathBox;
        private readonly CheckBox _tableBox;
        private readonly CheckBox _levelsBox;
        private readonly DataGridView _grid;
        private readonly Label _status;

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
            Text = "Export Color Index and RGB";
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(720, 480);
            Size = new Size(780, 560);
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
                Size = new Size(560, 23),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Text = suggestedPath ?? string.Empty
            };

            var browse = new Button
            {
                Text = "Browse…",
                Location = new Point(652, 13),
                Size = new Size(96, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            browse.Click += OnBrowse;

            _tableBox = new CheckBox
            {
                AutoSize = true,
                Checked = true,
                Text = "All color codes (ColorIndex 0–255 and RGB together)",
                Location = new Point(80, 48)
            };
            _tableBox.CheckedChanged += (s, e) => RefreshPreview();

            _levelsBox = new CheckBox
            {
                AutoSize = true,
                Checked = false,
                Text = "Also append ByLevel rows (ColorIndex −1 + layer name + RGB)",
                Location = new Point(80, 72)
            };
            _levelsBox.CheckedChanged += (s, e) => RefreshPreview();

            var hint = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(660, 0),
                ForeColor = Color.DimGray,
                Text = "Each row has ColorIndex and RGB at the same time, as in the Color dialog.",
                Location = new Point(80, 96)
            };

            _grid = new DataGridView
            {
                Location = new Point(12, 124),
                Size = new Size(736, 340),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            _grid.CellFormatting += OnGridCellFormatting;
            BuildGridColumns();

            _status = new Label
            {
                AutoSize = true,
                Location = new Point(12, 476),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Text = "Loading color table…"
            };

            var export = new Button
            {
                Text = "Export",
                DialogResult = DialogResult.OK,
                Location = new Point(572, 472),
                Size = new Size(86, 27),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            export.Click += OnExportClick;

            var cancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(664, 472),
                Size = new Size(84, 27),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            AcceptButton = export;
            CancelButton = cancel;

            Controls.AddRange(new Control[]
            {
                pathLabel, _pathBox, browse, _tableBox, _levelsBox, hint, _grid, _status, export, cancel
            });

            Shown += (s, e) => RefreshPreview();
        }

        private void BuildGridColumns()
        {
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColorIndex",
                HeaderText = "ColorIndex",
                FillWeight = 18
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Swatch",
                HeaderText = "",
                FillWeight = 8
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RGB",
                HeaderText = "RGB",
                FillWeight = 28
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "R",
                HeaderText = "R",
                FillWeight = 10
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "G",
                HeaderText = "G",
                FillWeight = 10
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "B",
                HeaderText = "B",
                FillWeight = 10
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Layer",
                HeaderText = "Layer",
                FillWeight = 16
            });
        }

        private void RefreshPreview()
        {
            _grid.Rows.Clear();
            try
            {
                ColorExportData data = ColorCsvExporter.Collect(IncludeColorTable, IncludeLevels);
                foreach (ColorCsvRow row in data.Rows)
                {
                    int index = _grid.Rows.Add(
                        row.ColorIndex,
                        string.Empty,
                        row.RGB,
                        row.R,
                        row.G,
                        row.B,
                        row.Layer ?? string.Empty);
                    _grid.Rows[index].Tag = row;
                }

                _status.Text = string.Format(
                    "{0} rows — ColorIndex and RGB on every row ({1} color codes, {2} level)",
                    data.Rows.Count,
                    data.ColorTableRows,
                    data.LevelRows);
            }
            catch (Exception ex)
            {
                _status.Text = "Could not read the color table: " + ex.Message;
            }
        }

        private void OnGridCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "Swatch")
                return;

            var row = _grid.Rows[e.RowIndex].Tag as ColorCsvRow;
            if (row == null)
                return;

            Color color = Color.FromArgb(row.R, row.G, row.B);
            e.CellStyle.BackColor = color;
            e.CellStyle.SelectionBackColor = color;
            e.Value = string.Empty;
            e.FormattingApplied = true;
        }

        private void OnBrowse(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Export ColorIndex and RGB";
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
                MessageBox.Show(this, "Select the color table (ColorIndex + RGB) and/or level colors.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
            }
        }
    }
}
