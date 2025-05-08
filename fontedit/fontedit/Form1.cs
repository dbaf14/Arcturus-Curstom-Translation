// Form1.cs
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace fontedit
{
    public partial class Form1 : Form
    {
        const int BytesPerLine = 18;
        byte[] data;

        DataGridView grid;
        Button btnLoad;
        Button btnSave;

        public Form1()
        {
            InitializeComponent();
            InitUI();
        }

        private void InitUI()
        {
            this.Text = "Hex-Grid Editor (18 Bytes/Zeile)";
            this.ClientSize = new Size(600, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            btnLoad = new Button
            {
                Text = "Load",
                Location = new Point(10, 10),
                Width = 80
            };
            btnLoad.Click += BtnLoad_Click;
            this.Controls.Add(btnLoad);

            btnSave = new Button
            {
                Text = "Save",
                Location = new Point(100, 10),
                Width = 80
            };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            grid = new DataGridView
            {
                Location = new Point(10, 50),
                Width = BytesPerLine * 30 + 2,
                Height = this.ClientSize.Height - 60,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                ColumnHeadersVisible = false,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font(FontFamily.GenericMonospace, 10),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            grid.CellEndEdit += Grid_CellEndEdit;
            grid.CellFormatting += Grid_CellFormatting;

            // 18 Spalten
            for (int i = 0; i < BytesPerLine; i++)
            {
                var col = new DataGridViewTextBoxColumn
                {
                    Width = 30,
                    MaxInputLength = 2
                };
                grid.Columns.Add(col);
            }

            this.Controls.Add(grid);
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "DAT-Dateien (*.dat)|*.dat|Alle Dateien|*.*"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            data = File.ReadAllBytes(dlg.FileName);
            FillGrid();
        }

        private void FillGrid()
        {
            grid.Rows.Clear();
            if (data == null) return;

            int lines = (int)Math.Ceiling(data.Length / (double)BytesPerLine);
            for (int r = 0; r < lines; r++)
            {
                var row = new DataGridViewRow();
                row.Height = 25;
                row.CreateCells(grid);
                for (int c = 0; c < BytesPerLine; c++)
                {
                    int idx = r * BytesPerLine + c;
                    if (idx < data.Length)
                        row.Cells[c].Value = data[idx].ToString("X2");
                    else
                        row.Cells[c].Value = "";
                }
                grid.Rows.Add(row);
            }
        }

        private void Grid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (data == null) return;
            var cell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            string txt = cell.Value?.ToString() ?? "";
            if (byte.TryParse(txt, System.Globalization.NumberStyles.HexNumber, null, out byte val))
            {
                int idx = e.RowIndex * BytesPerLine + e.ColumnIndex;
                if (idx < data.Length)
                    data[idx] = val;
            }
            else
            {
                int idx = e.RowIndex * BytesPerLine + e.ColumnIndex;
                cell.Value = idx < data.Length ? data[idx].ToString("X2") : "";
            }
        }

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var cell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            string txt = cell.Value?.ToString()?.ToUpper() ?? "";
            switch (txt)
            {
                case "FF":
                    cell.Style.BackColor = Color.Black;
                    cell.Style.ForeColor = Color.White;
                    break;
                case "AD":
                    cell.Style.BackColor = Color.FromArgb(40, 40, 40);
                    cell.Style.ForeColor = Color.White;
                    break;
                case "C0":
                    cell.Style.BackColor = Color.FromArgb(60, 60, 60);
                    cell.Style.ForeColor = Color.White;
                    break;
                case "80":
                    cell.Style.BackColor = Color.FromArgb(90, 90, 90);
                    cell.Style.ForeColor = Color.White;
                    break;
                case "A0":
                    cell.Style.BackColor = Color.FromArgb(70, 70, 70);
                    cell.Style.ForeColor = Color.White;
                    break;
                default:
                    cell.Style.BackColor = Color.White;
                    cell.Style.ForeColor = Color.Black;
                    break;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (data == null) return;

            // get all data from Grid to the data[]-Array
            for (int row = 0; row < grid.RowCount; row++)
            {
                for (int col = 0; col < grid.ColumnCount; col++)
                {
                    int index = row * BytesPerLine + col;
                    if (index >= data.Length) continue;

                    string val = grid.Rows[row].Cells[col].Value?.ToString();
                    if (byte.TryParse(val, System.Globalization.NumberStyles.HexNumber, null, out byte result))
                    {
                        data[index] = result;
                    }
                    else
                    {
                        data[index] = 0x00; // fallback if invalid
                    }
                }
            }

            var dlg = new SaveFileDialog
            {
                Filter = "DAT-Dateien (*.dat)|*.dat"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            File.WriteAllBytes(dlg.FileName, data);
            MessageBox.Show("File Saved.", "succsess", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }
}