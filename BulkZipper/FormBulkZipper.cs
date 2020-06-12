using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BulkZipper
{
    public partial class FormBulkZipper : Form
    {
        private bool stop = false;

        public FormBulkZipper()
        {
            InitializeComponent();
        }

        private void buttonOpenFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.RootFolder = Environment.SpecialFolder.MyComputer;
            dialog.SelectedPath = Environment.CurrentDirectory;
            var result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                textBoxFolder.Text = dialog.SelectedPath;
            }
        }

        private void buttonZip_Click(object sender, EventArgs e)
        {
            try
            {
                labelCount.Text = "0";
                labelProcessing.Text = "---";

                if (textBoxFolder.Text == "")
                {
                    MessageBox.Show("Choose a destination folder", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!Directory.Exists(textBoxFolder.Text))
                {
                    MessageBox.Show("Choose a valid folder", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (textBoxExtension.Text == "")
                {
                    MessageBox.Show("Type a extension", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var files = Directory.GetFiles(textBoxFolder.Text, "*." + textBoxExtension.Text);

                if (files == null || files.Length == 0)
                {
                    MessageBox.Show("No files found", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int index = 1;

                new Thread(() =>
                {
                    foreach (var file in files)
                    {
                        if (stop)
                        {
                            stop = false;
                            Thread.CurrentThread.Abort();
                        }

                        var zippedName = textBoxFolder.Text + "\\" + GetFileNameNoExtension(file) + ".zip";
                        var fileName = GetFileName(file);

                        labelProcessing.Invoke((MethodInvoker)delegate
                        {
                            labelProcessing.Text = fileName;
                            labelProcessing.Refresh();
                        });

                        Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile();
                        zip.ParallelDeflateThreshold = -1;
                        zip.AddEntry(fileName, File.ReadAllBytes(file));

                        if (File.Exists(zippedName))
                        {
                            File.Delete(zippedName);
                        }

                        zip.Save(zippedName);

                        labelCount.Invoke((MethodInvoker)delegate
                        {
                            labelCount.Text = index.ToString();
                            labelCount.Refresh();
                        });

                        index++;
                    }


                    labelProcessing.Invoke((MethodInvoker)delegate
                    {
                        labelProcessing.Text = "Done!";
                        labelProcessing.Refresh();
                    });
                }).Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static string GetFileNameNoExtension(string file)
        {
            if (string.IsNullOrEmpty(file)) return "";
            var fileName = file.Substring(file.LastIndexOf("\\") + 1);
            var indexExtension = fileName.LastIndexOf(".");

            if (indexExtension == -1)
            {
                return fileName;
            }
            else
            {
                return fileName.Substring(0, fileName.LastIndexOf("."));
            }
        }

        public static string GetFileName(string file)
        {
            if (string.IsNullOrEmpty(file)) return "";
            return file.Substring(file.LastIndexOf("\\") + 1);
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            stop = true;
        }
    }
}
