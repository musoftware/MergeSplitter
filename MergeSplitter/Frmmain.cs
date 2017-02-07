using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MergeSplitter
{
    public partial class Frmmain : Form
    {
        public Frmmain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog open = new OpenFileDialog())
            {
                open.Filter = "txt,csv|*.txt;*.csv";
                open.Multiselect = true;
                if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    foreach (var item in open.FileNames)
                    {
                        listBox1.Items.Add(item);
                    }
                }

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (string item in (from string s in listBox1.SelectedItems select s).ToArray())
            {
                listBox1.Items.Remove(item);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog save = new SaveFileDialog())
            {
                save.Filter = "txt,csv|*.txt;*.csv";
                if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    System.IO.File.WriteAllText(save.FileName, "");
                    foreach (string item in listBox1.Items)
                    {
                        System.IO.File.AppendAllLines(save.FileName, File.ReadAllLines(item, Encoding.Default), Encoding.Default);
                    }
                }
            }
        }

        string filesFolder = "";
        System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
        private async void button5_Click(object sender, EventArgs e)
        {
            if (button5.Text == "Stop")
            {
                cts.Cancel();
                return;
            }

            try
            {
                cts = new System.Threading.CancellationTokenSource();
                button5.Text = "Stop";

                using (FolderBrowserDialog folder = new FolderBrowserDialog())
                {

                    if (folder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        filesFolder = folder.SelectedPath;
                        //string ds = Path.GetTempFileName();

                        //System.IO.File.WriteAllText(ds, "");

                        bool mem = (radioButton4.Checked);

                        await Task.Run(() =>
                        {
                            foreach (string filename in listBox1.Items)
                            {
                                //System.IO.File.AppendAllLines(ds, File.ReadAllLines(item, Encoding.Default), Encoding.Default);
                                string[] lines = File.ReadAllLines(filename, Encoding.Default);

                                int parts = 0;
                                if (radioButton1.Checked)
                                    parts = lines.Length / (int)numericUpDown1.Value;
                                else
                                    parts = (int)numericUpDown1.Value;

                                if (parts == 0) continue;


                                int prog = (int)(Math.Round((decimal)lines.Length / parts));

                                for (int i = 0; i < Math.Round((decimal)lines.Length / parts, 0, MidpointRounding.AwayFromZero); i++)
                                {
                                    string newfilename = Path.Combine(folder.SelectedPath,
                                        Path.GetFileNameWithoutExtension(filename) + "_" + (i + 1) +
                                          Path.GetExtension(filename));

                                    File.WriteAllText(newfilename, "", Encoding.Default);

                                    if (mem)
                                    {
                                        List<string> linesX = new List<string>();

                                        File.WriteAllLines(newfilename, lines.Skip(i * parts).Take(Math.Min(lines.Length, parts)).ToArray(), Encoding.Default);

                                        //linesX.Add(lines[i2]);
                                    }
                                    else
                                    {
                                        for (int i2 = i * parts; i2 < Math.Min(lines.Length, i * parts + parts); i2++)
                                        {
                                            File.AppendAllLines(newfilename, new string[] { lines[i2] }, Encoding.Default);
                                        }
                                    }

                                    this.BeginInvoke((MethodInvoker)delegate
                                    {
                                        toolStripStatusLabel2.Text = Path.GetFileNameWithoutExtension(newfilename);
                              
                                    toolStripProgressBar1.Value = i* 100 /prog;
                                    });
                                }
                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    toolStripStatusLabel2.Text = "";
                                });
                            }
                        }, cts.Token);

                    }


                }
            }
            finally
            {
                button5.Text = "Split";
            }
        }


        public void ExploreFile(string filePath)
        {
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", filePath));
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(filesFolder))
            {
                ExploreFile(filesFolder);
            }
        }
    }
}
