using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace DAQ_Review
{
    public partial class FormMain : Form
    {
        public static string connString = "Host=localhost;Username=postgres;Password=.;Database=testdb";

        public FormMain()
        {
            InitializeComponent();
        }

        private async void FormMain_Load(object sender, EventArgs e)
        {
            using (var connection = new NpgsqlConnection(connString))
            {
                await connection.OpenAsync();

                var cmd = new NpgsqlCommand("SELECT * FROM public.\"Unreviewed Test Data\"", connection);

                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    listViewTests.Items.Add(new ListViewItem(new string[] { reader.GetString(3), reader.GetString(1), reader.GetInt64(2).ToString(), reader.GetString(4), reader.GetInt64(0).ToString() }));
                }
            }
               
        }

        private async void listViewTests_SelectedIndexChanged(object sender, EventArgs e)
        {
            tableLayoutPanelData.Controls.Clear();
            var it = listViewTests.SelectedItems[0];
            var task_id = Int64.Parse(it.SubItems[4].Text);

            using (var connection = new NpgsqlConnection(connString))
            {
                await connection.OpenAsync();
                var cmd = new NpgsqlCommand("SELECT * FROM public.test_data RIGHT JOIN tests ON tests.test_id = test_data.test WHERE tests.task_id = @id", connection);
                cmd.Parameters.AddWithValue("id", task_id);

                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var data_type = reader.GetString(2);

                    if (data_type == "dbm_scalar")
                    {
                        var lbl = new Label();
                        lbl.Text = reader.GetString(3) + " dBm";
                        tableLayoutPanelData.Controls.Add(lbl);
                    }
                    else if (data_type == "spectrum_plot_url")
                    {
                        var img = new PictureBox();
                        img.SizeMode = PictureBoxSizeMode.AutoSize;
                        img.ImageLocation = reader.GetString(3);
                        tableLayoutPanelData.Controls.Add(img);
                    }
                }
            }

        }
    }
}
