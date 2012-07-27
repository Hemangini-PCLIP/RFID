    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using System.Data.SqlClient;

    namespace RFID
    {
        public partial class Form1 : Form
        {
            SqlDataReader rdr = null;
            SqlConnection con = null;
            SqlCommand cmd = null;
            string ConnectionString = "Data Source=.\\SQLEXPRESS;AttachDbFilename=D:\\RFID_Employee_In_Out\\RFID\\RFID\\RFID.mdf;Integrated Security=True;User Instance=True";
          
            public Form1()
            {
                InitializeComponent();
               
            }

            private void textBox1_TextChanged(object sender, EventArgs e)
            {

                if (timer1.Enabled)
                {
                    MessageBox.Show("You are swapping more than once" + timer1.Tag);                    
                }
                if (textBox1.Text.ToString().Length == 8)
                {                
                    try
                    {                    
                        con = new SqlConnection(ConnectionString);
                        con.Open();
                                      
                        string CommandText = "SELECT first_name, last_name , snap" +
                                             "  FROM tblContact" +
                                             " Where ID_number = '" + textBox1.Text.ToString()+"'";
                        cmd = new SqlCommand(CommandText);
                        cmd.Connection = con;

                        rdr = cmd.ExecuteReader();
                                                
                            bool success = false;

                            while (rdr.Read())
                            {
                                success = true;
                                snap.Image = new Bitmap(rdr["snap"].ToString());
                                lblFullName.Text = rdr["first_name"].ToString() + " " + rdr["last_name"].ToString();                                
                                textBox1.Hide();
                                snap.Show();
                                lblFullName.Show();
                                label1.Show();
                                label2.Show();
                                label3.Show();
                            }

                            if (success == false)
                            {
                                MessageBox.Show("Wrong card entry....");
                            }
                            else
                            {

                                In_Out_Method(textBox1.Text.ToString());
                                //Calculate number of hours worked

                                con = new SqlConnection(ConnectionString);
                                con.Open();

                                CommandText = "SELECT in_out , ID_number , in_Time" +
                                                     "  FROM tblInOut" +
                                                     " Where ID_number = '" + textBox1.Text.ToString() + "'";
                                cmd = new SqlCommand(CommandText);
                                cmd.Connection = con;

                                rdr = cmd.ExecuteReader();
                                int count = 0;
                                TimeSpan hours_worked = new TimeSpan();
                                TimeSpan dt = new TimeSpan();
                                while (rdr.Read())
                                {
                                    if (count == 0)
                                    {
                                        count = 1;
                                        dt = (TimeSpan)rdr["in_Time"];
                                    }
                                    else
                                    {
                                        count = 0;

                                        hours_worked = hours_worked + dt.Subtract((TimeSpan)rdr["in_Time"]);

                                        //MessageBox.Show(hours_worked.ToString());

                                    }
                                }
                                label2.Text = hours_worked.ToString();
                            }

                                                
                    }
                    catch (Exception ex)
                    {
                        // Print error message
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                       // Close data reader object and database connection
                        textBox1.Clear();  
                        if (rdr != null)
                            rdr.Close();

                        if (con.State == ConnectionState.Open)
                            con.Close();
                        timer1.Enabled = true;
                        
                        timer1.Start();                        
                    }
                
                }           
            }

            
            private bool Is_Signed_Today_Method(String RFID)
             {
                bool success = false;
                try
                {
                    con = new SqlConnection(ConnectionString);
                                       
                    con.Open();

                    SqlCommand cmd = new SqlCommand("Is_Signed_today", con);
                  
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@IN_OUT", 1));
                    cmd.Parameters.Add(new SqlParameter("@RFID", RFID.ToString()));
                 
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        {
                            success = true;                    
                        }                        
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(""+ex);
                }
                
                return success;
            }
            private bool Is_In_Method(String RFID)
            {
                bool success = false;
                try
                {
                    con = new SqlConnection(ConnectionString);

                    con.Open();

                    SqlCommand cmd = new SqlCommand("Is_In", con);

                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@RFID", RFID.ToString()));

                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        if ((int)rdr["in_out"] == 1)
                        {
                            success = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("" + ex);
                }
                finally
                {
                    
                }

                return success;
            }
            private void In_Out_Method(String RFID_number)
            {
                String Insert = null;               
                DateTime datetime = (DateTime)DateTime.Now;

                if (Is_Signed_Today_Method(RFID_number))
                {
                    if (Is_In_Method(RFID_number))
                    {
                        Insert = "insert into tblInOut(ID_number, in_out, in_Date , in_Time)values ('" + RFID_number + "','" + 0 + "','" + datetime.ToString("yyyy/MM/dd") + "','" + datetime.ToString("HH:mm:ss") + "')";
                        label3.Text = "Signed out Successfully";
                    }
                    else
                    {
                        Insert = "insert into tblInOut(ID_number, in_out, in_Date, in_Time)values ('" + RFID_number + "','" + 1 + "','" + datetime.ToString("yyyy/MM/dd") + "','" + datetime.ToString("HH:mm:ss") + "')";
                        label3.Text = "Signed In Successfully";
                    }
                }
                else
                {
                    label3.Text = "Signed In Successfully";
                    Insert = "insert into tblInOut(ID_number, in_out, in_Date, in_Time)values ('" + RFID_number + "','" + 1 + "','" + datetime.ToString("yyyy/MM/dd") + "','" + datetime.ToString("HH:mm:ss") + "')";
                }

                try
                {                   
                    SqlConnection conn = new SqlConnection(ConnectionString);
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(Insert, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }                
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                                
            }
        
            private void Form1_Load(object sender, EventArgs e)
            {
                snap.Hide();
                lblFullName.Hide();
                label1.Hide();
                label2.Hide();
                label3.Hide();
                timer1.Enabled = false;
                timer1.Interval = 10000;
            }

            private void timer1_Tick(object sender, EventArgs e)
            {
                snap.Hide();
                lblFullName.Hide();
                label1.Hide();
                label2.Hide();
                label3.Hide();
                timer1.Enabled = false;
                timer1.Interval = 5000;
                textBox1.Clear();
                textBox1.Show();
                textBox1.Focus();
            }

        }
    }
