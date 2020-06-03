using System;
using System.Data.SqlClient;
using System.IO;
using System.Collections.Generic;


namespace CreatingStagingSOTFeatureFile
{
    class CreateStagingFile
    {
        static void Main(string[] args)
        {


            List<String> Table = new List<String>();


            string dir = @"C:\Users\fmang\source\repos\MnmFalconValidation\Stage_SOT Test\FeatureFiles\";


            using (SqlConnection FalconSOT = new SqlConnection("Data Source=DL9ZYL4H2;Initial Catalog=FalconSOT;Integrated Security=True"))
            {
                FalconSOT.Open();
                try
                {

                    /** Get List to store Url's**/

                    SqlCommand GetTableName = new SqlCommand("SELECT distinct name FROM sys.tables where schema_id = 1 and name not in ('aws_EC2SecurityGroupAssignment','infra_Subnet','infra_DNSAlias','infra_DNSEntry','TableCountAudit') order by name", FalconSOT);

                    SqlDataReader y = GetTableName.ExecuteReader();


                    /**Iterate query & add Url's to list**/

                    while (y.Read())
                    {
                        Table.Add(y["name"].ToString().ToLower());

                    }

                    y.Close();
                }

                catch (SqlException Ex)

                {

                    Console.WriteLine(Ex.Message.ToString());
                }

                
                
                /**Get all locations for each table**/

                try
                {
                    foreach (String Current_Name in Table)
                    {
                        

                        string Path = dir + " " + Current_Name + ".feature";

                        using (StreamWriter fs = File.CreateText(Path))
                        {
                            fs.Write("Feature: " + Current_Name + "\n" + "@mytag\n" + "Scenario Outline: Data Verification, " + Current_Name + "\n" + "Given the following <SOT_Table> and <LocationID>\n" + "When I lookup against the <Staging_Table>\n" + "Then I should find that the record counts are equal\n" + "Examples:\n" + "| Location |SOT_Table | LocationID | Staging_Table |\n");
                        }

                        string STB;


                        if (Current_Name.Substring(0, 3) == "aws")
                        {
                            char sep = '_';
                            string[] Name = Current_Name.Split(sep, StringSplitOptions.None);
                            STB = Name[1];
                        }


                        else
                        {
                            STB = Current_Name;
                        }


                        /* Retrieve locationid, locationname for every table*/
                        SqlCommand Query = new SqlCommand("SELECT distinct cast(a.locationid as int) locationid, b.locationname LocName from " + Current_Name
                        + " a join [dbo].[vw_LatestSOTLAuditID] b on a.locationid = b.locationid", FalconSOT);

                        SqlDataReader z = Query.ExecuteReader();

                        while (z.Read())
                        {


                            string LocationName = z["LocName"].ToString();
                            string LocationID = z["locationid"].ToString();

                            using (StreamWriter fs = File.AppendText(Path))
                            {

                                fs.Write("| " + LocationName + " | " + Current_Name + " | " + LocationID + " | " + LocationName + "." + STB + " |\n");
                            }
                        }
                        z.Close();

                    }
                }



                catch (SqlException Ex)
                {
                    Console.WriteLine(Ex.Message.ToString());
                    Console.ReadLine();
                }
                
            }
        }
    }
}