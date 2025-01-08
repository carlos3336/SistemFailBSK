using System;
using System.Data.SQLite;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace SistemFailBSK
{
    public partial class Form1 : Form
    {
        private Timer timer;
        private string _username;
        private int _accessLevel;
        private string connectionString = $"Data Source={Application.StartupPath}\\SistemFailBSK.db;Version=3;"; // Define connectionString here

        public Form1(string username)
        {
            InitializeComponent();
            _username = username;
            _accessLevel = GetUserAccessLevel(username);
            dataGridView2.Visible = false; // Set DataGridView to be initially invisible
            InitializeDateTimeLabel();
            InitializeDataGridView(); // Initialize DataGridView with custom columns
            rbtnUpdate.CheckedChanged += rbtnInsert_CheckedChanged;
            rbtnInsert.CheckedChanged += rbtnUpdate_CheckedChanged;
            InitializeUIBasedOnAccessLevel();
        }

        private int GetUserAccessLevel(string username)
        {
            // Use the correct connection string for users.db
            string connectionString = "Data Source=users.db;Version=3;";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT access_level FROM users WHERE username = @username";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        private void InitializeUIBasedOnAccessLevel()
        {
            if (_accessLevel != 1) // Assuming 1 is the admin level
            {
                tabAdminPanel.Enabled =false; // Disable the Admin Panel tab for non-admin users
            }
            else
            {
                tabAdminPanel.Enabled = true;
            }
        }

        private void InitializeDataGridView()
        {
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.Font = new Font("Microsoft Sans Serif", 10); // Adjust the font size as needed

            DataGridViewTextBoxColumn oydsNameColumn = new DataGridViewTextBoxColumn
            {
                Name = "OYDSNAME",
                HeaderText = "Nama OYDS",
                Width = 230 // Set the desired width for Nama OYDS column
            };
            dataGridView1.Columns.Add(oydsNameColumn);

            DataGridViewTextBoxColumn oydsNoColumn = new DataGridViewTextBoxColumn
            {
                Name = "OYDSNO",
                HeaderText = "No OYDS",
                Width = 100 // Set the desired width for No OYDS column
            };
            dataGridView1.Columns.Add(oydsNoColumn);

            DataGridViewTextBoxColumn fileNoColumn = new DataGridViewTextBoxColumn
            {
                Name = "FILENO",
                HeaderText = "No Fail",
                Width = 100 // Set the desired width for No Fail column
            };
            dataGridView1.Columns.Add(fileNoColumn);

            DataGridViewTextBoxColumn idColumn = new DataGridViewTextBoxColumn
            {
                Name = "ID",
                HeaderText = "ID",
                Visible = false // Hide the ID column
            };
            dataGridView1.Columns.Add(idColumn);

            dataGridView1.CellClick += DataGridView1_CellClick;
        }

        private void InitializeDateTimeLabel()
        {
            timer = new Timer();
            timer.Interval = 1000; // Update every second
            timer.Tick += Timer_Tick;
            timer.Start();
            UpdateDateTimeLabel(); // Set the label text initially
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDateTimeLabel();
        }

        private void UpdateDateTimeLabel()
        {
            CultureInfo culture = new CultureInfo("ms-MY");
            string currentDate = DateTime.Now.ToString("dddd, dd MMMM yyyy", culture);
            label2.Text = $"Tarikh: {currentDate}";
        }

        private void btnCreateUser_Click(object sender, EventArgs e)
        {
            if (_accessLevel == 1)
            {
                //UserManagementForm userManagementForm = new UserManagementForm();
                //userManagementForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("You do not have permission to create users.");
            }
        }

        private void Simpan_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(NoDoc.Text))
                {
                    MessageBox.Show("No Dokumen hendaklah diisi!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!long.TryParse(NoDoc.Text, out long oydsno))
                {
                    MessageBox.Show("No Dokumen tidak sah. Sila masukkan nombor sahaja.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(comboBox1.Text) ||
                    string.IsNullOrWhiteSpace(oyds.Text) ||
                    string.IsNullOrWhiteSpace(NoFail.Text) ||
                    string.IsNullOrWhiteSpace(Pengeluar.Text) ||
                    string.IsNullOrWhiteSpace(Penerima.Text))
                {
                    MessageBox.Show("Isikan semua ruang!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string connectionString = $"Data Source={Application.StartupPath}\\SistemFailBSK.db;Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string insertQuery = @"
                        INSERT INTO SFBSK (PKS, OYDSNAME, FILENO, OYDSNO, PROVIDER, RECEIVER, REMARK, PROVIDEDATE)
                        VALUES (@PKS, @OYDSNAME, @FILENO, @OYDSNO, @PROVIDER, @RECEIVER, @REMARK, @PROVIDEDATE);
                    ";

                    using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@PKS", comboBox1.Text);
                        command.Parameters.AddWithValue("@OYDSNAME", oyds.Text);
                        command.Parameters.AddWithValue("@FILENO", NoFail.Text);
                        command.Parameters.AddWithValue("@OYDSNO", oydsno);
                        command.Parameters.AddWithValue("@PROVIDER", Pengeluar.Text);
                        command.Parameters.AddWithValue("@RECEIVER", Penerima.Text);
                        command.Parameters.AddWithValue("@REMARK", Remark.Text);
                        command.Parameters.AddWithValue("@PROVIDEDATE", DateTime.Now);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Data berjaya disimpan!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFormFields();
                        }
                        else
                        {
                            MessageBox.Show("Data tidak berjaya disimpan. Periksa semula data yang dimasukkan.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (SQLiteException sqlex)
            {
                MessageBox.Show($"SQLite Error: {sqlex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"General Error: {ex.Message}");
            }
        }

        private void ClearFormFields()
        {
            comboBox1.SelectedIndex = -1;
            oyds.Clear();
            NoFail.Clear();
            NoDoc.Clear();
            Pengeluar.Clear();
            Penerima.Clear();
            Remark.Clear();
            comboBox1.Focus();
        }

        private void Search_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            string keyword = searchKeyword.Text.Trim();
            string connectionString = $"Data Source={Application.StartupPath}\\SistemFailBSK.db;Version=3;";
            string searchQuery = @"
                SELECT ID, OYDSNAME, OYDSNO, FILENO
                FROM SFBSK
                WHERE OYDSNO LIKE @Keyword
                OR OYDSNAME LIKE @Keyword
                OR FILENO LIKE @Keyword";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(searchQuery, connection))
                {
                    command.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            dataGridView1.Rows.Add("Tiada rekod yang dijumpai.");
                            return;
                        }

                        while (reader.Read())
                        {
                            dataGridView1.Rows.Add(reader["OYDSNAME"], reader["OYDSNO"], reader["FILENO"], reader["ID"]);
                        }
                    }
                }
            }
        }

        private int selectedRecordId;

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow selectedRow = dataGridView1.Rows[e.RowIndex];
            int selectedID = Convert.ToInt32(selectedRow.Cells["ID"].Value);

            string connectionString = $"Data Source={Application.StartupPath}\\SistemFailBSK.db;Version=3;";
            string query = "SELECT * FROM SFBSK WHERE ID = @ID";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", selectedID);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            selectedRecordId = selectedID;
                            updatePKS.Text = reader["PKS"].ToString();
                            updateOYDSNAME.Text = reader["OYDSNAME"].ToString();
                            updateFILENO.Text = reader["FILENO"].ToString();
                            updateOYDSNO.Text = reader["OYDSNO"].ToString();
                            updatePROVIDER.Text = reader["PROVIDER"].ToString();
                            updateRECEIVER.Text = reader["RECEIVER"].ToString();
                            updateREMARK.Text = reader["REMARK"].ToString();

                            if (DateTime.TryParse(reader["PROVIDEDATE"]?.ToString(), out DateTime parsedDate))
                            {
                                ProvideDate.Text = parsedDate.ToString("dd/MM/yyyy HH:mm:ss");
                            }
                            else
                            {
                                ProvideDate.Text = string.Empty;
                            }

                            groupBox1.Visible = false;
                            tabControl1.SelectedTab = tabControl1.TabPages["Penerimaan"];
                        }
                        else
                        {
                            MessageBox.Show("No data found for the selected record.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void Kemaskini_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(updatePKS.Text) ||
                    string.IsNullOrWhiteSpace(updateOYDSNAME.Text) ||
                    string.IsNullOrWhiteSpace(updateFILENO.Text) ||
                    string.IsNullOrWhiteSpace(updateOYDSNO.Text) ||
                    string.IsNullOrWhiteSpace(updatePROVIDER.Text) ||
                    string.IsNullOrWhiteSpace(updateRECEIVER.Text))
                {
                    MessageBox.Show("All fields are required. Please fill in all fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string connectionString = $"Data Source={Application.StartupPath}\\SistemFailBSK.db;Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    if (rbtnUpdate.Checked)
                    {
                        string updateQuery = @"
                            UPDATE SFBSK 
                            SET PKS = @PKS, OYDSNAME = @OYDSNAME, FILENO = @FILENO, PROVIDER = @PROVIDER, 
                                RECEIVER = @RECEIVER, REMARK = @REMARK 
                            WHERE ID = @ID;
                        ";

                        using (SQLiteCommand command = new SQLiteCommand(updateQuery, connection))
                        {
                            command.Parameters.AddWithValue("@PKS", updatePKS.Text);
                            command.Parameters.AddWithValue("@OYDSNAME", updateOYDSNAME.Text);
                            command.Parameters.AddWithValue("@FILENO", updateFILENO.Text);
                            command.Parameters.AddWithValue("@PROVIDER", updatePROVIDER.Text);
                            command.Parameters.AddWithValue("@RECEIVER", updateRECEIVER.Text);
                            command.Parameters.AddWithValue("@REMARK", updateREMARK.Text);
                            command.Parameters.AddWithValue("@ID", selectedRecordId);

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Data berjaya dikemaskini!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearFormFields();
                            }
                            else
                            {
                                MessageBox.Show("Update failed. Please check your data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    else if (rbtnInsert.Checked)
                    {
                        string insertQuery = @"
                            INSERT INTO SFBSK (PKS, OYDSNAME, FILENO, OYDSNO, PROVIDER, RECEIVER, REMARK)
                            VALUES (@PKS, @OYDSNAME, @FILENO, @OYDSNO, @PROVIDER, @RECEIVER, @REMARK);
                        ";

                        using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@PKS", updatePKS.Text);
                            command.Parameters.AddWithValue("@OYDSNAME", updateOYDSNAME.Text);
                            command.Parameters.AddWithValue("@FILENO", updateFILENO.Text);
                            command.Parameters.AddWithValue("@OYDSNO", updateOYDSNO.Text);
                            command.Parameters.AddWithValue("@PROVIDER", updatePROVIDER.Text);
                            command.Parameters.AddWithValue("@RECEIVER", updateRECEIVER.Text);
                            command.Parameters.AddWithValue("@REMARK", updateREMARK.Text);

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Data berjaya disimpan!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearUpdateFormFields();
                            }
                            else
                            {
                                MessageBox.Show("Data gagal disimpan, semak semula data yang dimasukkan.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void ClearUpdateFormFields()
        {
            updateOYDSNAME.Clear();
            updateFILENO.Clear();
            updateOYDSNO.Clear();
            updatePROVIDER.Clear();
            updateRECEIVER.Clear();
            updateREMARK.Clear();
            ProvideDate.Clear();
            updatePKS.Focus();
        }

        private void backToResult_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = true;
            updateOYDSNAME.Clear();
            updateFILENO.Clear();
            updateOYDSNO.Clear();
            updatePROVIDER.Clear();
            updateRECEIVER.Clear();
            updateREMARK.Clear();
        }

        private void rbtnUpdate_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnUpdate.Checked)
            {
                groupBox1.Visible = true;
                Kemaskini.Text = "Kemaskini";

                updateOYDSNAME.ReadOnly = true;
                updateOYDSNO.ReadOnly = true;
                updateFILENO.ReadOnly = true;
                updatePROVIDER.ReadOnly = true;
                updateRECEIVER.ReadOnly = true;
                //updatePKS.ReadOnly = true;

                // Optionally, clear other input fields if necessary
                updateREMARK.Clear();
                ProvideDate.Clear();
            }
        }

        private void rbtnInsert_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnInsert.Checked)
            {
                // Set the group box visibility and button text for Insert mode
                groupBox1.Visible = false;
                Kemaskini.Text = "Simpan";

                // Unlock fields for data input
                updateOYDSNAME.ReadOnly = false;
                updateOYDSNO.ReadOnly = false;
                updateFILENO.ReadOnly = false;
                updatePROVIDER.ReadOnly = false;
                updateRECEIVER.ReadOnly = false;
                //updatePKS.ReadOnly = false;

                // Clear fields for fresh input
                ProvideDate.Clear();
                updateOYDSNAME.Clear();
                updateFILENO.Clear();
                updateOYDSNO.Clear();
                updatePROVIDER.Clear();
                updateRECEIVER.Clear();
                updateREMARK.Clear();

                // Optionally set focus to the first field
                updatePKS.Focus();
            }
        }

        private void btnCreateUser1_Click(object sender, EventArgs e)
        {
            string username = txtUsername1.Text.Trim();
            string password = txtPassword1.Text.Trim();
            string accessLevelText = cmbAccessLevel1.SelectedItem?.ToString();

            // Validate input fields
            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Nama Pengguna perlu diisi.", "Nama Pengguna", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Katalaluan perlu diisi!.", "Katalaluan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(accessLevelText) || !int.TryParse(accessLevelText, out int accessLevel))
            {
                MessageBox.Show("Sila pilih aras capaian pengguna!.", "Aras Capaian Pengguna", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string connectionString = "Data Source=users.db;Version=3;"; // Use users.db

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Check if the username already exists
                string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username";
                using (SQLiteCommand checkCommand = new SQLiteCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@username", username);
                    int userCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                    if (userCount > 0)
                    {
                        MessageBox.Show("Nama Pengguna telah wujud. Sila pilih nama pengguna yang berbeza.", "Nama Pengguna bertindih", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return; // Exit the method if the username is a duplicate
                    }
                }

                // Proceed with insertion if the username is unique
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd"); // Get current date in yyyy-MM-dd format
                string query = "INSERT INTO users (username, password, access_level, date) VALUES (@username, @password, @access_level, @date)";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);
                    command.Parameters.AddWithValue("@access_level", accessLevel);
                    command.Parameters.AddWithValue("@date", currentDate); // Add current date to parameters
                    command.ExecuteNonQuery();
                    MessageBox.Show("Daftar Pengguna Berjaya.");
                }
            }
        }




        private void btnDeleteUser1_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                // Assuming the first column contains the username or a unique identifier
                string username = dataGridView2.SelectedRows[0].Cells[0].Value.ToString();

                // Remove the entry from the database (you need to implement this method)
                DeleteUserFromDatabase(username);

                // Refresh the DataGridView
                dataGridView2.Rows.RemoveAt(dataGridView2.SelectedRows[0].Index);
            }
            else
            {
                MessageBox.Show("Please select a user to delete.");
            }
        }

        private void DeleteUserFromDatabase(string username)
        {
            // Implement the logic to remove the user from the database
            // Example using SQLite:
            using (var connection = new SQLiteConnection("Data Source=users.db;Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand("DELETE FROM users WHERE username = @username", connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.ExecuteNonQuery();
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            


        }


        private void userlist_Click(object sender, EventArgs e)
        {
            LoadDataIntoDataGridView();
            dataGridView2.Visible = true; // Make DataGridView visible after loading data
        }

        private void LoadDataIntoDataGridView()
        {
            string userDbConnectionString = "Data Source=users.db;Version=3;";
            string query = "SELECT username, access_level, date FROM users"; // Adjust your query as necessary

            using (SQLiteConnection connection = new SQLiteConnection(userDbConnectionString))
            {
                try
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            // Clear existing columns and rows
                            dataGridView2.Columns.Clear();
                            dataGridView2.Rows.Clear();

                            // Define and add columns with custom headers
                            var columnUsername = new DataGridViewTextBoxColumn();
                            columnUsername.HeaderText = "Pengguna"; // Custom header
                            columnUsername.Name = "Username";
                            columnUsername.Width = 85; // Adjust column width

                            var columnAccessLevel = new DataGridViewTextBoxColumn();
                            columnAccessLevel.HeaderText = "Aras"; // Custom header
                            columnAccessLevel.Name = "AccessLevel";
                            columnAccessLevel.Width = 30; // Adjust column width
                            columnAccessLevel.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; // Center alignment


                            var columnDate = new DataGridViewTextBoxColumn();
                            columnDate.HeaderText = "Tarikh Daftar"; // Custom header
                            columnDate.Name = "Date";
                            columnDate.Width = 97; // Adjust column width

                            // Add columns to the DataGridView
                            dataGridView2.Columns.Add(columnUsername);
                            dataGridView2.Columns.Add(columnAccessLevel);
                            dataGridView2.Columns.Add(columnDate);

                            // Add rows
                            while (reader.Read())
                            {
                                dataGridView2.Rows.Add(reader["username"], reader["access_level"], reader["date"]);
                            }

                            MessageBox.Show("Data loaded successfully."); // Debug message
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading data: " + ex.Message);
                }
            }
        }


    }
}

