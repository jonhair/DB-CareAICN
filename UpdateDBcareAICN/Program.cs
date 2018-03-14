//Updates db_CareAICNs and is scheduled for daily run

using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace UpdateDBcareAICN
{
    class Program
    {
        static List<Account> actList = new List<Account>();
        static FileStream fs;
        static StreamWriter sw;

        static void Main(string[] args)
        {
            //TEST


            //directory for log files
            Directory.CreateDirectory("P:\\SpecialP\\Jon\\Resources\\Data\\CareAICN Logs");

            DateTime dateToday = DateTime.Now;

            //process log
            fs = new FileStream(@"P:\\SpecialP\\Jon\\Resources\\Data\\CareAICN Logs\\" + dateToday.ToString("MM-dd-yyyy") + ".log", FileMode.Create, FileAccess.ReadWrite);
            sw = new StreamWriter(fs);

            updateDB();
            sw.Flush();

        }

        static void updateDB()
        {
            Console.WriteLine("Contacting SQL...");

            SqlConnection sqlConnCash = new SqlConnection("Data Source=dcr-cbo-ms-01;Initial Catalog=db_cashMgmt;User ID=cashMgmt;Password=!@#$cash!@#$");
            sqlConnCash.Open();

            SqlConnection sqlConnAICN = new SqlConnection("Data Source=dcr-cbo-ms-01;Initial Catalog=db_CareAICNs;User ID=cashMgmt;Password=!@#$cash!@#$");
            sqlConnAICN.Open();

            SqlTransaction sqlTrans;

            DateTime dtPast = DateTime.Now.AddDays(0);

            //string selStr = @"SELECT fileLocation FROM tblFile WHERE fileLocation LIKE 'H:\ElecCashP\Medicare_NC\835\hold\Origbefore74%' " +
            //                $"AND dateImp BETWEEN '{dtPast.ToString("MM-dd-yyyy")} 12:00:00 AM' AND '{dtPast.ToString("MM-dd-yyyy")} 11:59:59 PM'";

            string selStr = @"SELECT fileLocation FROM tblFile WHERE fileLocation LIKE 'H:\ElecCashP\Medicare_NC\835\hold\Origbefore74%' " +
                            $"AND dateImp BETWEEN '2018-03-09 12:00:00 AM' AND '2018-03-15 10:00:00 AM'";

            SqlCommand sqlComm = new SqlCommand(selStr, sqlConnCash);
            SqlDataReader sqlReader = sqlComm.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(sqlReader);
            sqlConnCash.Close();

            int rowCount = dt.Rows.Count;

            Console.WriteLine(rowCount + " files to process.");

            string acctNum, stat, billAmt, payAmt, ICN, last, first, HIC, checkAmt, checkDate, EFT;
            acctNum = stat = billAmt = payAmt = ICN = last = first = HIC = checkAmt = checkDate = EFT = "";

            //write log info start
            sw.WriteLine($"{DateTime.Now.ToString()} : {rowCount} files to process");

            foreach (DataRow row in dt.Rows)
            {
                sw.WriteLine($"{DateTime.Now.ToString()} : {row.Field<string>("fileLocation")}");

                if (File.Exists(row.Field<string>("fileLocation")))
                {
                    string[][] arr835 = File.ReadAllText(row.Field<string>("fileLocation")).Split('~').Select(s => s.Split('*')).ToArray();

                    for (int i = 0; i < arr835.GetLength(0); i++)
                    {
                        if (arr835[i][0].Contains("CLP"))
                        {
                            acctNum = arr835[i][1];
                            stat = arr835[i][2];
                            billAmt = arr835[i][3];
                            payAmt = arr835[i][4];
                            ICN = arr835[i][7];
                        }
                        else if(arr835[i][0].Contains("BPR"))
                        {
                            checkAmt = arr835[i][2];
                            checkDate = arr835[i][16];
                        }
                        else if (arr835[i][0].Contains("TRN"))
                        {
                            EFT = arr835[i][2];
                        }
                        else if (arr835[i][0].Contains("NM1") && arr835[i][1].Contains("QC") && arr835[i][2].Contains("1"))
                        {
                            last = arr835[i][3];
                            first = arr835[i][4];
                            HIC = arr835[i][9];

                            sqlTrans = sqlConnAICN.BeginTransaction();
                            sqlComm = new SqlCommand();
                            sqlComm.Connection = sqlConnAICN;
                            sqlComm.Transaction = sqlTrans;

                            StringBuilder sb = new StringBuilder();
                            sb.Append(checkDate.Substring(0, 4));
                            sb.Append("-");
                            sb.Append(checkDate.Substring(4, 2));
                            sb.Append("-");
                            sb.Append(checkDate.Substring(6, 2));

                            string tmpDate = "";
                            tmpDate = sb.ToString();

                            Account tmpAct = new Account(acctNum, HIC, ICN, last, first, stat, billAmt, payAmt, checkAmt, tmpDate, EFT);
                            actList.Add(tmpAct);

                            DateTime eftDate = Convert.ToDateTime(tmpDate);


                            //write to log
                            sw.WriteLine($"Acct: {acctNum}, ICN: {ICN}");

                            sqlComm.CommandText = "INSERT INTO tbl_835Data (" +
                                                    "patacctno," +
                                                    "hic," +
                                                    "icn," +
                                                    "last," +
                                                    "first," +
                                                    "claimstat," +
                                                    "billamt," +
                                                    "payamt," +
                                                    "chkAmt," +
                                                    "chkDate," +
                                                    "eftNum" +
                                                    ") VALUES " +
                                                    $"('{acctNum}','{HIC}','{ICN}','{last}','{first}',{stat},{billAmt},{payAmt},{checkAmt},'{eftDate}','{EFT}')";

                            sqlComm.ExecuteNonQuery();
                            sqlTrans.Commit();
                        }
                    }
                }
            }

            sqlConnAICN.Close();
            sqlConnCash.Close();
        }
    }
}
