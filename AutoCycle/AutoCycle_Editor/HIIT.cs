using AutoCycle.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoCycle_Editor
{
    public partial class HIIT : Form
    {
        public HIIT()
        {
            InitializeComponent();
        }

        private void HIIT_Load(object sender, EventArgs e)
        {

        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[1].Value is not null)
                {
                    BikeService.SendResistanceForHIIT(Convert.ToInt32(row.Cells[0].Value), (string)row.Cells[2].Value);

                    Thread.Sleep(Convert.ToInt32(row.Cells[1].Value));
                }
            }
        }

        private bool IsDataValid()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (string.IsNullOrWhiteSpace((string)cell.Value))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new();

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter streamWriter = new(saveFileDialog.FileName))
                {
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        streamWriter.WriteLine($"{row.Cells[0].Value},{row.Cells[1].Value},{row.Cells[2].Value}");
                    }

                    streamWriter.Close();
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new();

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string[] lines = File.ReadAllLines(openFileDialog.FileName);

                foreach (string line in lines)
                {
                    string[] items = line.Split(',');

                    dataGridView1.Rows.Add(items[0], items[1], items[2]);
                }
            }
        }
    }
}
