using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Running_Balance_in_Crystal_Report
{
    public partial class Form1 : Form
    {
        string ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Data\RunningBalance.mdf;Integrated Security=True";
        public Form1()
        {
            InitializeComponent();
            LoadReport();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if(TBCustomerName.Text.Trim().Length == 0 )
            {
                MessageBox.Show("Customer Name is Required");
                TBCustomerName.Focus();
                return;
            }

            string CustomerName = TBCustomerName.Text;

            string DateStr = TBDate.Text;

            try
            {
                Convert.ToDateTime(DateStr);
            }
            catch
            {
                MessageBox.Show("Invalid Date");
                TBDate.Focus();
                return;
            }

            string Notes = TBNotes.Text;

            string DueAmountStr = TBDueAmount.Text;

            try
            {
                if (!String.IsNullOrEmpty(DueAmountStr))
                    Convert.ToDouble(DueAmountStr);
            }
            catch
            {
                MessageBox.Show("Invalid DueAmount");
                TBDueAmount.Focus();
                return;
            }

            string CashReceivedStr = TBCashReceived.Text;

            try
            {
                if (!String.IsNullOrEmpty(CashReceivedStr))
                    Convert.ToDouble(CashReceivedStr);
            }
            catch
            {
                MessageBox.Show("Invalid Cash Received");
                TBCashReceived.Focus();
                return;
            }


            if(DueAmountStr != "0")
            {
                string query = "Insert Into [Invoices] (CustomerName, Date, DueAmount, Notes) Values (@CustomerName, @Date, @DueAmount, @Notes)";
                SqlConnection con = new SqlConnection(ConnectionString);
                con.Open();
                SqlCommand com = new SqlCommand(query, con);
                com.Parameters.AddWithValue("@CustomerName", CustomerName);
                com.Parameters.AddWithValue("@Date", DateStr);
                com.Parameters.AddWithValue("@DueAmount", DueAmountStr);
                com.Parameters.AddWithValue("@Notes", Notes);

                com.ExecuteNonQuery();
                con.Close();
            }
            else if(CashReceivedStr != "0")
            {
                string query = "Insert Into [Payments] (CustomerName, Date, PaidAmount, Notes) Values (@CustomerName, @Date, @PaidAmount, @Notes)";
                SqlConnection con = new SqlConnection(ConnectionString);
                con.Open();
                SqlCommand com = new SqlCommand(query, con);
                com.Parameters.AddWithValue("@CustomerName", CustomerName);
                com.Parameters.AddWithValue("@Date", DateStr);
                com.Parameters.AddWithValue("@PaidAmount", CashReceivedStr);
                com.Parameters.AddWithValue("@Notes", Notes);

                com.ExecuteNonQuery();
                con.Close();
            }

            TBDate.Text = "";
            TBNotes.Text = "";
            TBDueAmount.Text = "";
            TBCashReceived.Text = "";

            LoadReport();
        }

        private void LoadReport()
        {
            ReportDS DS = new ReportDS();
            RunningBalanceReport Report = new RunningBalanceReport();

            DataRow row = DS.Tables["Report"].NewRow();
            row["CustomerName"] = TBCustomerName.Text;
            DS.Tables["Report"].Rows.Add(row);

            SqlConnection con = new SqlConnection(ConnectionString);
            con.Open();

            string query = "Select Date, DueAmount As 'Debit', Notes, '0' As 'Credit' From [Invoices] Where CustomerName = @CustomerName" + Environment.NewLine +
                "Union All" + Environment.NewLine +
                "Select Date, '0' As Debit, Notes, (-1 * PaidAmount) As 'Credit' From [Payments] Where CustomerName = @CustomerName" + Environment.NewLine +
                "Order By Date";

            SqlCommand com = new SqlCommand(query, con);

            com.Parameters.AddWithValue("@CustomerName", TBCustomerName.Text);

            SqlDataAdapter da = new SqlDataAdapter(com);
            da.Fill(DS, "Rows");

            Report.SetDataSource(DS);
            ReportViewer.ReportSource = Report;
        }
    }
}
