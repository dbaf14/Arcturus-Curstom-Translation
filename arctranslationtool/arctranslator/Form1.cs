using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ArcTransTool
{
    public partial class Form1 : Form
    {
        private Button btnLoadTxt, btnLoadScr, btnTranslate, btnClose;
        private DataGridView dataGridView1;
        private TextBox textBoxLegend;
        private string loadedFilePath = "";
        private List<string> fileLines = new List<string>();
        private List<int> matchedLineIndices = new List<int>();
        private Dictionary<char, char> legendMap = new Dictionary<char, char>();

        public Form1()
        {
            InitializeComponents();
            LoadLegend();
        }

        private void InitializeComponents()
        {
            this.Text = "ArcTransTool";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Legende (Bereich 3)
            textBoxLegend = new TextBox
            {
                Location = new Point(10, 10),
                Size = new Size(960, 60),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(textBoxLegend);

            // DataGridView für Original + Übersetzung (Bereiche 1 + 2)
            dataGridView1 = new DataGridView
            {
                Location = new Point(10, 80),
                Size = new Size(960, 500),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dataGridView1.Columns.Add("Original", "Originaltext");
            dataGridView1.Columns.Add("Translation", "Translation");
            dataGridView1.Columns[0].ReadOnly = true;
            this.Controls.Add(dataGridView1);

            // Buttons
            btnLoadTxt = new Button
            {
                Text = "Load .txt File",
                Location = new Point(10, 590),
                Size = new Size(120, 30)
            };
            btnLoadTxt.Click += BtnLoadTxt_Click;
            this.Controls.Add(btnLoadTxt);

            btnLoadScr = new Button
            {
                Text = "Load .scr File",
                Location = new Point(140, 590),
                Size = new Size(120, 30)
            };
            btnLoadScr.Click += BtnLoadScr_Click;
            this.Controls.Add(btnLoadScr);

            btnTranslate = new Button
            {
                Text = "<Translate&Save<",
                Location = new Point(270, 590),
                Size = new Size(120, 30)
            };
            btnTranslate.Click += BtnTranslate_Click;
            this.Controls.Add(btnTranslate);

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(400, 590),
                Size = new Size(120, 30)
            };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private void LoadLegend()
        {
            string legendPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "legend.ini");
            if (!File.Exists(legendPath)) return;

            string[] lines = File.ReadAllLines(legendPath, Encoding.UTF8);
            StringBuilder legendDisplay = new StringBuilder();
            foreach (string line in lines)
            {
                if (line.Contains("§"))
                {
                    var parts = line.Split('§');
                    if (parts.Length == 2)
                    {
                        char from = parts[0].Trim()[0];
                        char to = parts[1].Trim()[0];
                        legendMap[to] = from;
                        legendDisplay.AppendLine($"{to} = {from}");
                    }
                }
            }
            textBoxLegend.Text = legendDisplay.ToString();
        }

        private void LoadFile(string path)
        {
            loadedFilePath = path;
            fileLines = new List<string>(File.ReadAllLines(path, Encoding.GetEncoding("euc-kr")));
            matchedLineIndices.Clear();
            dataGridView1.Rows.Clear();

            Regex regex = new Regex(@"\b(say|shout|monologue|header|msg)\s*""([^""]+)""", RegexOptions.IgnoreCase);

            for (int i = 0; i < fileLines.Count; i++)
            {
                Match match = regex.Match(fileLines[i]);
                if (match.Success)
                {
                    string text = match.Groups[2].Value;
                    dataGridView1.Rows.Add(text, "");
                    matchedLineIndices.Add(i);
                }
            }
        }

        private void BtnLoadTxt_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Textdateien (*.txt)|*.txt" };
            try
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                    LoadFile(ofd.FileName);
            }
            finally
            {
                ofd.Dispose();
            }
        }

        private void BtnLoadScr_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Scriptdateien (*.scr)|*.scr" };
            try
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                    LoadFile(ofd.FileName);
            }
            finally
            {
                ofd.Dispose();
            }
        }

        private void BtnTranslate_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < matchedLineIndices.Count; i++)
            {
                string newText = dataGridView1.Rows[i].Cells[1].Value?.ToString();
                if (!string.IsNullOrWhiteSpace(newText))
                {
                    string mappedText = ApplyLegendMapping(newText);
                    int lineIndex = matchedLineIndices[i];

                    fileLines[lineIndex] = Regex.Replace(fileLines[lineIndex],
                        "\"([^\"]+)\"",
                        $"\"{mappedText}\"",
                        RegexOptions.IgnoreCase);
                }
            }

            File.WriteAllLines(loadedFilePath, fileLines, Encoding.GetEncoding("euc-kr"));
            LoadFile(loadedFilePath); // Neu laden, um Änderungen zu zeigen
        }

        private string ApplyLegendMapping(string input)
        {
            StringBuilder sb = new StringBuilder(input);
            foreach (var map in legendMap)
            {
                sb.Replace(map.Key, map.Value);
            }
            return sb.ToString();
        }
    }
}
