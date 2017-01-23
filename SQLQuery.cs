using System.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace ProductionLineTV
{
    public class DataLine
    {
        public string date;
        public string sequence;
        public string route;
        public string orderNo;
        public int orderQty;
        public int qcCount;
        public int eolCount; 
    }

    class SQLQuery
    {
        SqlConnection conn = null;

        string connection = "Data Source=SCORCH;Initial Catalog=live;User ID=sa";

        const string WHS_DUE_DATE = "Whs Due Date";
        const string SEQ = "Seq";
        const string ROUTE_NAME = "route_name";
        const string ORDER_NO = "order_no";
        const string ORDER_QTY = "order_qty";
        const string QC_COUNT = "QC Count";
        const string EOL_COUNT = "EOL Count";

        public SQLQuery()
        {
            conn = new SqlConnection(connection);
        }

        public bool Connect()
        {
            try
            {
                conn.Open();
                return true;
            }
            catch (Exception e)
            {
                string error = e.ToString();
                return false;
            }
        }

        public List<DataLine> GetDate(string date)
        {
            List<DataLine> data = new List<DataLine>();
            if(Connect())
            {
                try
                {
                    SqlDataReader myReader = null;
                    SqlCommand myCommand = new SqlCommand("SELECT DISTINCT * FROM [_Paul A0 Scan Integ 2] WHERE [Whs Due Date] ='" + date + "' ORDER BY Seq", conn);
                    using (myReader = myCommand.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            DataLine temp = new DataLine();
                            temp.date = myReader.GetDateTime(myReader.GetOrdinal(WHS_DUE_DATE)).ToShortDateString();
                            temp.sequence = SafeGetString(myReader, SEQ);
                            temp.route = SafeGetString(myReader, ROUTE_NAME);
                            temp.orderNo = SafeGetString(myReader, ORDER_NO);
                            temp.orderQty = SafeGetInt(myReader, ORDER_QTY);
                            temp.qcCount = SafeGetInt(myReader, QC_COUNT);
                            temp.eolCount = SafeGetInt(myReader, EOL_COUNT);
                            data.Add(temp);
                        }
                    }
                    myReader.Close();
                    conn.Close();
                    return data;
                }
                catch (Exception e)
                {
                    string error = e.ToString();
                    conn.Close();
                    return data;
                }
            }
            return data;
        }

        public DateTime GetLastDate()
        {
            DateTime last = new DateTime();
            if(Connect())
            {
                string barcode = "";
                try
                {
                    SqlDataReader myReader = null;
                    SqlCommand myCommand = new SqlCommand("SELECT TOP 1 * FROM [_Paul A0 Prod Count] ORDER BY Date DESC", conn);
                    using (myReader = myCommand.ExecuteReader())
                    {
                        myReader.Read();
                        barcode = SafeGetString(myReader, "Barcode");
                    }
                    barcode = barcode.Substring(0, 6);
                    myCommand = new SqlCommand("SELECT DISTINCT [Whs Due Date], [order_no] FROM [_Paul A0 Scan Integ 2] WHERE [order_no] = '" + barcode + "'", conn);
                    using (myReader = myCommand.ExecuteReader())
                    {
                        if (myReader.HasRows)
                        {
                            myReader.Read();
                            last = myReader.GetDateTime(myReader.GetOrdinal("Whs Due Date"));
                        }
                    }
                    myReader.Close();
                    conn.Close();
                    return last;
                }
                catch (Exception e)
                {
                    string error = e.ToString();
                    conn.Close();
                    return last;
                }
            }
            return last;
        }

        public Date ConstructData(List<DataLine> inData)
        {
            Date outData = new Date();
            foreach(DataLine l in inData)
            {
                Sequence seq = new Sequence();
                Route route = new Route();
                OrderNo orderNo = new OrderNo();

                orderNo.qty = l.orderQty;
                orderNo.name = l.orderNo;
                orderNo.qc = l.qcCount;
                orderNo.qeol = l.eolCount;
                route.name = l.route;
                route.orderNos.Add(orderNo);
                seq.name = l.sequence;
                seq.routes.Add(route);
                outData.name = l.date;

                Sequence seqT = outData.sequences.Find(i => i.name == l.sequence);
                if(seqT != null)
                {
                    Route rouT = seqT.routes.Find(i => i.name == l.route);
                    if (rouT != null)
                    {
                        OrderNo ordT = rouT.orderNos.Find(i => i.name == l.orderNo);
                        if (ordT != null)
                        {
                            if (ordT.qeol != ordT.qty || ordT.qc != ordT.qty)
                            {
                                ordT.green = false;
                                rouT.green = false;
                                seqT.green = false;
                                outData.green = false;
                            }
                            ordT.qty += l.orderQty;
                            ordT.qc += l.qcCount;
                            ordT.qeol += l.eolCount;
                            if (ordT.qc > ordT.qty)
                                ordT.qc = ordT.qty;
                            if (ordT.qeol > ordT.qty)
                                ordT.qeol = ordT.qty;
                        }
                        else
                        {
                            if (orderNo.qeol != orderNo.qty || orderNo.qc != orderNo.qty)
                            {
                                orderNo.green = false;
                                rouT.green = false;
                                seqT.green = false;
                                outData.green = false;
                            }
                            rouT.orderNos.Add(orderNo);
                            rouT.ordCount++;
                            seqT.ordCount++;
                            outData.ordCount++;
                        }
                    }
                    else
                    {
                        if (orderNo.qeol != orderNo.qty || orderNo.qc != orderNo.qty)
                        {
                            orderNo.green = false;
                            route.green = false;
                            seqT.green = false;
                            outData.green = false;
                        }
                        route.ordCount = 1;
                        seqT.routes.Add(route);
                        seqT.ordCount++;
                        outData.ordCount++;
                    }
                }
                else
                {
                    if (orderNo.qeol != orderNo.qty || orderNo.qc != orderNo.qty)
                    {
                        orderNo.green = false;
                        route.green = false;
                        seq.green = false;
                        outData.green = false;
                    }
                    route.ordCount = 1;
                    seq.ordCount = 1;
                    outData.sequences.Add(seq);
                    outData.ordCount++;
                }
            }
            return outData;
        }

        public List<string> GetHourly()
        {
            List<string> totals = new List<string>();
            if (Connect())
            {
                try
                {
                    SqlDataReader myReader = null;
                    SqlCommand myCommand = new SqlCommand("SELECT DISTINCT [HOUR], [UNITS] FROM [_DAVE_ENDSCAN2]", conn);
                    using (myReader = myCommand.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            string hour = SafeGetString(myReader, "HOUR");
                            int units = SafeGetInt(myReader, "UNITS");
                            string temp = hour.TrimStart() + "  " + units.ToString();
                            totals.Add(temp);
                        }
                    }
                    myReader.Close();
                    conn.Close();
                    return totals;
                }
                catch (Exception e)
                {
                    string error = e.ToString();
                    conn.Close();
                    return totals;
                }
            }
            return totals;
        }

        public Target GetTargets(string date)
        {
            Target target = new Target();
            if(Connect())
            {
                try
                {
                    SqlDataReader myReader = null;
                    SqlCommand myCommand = new SqlCommand("SELECT DISTINCT [DATE], [7AM], [8AM], [9AM], [10AM], [11AM], [12PM], [1PM], [2PM], [3PM], [4PM] FROM [PROD_LINE_TARGETS] WHERE [DATE] = '" + date + "'", conn);
                    using (myReader = myCommand.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            target.eight = SafeGetInt(myReader, "8AM");
                            target.nine = SafeGetInt(myReader, "9AM");
                            target.ten = SafeGetInt(myReader, "10AM");
                            target.eleven = SafeGetInt(myReader, "11AM");
                            target.twelve = SafeGetInt(myReader, "12PM");
                            target.one = SafeGetInt(myReader, "1PM");
                            target.two = SafeGetInt(myReader, "2PM");
                            target.three = SafeGetInt(myReader, "3PM");
                            target.four = SafeGetInt(myReader, "4PM");
                        }
                    }
                    myReader.Close();
                    conn.Close();
                    return target;
                }
                catch (Exception e)
                {
                    string error = e.ToString();
                    conn.Close();
                    return target;
                }
            }
            return target;
        }

        public string SafeGetString(SqlDataReader reader, string ord)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(ord)))
                return reader.GetString(reader.GetOrdinal(ord)).Trim();
            else
                return "-";
        }

        public int SafeGetInt(SqlDataReader reader, string ord)
        {
            float temp = reader.IsDBNull(reader.GetOrdinal(ord)) ? default(float) : float.Parse(reader[ord].ToString());
            return (int)temp;
        }

        public int GetHourInt(string hour)
        {
            switch(hour)
            {
                case "0AM":
                    return 0;
                case "1AM":
                    return 1;
                case "2AM":
                    return 2;
                case "3AM":
                    return 3;
                case "4AM":
                    return 4;
                case "5AM":
                    return 5;
                case "6AM":
                    return 6;
                case "7AM":
                    return 7;
                case "8AM":
                    return 8;
                case "9AM":
                    return 9;
                case "10AM":
                    return 10;
                case "11AM":
                    return 11;
                case "12PM":
                    return 12;
                case "1PM":
                    return 13;
                case "2PM":
                    return 14;
                case "3PM":
                    return 15;
                case "4PM":
                    return 16;
                case "5PM":
                    return 17;
                case "6PM":
                    return 18;
                case "7PM":
                    return 19;
                case "8PM":
                    return 20;
                case "9PM":
                    return 21;
                case "10PM":
                    return 22;
                case "11PM":
                    return 23;
                default:
                    return 0;
            }
        }
    }
}
