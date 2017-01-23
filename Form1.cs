using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProductionLineTV
{
    public partial class Form1 : Form
    {
        SQLQuery sqlQuery = new SQLQuery();

        Date[] pages;

        Date data = new Date();

        DateTimePicker datPicker = new DateTimePicker();

        List<DataLine> fulldata = new List<DataLine>();

        List<DataLine>[] listPages;

        Panel content = new Panel();

        Label lblOrderedTotal = new Label();
        Label lblQCTotal = new Label();
        Label lblQCPer = new Label();
        Label lblEOLTotal = new Label();
        Label lblEOLPer = new Label();
        Label dateTot = new Label();
        Label lblPage = new Label();

        DateTime lastUpdate = new DateTime();
        DateTime currDay = new DateTime();
        DateTime actualDay = new DateTime();

        int rowCount = 0;
        int pageCount = 0;
        int yCount = 0;
        int currentPage = 0;
        int orderedTotal, qcTotal, eolTotal;
        double qcPer, eolPer;

        bool secondScreen = false;

        int[] hourTotals = new int[24];

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Height = Screen.FromControl(this).Bounds.Height;
            this.Width = Screen.FromControl(this).Bounds.Width;

            rowCount = ((this.Height - 180) / 50) - 1;

            currDay = DateTime.Today;
            actualDay = DateTime.Today;

            SetPages(currDay);
            CreatePage();
            DrawPage();
            FullScreen();

            timer1.Start();
            lastUpdate = DateTime.Now;
        }

        private bool CheckScreens()
        {
            return Screen.AllScreens.Length > 1;
        }

        private void SetPages(DateTime date)
        {
            fulldata = sqlQuery.GetDate(date.ToString("MM/dd/yyyy"));

            data = sqlQuery.ConstructData(fulldata);
            List<DataLine> tempList = new List<DataLine>();
            foreach(Sequence s in data.sequences)
            {
                foreach(Route r in s.routes)
                {
                    foreach(OrderNo o in r.orderNos)
                    {
                        DataLine temp = new DataLine();
                        temp.date = data.name;
                        temp.sequence = s.name;
                        temp.route = r.name;
                        temp.orderNo = o.name;
                        temp.orderQty = o.qty;
                        temp.qcCount = o.qc;
                        temp.eolCount = o.qeol;
                        tempList.Add(temp);
                    }
                }
            }

            int totalRows = tempList.Count;
            pageCount = totalRows / rowCount;
            if ((totalRows % rowCount) != 0)
                pageCount++;
            pages = new Date[pageCount];

            listPages = new List<DataLine>[pageCount];
            for (int c = 0; c < pageCount; c++)
            {
                listPages[c] = new List<DataLine>();
            }
            int pageCounter = 0;
            int rowCounter = 0;
            for(int a = 0; a < totalRows; a++)
            {
                listPages[pageCounter].Add(tempList.ElementAt(a));
                rowCounter++;
                if (rowCounter == rowCount)
                {
                    pageCounter++;
                    rowCounter = 0;
                }
            }
        }

        private void CreatePage()
        {
            PictureBox picB = new PictureBox();
            picB.Image = Properties.Resources.richmond;
            picB.SizeMode = PictureBoxSizeMode.AutoSize;
            picB.Location = new Point(10, 0);
            picB.Click += new EventHandler(PicB_Click);
            this.Controls.Add(picB);
            yCount = 100;

            Label lblDate = new Label();
            lblDate.Size = new Size((int)(this.Width / 9.6), 80);
            lblDate.Font = new Font(lblDate.Font.FontFamily, 20, FontStyle.Bold);
            lblDate.Text = "Date";
            lblDate.TextAlign = ContentAlignment.MiddleCenter;
            lblDate.Location = new Point(0, yCount);
            this.Controls.Add(lblDate);

            Label lblSeq = new Label();
            lblSeq.Size = lblDate.Size;
            lblSeq.Font = lblDate.Font;
            lblSeq.Text = "Sequence";
            lblSeq.TextAlign = ContentAlignment.MiddleCenter;
            lblSeq.Location = new Point(lblDate.Width, yCount);
            this.Controls.Add(lblSeq);

            Label lblRou = new Label();
            lblRou.Size = lblDate.Size;
            lblRou.Font = lblDate.Font;
            lblRou.Text = "Route";
            lblRou.TextAlign = lblDate.TextAlign;
            lblRou.Location = new Point(lblDate.Width + lblSeq.Location.X, yCount);
            this.Controls.Add(lblRou);

            Label lblOrd = new Label();
            lblOrd.Size = lblDate.Size;
            lblOrd.Font = lblDate.Font;
            lblOrd.Text = "Order No";
            lblOrd.TextAlign = lblDate.TextAlign;
            lblOrd.Location = new Point(lblDate.Width + lblRou.Location.X, yCount);
            this.Controls.Add(lblOrd);

            Label lblQty = new Label();
            lblQty.Size = new Size(lblDate.Size.Width / 2, lblDate.Size.Height);
            lblQty.Font = lblDate.Font;
            lblQty.Text = "Qty";
            lblQty.TextAlign = lblDate.TextAlign;
            lblQty.Location = new Point(lblDate.Width + lblOrd.Location.X, yCount);
            this.Controls.Add(lblQty);

            Label lblQC = new Label();
            lblQC.Size = lblQty.Size;
            lblQC.Font = lblDate.Font;
            lblQC.Text = "QC";
            lblQC.TextAlign = lblDate.TextAlign;
            lblQC.Location = new Point(lblQty.Width + lblQty.Location.X, yCount);
            this.Controls.Add(lblQC);

            Label lblEOL = new Label();
            lblEOL.Size = lblQty.Size;
            lblEOL.Font = lblDate.Font;
            lblEOL.Text = "QC%";
            lblEOL.TextAlign = lblDate.TextAlign;
            lblEOL.Location = new Point(lblQty.Width + lblQC.Location.X, yCount);
            this.Controls.Add(lblEOL);

            Label lblQCP = new Label();
            lblQCP.Size = lblQty.Size;
            lblQCP.Font = lblDate.Font;
            lblQCP.Text = "EOL";
            lblQCP.TextAlign = lblDate.TextAlign;
            lblQCP.Location = new Point(lblQty.Width + lblEOL.Location.X, yCount);
            this.Controls.Add(lblQCP);

            Label lblEOLP = new Label();
            lblEOLP.Size = lblQty.Size;
            lblEOLP.Font = lblDate.Font;
            lblEOLP.Text = "EOL%";
            lblEOLP.TextAlign = lblDate.TextAlign;
            lblEOLP.Location = new Point(lblQty.Width + lblQCP.Location.X, yCount);
            this.Controls.Add(lblEOLP);

            yCount += lblDate.Height;

            content.Size = new Size((int)this.Width, rowCount * 50);
            content.Location = new Point(0, yCount);
            this.Controls.Add(content);

            Panel panTDate = new Panel();
            panTDate.Size = new Size(lblDate.Width * 4, 50);
            panTDate.Location = new Point(0, this.Height - 50);
            panTDate.BorderStyle = BorderStyle.Fixed3D;
            this.Controls.Add(panTDate);

            dateTot = new Label();
            dateTot.Size = new Size(lblDate.Width * 4, 50);
            dateTot.Font = lblDate.Font;
            dateTot.Text = currDay.ToShortDateString() + "  -  Totals";
            dateTot.TextAlign = lblDate.TextAlign;
            dateTot.Location = new Point(0, 0);
            dateTot.BorderStyle = BorderStyle.Fixed3D;
            panTDate.Controls.Add(dateTot);

            Panel panTTotal = new Panel();
            panTTotal.Size = new Size(lblDate.Width / 2, 50);
            panTTotal.Location = new Point(panTDate.Width, panTDate.Location.Y);
            panTTotal.BorderStyle = BorderStyle.Fixed3D;
            this.Controls.Add(panTTotal);

            lblOrderedTotal.Size = new Size(panTTotal.Width, 50);
            lblOrderedTotal.Font = lblDate.Font;
            lblOrderedTotal.Text = orderedTotal.ToString();
            lblOrderedTotal.TextAlign = lblDate.TextAlign;
            lblOrderedTotal.Location = new Point(0, 0);
            lblOrderedTotal.BorderStyle = BorderStyle.Fixed3D;
            panTTotal.Controls.Add(lblOrderedTotal);

            Panel panTQC = new Panel();
            panTQC.Size = new Size(panTTotal.Width, 50);
            panTQC.Location = new Point(panTTotal.Width + panTTotal.Location.X, panTDate.Location.Y);
            panTQC.BorderStyle = BorderStyle.Fixed3D;
            this.Controls.Add(panTQC);

            lblQCTotal.Size = new Size(panTTotal.Width, 50);
            lblQCTotal.Font = lblDate.Font;
            lblQCTotal.Text = qcTotal.ToString();
            lblQCTotal.TextAlign = lblDate.TextAlign;
            lblQCTotal.Location = new Point(0, 0);
            lblQCTotal.BorderStyle = BorderStyle.Fixed3D;
            panTQC.Controls.Add(lblQCTotal);

            Panel panTQCPer = new Panel();
            panTQCPer.Size = new Size(panTTotal.Width, 50);
            panTQCPer.Location = new Point(panTQC.Width + panTQC.Location.X, panTDate.Location.Y);
            panTQCPer.BorderStyle = BorderStyle.Fixed3D;
            this.Controls.Add(panTQCPer);

            lblQCPer.Size = new Size(panTTotal.Width, 50);
            lblQCPer.Font = lblDate.Font;
            lblQCPer.Text = qcPer.ToString("0%");
            lblQCPer.TextAlign = lblDate.TextAlign;
            lblQCPer.Location = new Point(0, 0);
            lblQCPer.BorderStyle = BorderStyle.Fixed3D;
            panTQCPer.Controls.Add(lblQCPer);

            Panel panTEOL = new Panel();
            panTEOL.Size = new Size(panTTotal.Width, 50);
            panTEOL.Location = new Point(panTQCPer.Width + panTQCPer.Location.X, panTDate.Location.Y);
            panTEOL.BorderStyle = BorderStyle.Fixed3D;
            this.Controls.Add(panTEOL);

            lblEOLTotal.Size = new Size(panTTotal.Width, 50);
            lblEOLTotal.Font = lblDate.Font;
            lblEOLTotal.Text = eolTotal.ToString();
            lblEOLTotal.TextAlign = lblDate.TextAlign;
            lblEOLTotal.Location = new Point(0, 0);
            lblEOLTotal.BorderStyle = BorderStyle.Fixed3D;
            panTEOL.Controls.Add(lblEOLTotal);

            Panel panTEOLPer = new Panel();
            panTEOLPer.Size = new Size(panTTotal.Width, 50);
            panTEOLPer.Location = new Point(panTEOL.Width + panTEOL.Location.X, panTDate.Location.Y);
            panTEOLPer.BorderStyle = BorderStyle.Fixed3D;
            this.Controls.Add(panTEOLPer);

            lblEOLPer.Size = new Size(panTTotal.Width, 50);
            lblEOLPer.Font = lblDate.Font;
            lblEOLPer.Text = eolPer.ToString("0%");
            lblEOLPer.TextAlign = lblDate.TextAlign;
            lblEOLPer.Location = new Point(0, 0);
            lblEOLPer.BorderStyle = BorderStyle.Fixed3D;
            panTEOLPer.Controls.Add(lblEOLPer);

            datPicker = new DateTimePicker();
            datPicker.Size = new Size(500, 68);
            datPicker.Font = new Font(datPicker.Font.FontFamily, 40, FontStyle.Bold);
            datPicker.Location = new Point(233, 12);
            datPicker.ValueChanged += new EventHandler(DatPicker_ValueChanged);
            this.Controls.Add(datPicker);

            lblPage = new Label();
            lblPage.Size = datPicker.Size;
            lblPage.Font = datPicker.Font;
            lblPage.Location = new Point(datPicker.Width + datPicker.Location.X + 20, 12);
            lblPage.Text = "Page: " + (currentPage + 1) + "/" + pageCount;
            lblPage.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblPage);
        }

        private void DatPicker_ValueChanged(object sender, EventArgs e)
        {
            currentPage = 0;
            currDay = datPicker.Value;
            SetPages(currDay);
            DrawPage();
            dateTot.Text = currDay.ToShortDateString() + "  -  Totals";
            lastUpdate = DateTime.Now;
        }

        private void DrawPage()
        {
            if (pageCount == 0)
                return;

            Date currPage = sqlQuery.ConstructData(listPages[currentPage]);
            content.Controls.Clear();

            dateTot.Text = currDay.ToShortDateString() + "  -  Totals";

            yCount = 0;

            Panel panD = new Panel();
            panD.Size = new Size((int)(this.Width / 9.6), currPage.ordCount * 50);
            panD.Location = new Point(0, yCount);
            panD.BorderStyle = BorderStyle.Fixed3D;
            content.Controls.Add(panD);

            Label lblD = new Label();
            lblD.Size = panD.Size;
            lblD.Font = new Font(lblD.Font.FontFamily, 20, FontStyle.Bold);
            lblD.Text = currPage.name;
            lblD.TextAlign = ContentAlignment.MiddleCenter;
            lblD.Location = new Point(0, 0);
            if (currPage.green)
                lblD.BackColor = Color.Green;
            panD.Controls.Add(lblD);
            foreach (Sequence s in currPage.sequences)
            {
                Panel panS = new Panel();
                panS.Size = new Size(panD.Width, s.ordCount * 50);
                panS.Location = new Point(panD.Width, yCount);
                panS.BorderStyle = BorderStyle.Fixed3D;
                content.Controls.Add(panS);

                Label lblS = new Label();
                lblS.Size = new Size(panS.Width, panS.Height);
                lblS.Font = new Font(lblS.Font.FontFamily, 20, FontStyle.Bold);
                lblS.Text = s.name;
                lblS.TextAlign = ContentAlignment.MiddleCenter;
                lblS.Location = new Point(0, 0);
                if (s.green)
                    lblS.BackColor = Color.Green;
                panS.Controls.Add(lblS);

                foreach (Route r in s.routes)
                {
                    Panel panR = new Panel();
                    panR.Size = new Size(panD.Width, r.ordCount * 50);
                    panR.Location = new Point(panD.Width + panS.Width, yCount);
                    panR.BorderStyle = BorderStyle.Fixed3D;
                    content.Controls.Add(panR);

                    Label lblR = new Label();
                    lblR.Size = new Size(panR.Width, panR.Height);
                    lblR.Font = new Font(lblR.Font.FontFamily, 20, FontStyle.Bold);
                    lblR.Text = r.name;
                    lblR.TextAlign = ContentAlignment.MiddleCenter;
                    lblR.Location = new Point(0, 0);
                    if (r.green)
                        lblR.BackColor = Color.Green;
                    panR.Controls.Add(lblR);

                    foreach (OrderNo o in r.orderNos)
                    {
                        Panel panO = new Panel();
                        panO.Size = new Size(panD.Width, 50);
                        panO.Location = new Point(panR.Location.X + panR.Width, yCount);
                        panO.BorderStyle = BorderStyle.Fixed3D;
                        content.Controls.Add(panO);

                        Label lblO = new Label();
                        lblO.Size = new Size(panO.Width, panO.Height);
                        lblO.Font = new Font(lblO.Font.FontFamily, 20, FontStyle.Bold);
                        lblO.Text = o.name;
                        lblO.TextAlign = ContentAlignment.MiddleCenter;
                        lblO.Location = new Point(0, 0);
                        if (o.green)
                            lblO.BackColor = Color.Green;
                        panO.Controls.Add(lblO);

                        Panel panO1 = new Panel();
                        panO1.Size = new Size(panD.Width / 2, 50);
                        panO1.Location = new Point(panO.Location.X + panO.Width, yCount);
                        panO1.BorderStyle = BorderStyle.Fixed3D;
                        content.Controls.Add(panO1);

                        Label lblO1 = new Label();
                        lblO1.Size = new Size(panO1.Width, panO1.Height);
                        lblO1.Font = new Font(lblO1.Font.FontFamily, 20, FontStyle.Bold);
                        lblO1.Text = o.qty.ToString();
                        lblO1.TextAlign = ContentAlignment.MiddleCenter;
                        lblO1.Location = new Point(0, 0);
                        if (o.green)
                            lblO1.BackColor = Color.Green;
                        panO1.Controls.Add(lblO1);

                        Panel panO2 = new Panel();
                        panO2.Size = new Size(panO1.Width, 50);
                        panO2.Location = new Point(panO1.Location.X + panO1.Width, yCount);
                        panO2.BorderStyle = BorderStyle.Fixed3D;
                        content.Controls.Add(panO2);

                        Label lblO2 = new Label();
                        lblO2.Size = new Size(panO2.Width, panO2.Height);
                        lblO2.Font = new Font(lblO2.Font.FontFamily, 20, FontStyle.Bold);
                        lblO2.Text = o.qc.ToString();
                        lblO2.TextAlign = ContentAlignment.MiddleCenter;
                        lblO2.Location = new Point(0, 0);


                        Panel panO3 = new Panel();
                        panO3.Size = new Size(panO1.Width, 50);
                        panO3.Location = new Point(panO2.Location.X + panO2.Width, yCount);
                        panO3.BorderStyle = BorderStyle.Fixed3D;
                        content.Controls.Add(panO3);

                        Label lblO3 = new Label();
                        lblO3.Size = new Size(panO3.Width, panO3.Height);
                        lblO3.Font = new Font(lblO3.Font.FontFamily, 20, FontStyle.Bold);
                        lblO3.Text = o.qeol.ToString();
                        lblO3.TextAlign = ContentAlignment.MiddleCenter;
                        lblO3.Location = new Point(0, 0);


                        Panel panO4 = new Panel();
                        panO4.Size = new Size(panO1.Width, 50);
                        panO4.Location = new Point(panO3.Location.X + panO3.Width, yCount);
                        panO4.BorderStyle = BorderStyle.Fixed3D;
                        content.Controls.Add(panO4);

                        Label lblO4 = new Label();
                        lblO4.Size = new Size(panO4.Width, panO4.Height);
                        lblO4.Font = new Font(lblO4.Font.FontFamily, 20, FontStyle.Bold);
                        double perc = CalcTargetPer(o.qty, o.qc);
                        if (perc <= 0.24)
                        {
                            lblO2.BackColor = Color.Red;
                            lblO4.BackColor = Color.Red;
                        }
                        else if (perc >= 0.25 && perc <= 0.74)
                        {
                            lblO2.BackColor = Color.Orange;
                            lblO4.BackColor = Color.Orange;
                        }
                        else if (perc >= 0.75 && perc != 1.00)
                        {
                            lblO2.BackColor = Color.Yellow;
                            lblO4.BackColor = Color.Yellow;
                        }
                        else if (perc == 1.00)
                        {
                            lblO2.BackColor = Color.Green;
                            lblO4.BackColor = Color.Green;
                        }
                        lblO4.Text = perc.ToString("0%");
                        lblO4.TextAlign = ContentAlignment.MiddleCenter;
                        lblO4.Location = new Point(0, 0);


                        Panel panO5 = new Panel();
                        panO5.Size = new Size(panO1.Width, 50);
                        panO5.Location = new Point(panO4.Location.X + panO4.Width, yCount);
                        panO5.BorderStyle = BorderStyle.Fixed3D;
                        content.Controls.Add(panO5);

                        Label lblO5 = new Label();
                        lblO5.Size = new Size(panO5.Width, panO5.Height);
                        lblO5.Font = new Font(lblO5.Font.FontFamily, 20, FontStyle.Bold);
                        perc = CalcTargetPer(o.qty, o.qeol); 
                        if (perc <= 0.33)
                        {
                            lblO5.BackColor = Color.Red;
                            lblO3.BackColor = Color.Red;
                        }
                        else if (perc >= 0.34 && perc <= 0.66)
                        {
                            lblO5.BackColor = Color.Orange;
                            lblO3.BackColor = Color.Orange;
                        }
                        else if (perc >= 0.67 && perc != 1.00)
                        {
                            lblO5.BackColor = Color.Yellow;
                            lblO3.BackColor = Color.Yellow;
                        }
                        else if (perc == 1.00)
                        {
                            lblO5.BackColor = Color.Green;
                            lblO3.BackColor = Color.Green;
                        }
                        lblO5.Text = perc.ToString("0%");
                        lblO5.TextAlign = ContentAlignment.MiddleCenter;
                        lblO5.Location = new Point(0, 0);

                        yCount += panO.Height;

                        panO2.Controls.Add(lblO2);
                        panO3.Controls.Add(lblO4);
                        panO4.Controls.Add(lblO3);
                        panO5.Controls.Add(lblO5);
                    }
                }
            }
            CalcTotals();
            GetHourly();
            lblPage.Text = "Page: " + (currentPage + 1) + "/" + pageCount;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(actualDay.Day != DateTime.Now.Day)
            {
                currentPage = 0;
                ResetHourly();
                SetPages(DateTime.Now);
                DrawPage();
                lastUpdate = DateTime.Now;
                actualDay = DateTime.Now;
                datPicker.Text = DateTime.Now.ToShortDateString();
                return;
            }
            if(DateTime.Now >= lastUpdate.AddSeconds(15))
            {
                if (pageCount != 0)
                {
                    if (currentPage < pageCount - 1)
                    {
                        currentPage++;
                        DrawPage();
                        lastUpdate = DateTime.Now;
                    }
                    else
                    {
                        currentPage = 0;
                        DateTime teD = sqlQuery.GetLastDate();
                        if (teD != new DateTime())
                            currDay = teD;
                        SetPages(currDay);
                        DrawPage();
                        lastUpdate = DateTime.Now;
                    }
                }
                else
                {
                    currentPage = 0;
                    SetPages(currDay);
                    DrawPage();
                    lastUpdate = DateTime.Now;
                }
            }
        }

        private void ResetHourly()
        {
            lbl8am.Visible = false;
            lbl9am.Visible = false;
            lbl10am.Visible = false;
            lbl11am.Visible = false;
            lbl12pm.Visible = false;
            lbl1pm.Visible = false;
            lbl2pm.Visible = false;
            lbl3pm.Visible = false;
            lbl4pm.Visible = false;

            lbl8Total.Visible = false;
            lbl9Total.Visible = false;
            lbl10Total.Visible = false;
            lbl11Total.Visible = false;
            lbl12Total.Visible = false;
            lbl1Total.Visible = false;
            lbl2Total.Visible = false;
            lbl3Total.Visible = false;
            lbl4Total.Visible = false;

            lbl8Target.Visible = false;
            lbl9Target.Visible = false;
            lbl10Target.Visible = false;
            lbl11Target.Visible = false;
            lbl12Target.Visible = false;
            lbl1Target.Visible = false;
            lbl2Target.Visible = false;
            lbl3Target.Visible = false;
            lbl4Target.Visible = false;

            lbl8Per.Visible = false;
            lbl9Per.Visible = false;
            lbl10Per.Visible = false;
            lbl11Per.Visible = false;
            lbl12Per.Visible = false;
            lbl1Per.Visible = false;
            lbl2Per.Visible = false;
            lbl3Per.Visible = false;
            lbl4Per.Visible = false;

            panDTotal.Location = new Point(1360, 330);
        }

        private void PicB_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                TopMost = true;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                TopMost = false;
                WindowState = FormWindowState.Normal;
                FormBorderStyle = FormBorderStyle.Sizable;
            }
        }

        private void CalcTotals()
        {
            orderedTotal = 0;
            qcTotal = 0;
            eolTotal = 0;
            qcPer = 0.0;
            eolPer = 0.0;
            if (data.sequences.Count == 0)
                return;
            foreach (Sequence s in data.sequences)
            {
                foreach (Route r in s.routes)
                {
                    foreach (OrderNo o in r.orderNos)
                    {
                        eolTotal += o.qeol;
                        qcTotal += o.qc;
                        orderedTotal += o.qty;
                    }
                }
            }
            qcPer = CalcTargetPer(orderedTotal, qcTotal);
            eolPer = CalcTargetPer(orderedTotal, eolTotal);

            lblOrderedTotal.Text = orderedTotal.ToString();
            lblEOLTotal.Text = eolTotal.ToString();
            lblQCTotal.Text = qcTotal.ToString();
            lblEOLPer.Text = eolPer.ToString("0%");
            lblQCPer.Text = qcPer.ToString("0%");

            if (qcPer <= 0.33)
            {
                lblQCPer.BackColor = Color.Red;
                lblQCTotal.BackColor = Color.Red;
            }
            else if (qcPer >= 0.34 && qcPer <= 0.66)
            {
                lblQCPer.BackColor = Color.Orange;
                lblQCTotal.BackColor = Color.Orange;
            }
            else if (qcPer >= 0.67 && qcPer != 1.00)
            {
                lblQCPer.BackColor = Color.Yellow;
                lblQCTotal.BackColor = Color.Yellow;
            }
            else if (qcPer == 1.00)
            {
                lblQCPer.BackColor = Color.Green;
                lblQCTotal.BackColor = Color.Green;
            }

            if (eolPer <= 0.33)
            {
                lblEOLPer.BackColor = Color.Red;
                lblEOLTotal.BackColor = Color.Red;
            }
            else if (eolPer >= 0.34 && eolPer <= 0.66)
            {
                lblEOLPer.BackColor = Color.Orange;
                lblEOLTotal.BackColor = Color.Orange;
            }
            else if (eolPer >= 0.67 && eolPer != 1.00)
            {
                lblEOLPer.BackColor = Color.Yellow;
                lblEOLTotal.BackColor = Color.Yellow;
            }
            else if (eolPer == 1.00)
            {
                lblEOLPer.BackColor = Color.Green;
                lblEOLTotal.BackColor = Color.Green;
            }
        }

        private void FullScreen()
        {
            TopMost = true;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
        }

        private void GetHourly()
        {
            List<string> hourly = sqlQuery.GetHourly();
            hourly = MissingHour(hourly);
            Target targets = sqlQuery.GetTargets(DateTime.Now.ToString("MM/dd/yyyy"));
            if (targets.eight == 0 && targets.nine == 0 && targets.ten == 0 && targets.eleven == 0 && targets.twelve == 0 &&
                targets.one == 0 && targets.two == 0 && targets.three == 0 && targets.four == 0)
            {
                int t = DefaultTargets();
                if (DateTime.Today.DayOfWeek == DayOfWeek.Friday)
                {
                    targets.eight = targets.nine = targets.ten = targets.eleven = targets.twelve = targets.one = t;
                    targets.two = targets.three = targets.four = 0;
                }
                else
                {
                    targets.one = targets.two = targets.three = targets.eight = targets.nine = targets.ten = targets.eleven = t;
                    targets.twelve = targets.four = (t / 2);
                }
            }
            panDTotal.Location = new Point(1360, 330);
            int runningTotal = 0;
            foreach (string h in hourly)
            {
                string hour = h.Substring(0, 5);
                hour = hour.Trim();
                string total = h.Substring(5);
                total = total.Trim();
                runningTotal += Int32.Parse(total);
                switch (hour)
                {
                    /*case "7AM":
                        DrawHour(lbl7Total, lbl7am, lbl7Target, lbl7Per, total, targets.seven.ToString());
                        break;*/
                    case "8AM":
                        DrawHour(lbl8Total, lbl8am, lbl8Target, lbl8Per, total, targets.eight.ToString());
                        break;
                    case "9AM":
                        DrawHour(lbl9Total, lbl9am, lbl9Target, lbl9Per, total, targets.nine.ToString());
                        break;
                    case "10AM":
                        DrawHour(lbl10Total, lbl10am, lbl10Target, lbl10Per, total, targets.ten.ToString());
                        break;
                    case "11AM":
                        DrawHour(lbl11Total, lbl11am, lbl11Target, lbl11Per, total, targets.eleven.ToString());
                        break;
                    case "12PM":
                        DrawHour(lbl12Total, lbl12pm, lbl12Target, lbl12Per, total, targets.twelve.ToString());
                        break;
                    case "1PM":
                        DrawHour(lbl1Total, lbl1pm, lbl1Target, lbl1Per, total, targets.one.ToString());
                        break;
                    case "2PM":
                        DrawHour(lbl2Total, lbl2pm, lbl2Target, lbl2Per, total, targets.two.ToString());
                        break;
                    case "3PM":
                        DrawHour(lbl3Total, lbl3pm, lbl3Target, lbl3Per, total, targets.three.ToString());
                        break;
                    case "4PM":
                        DrawHour(lbl4Total, lbl4pm, lbl4Target, lbl4Per, total, targets.four.ToString());
                        break;
                }
            }
            lblDailyTotal.Text = runningTotal.ToString();
            int targTotal = 0;
            if (lbl8am.Visible == true)
                targTotal += Int32.Parse(lbl8Target.Text);
            if (lbl9am.Visible == true)
                targTotal += Int32.Parse(lbl9Target.Text);
            if (lbl10am.Visible == true)
                targTotal += Int32.Parse(lbl10Target.Text);
            if (lbl11am.Visible == true)
                targTotal += Int32.Parse(lbl11Target.Text);
            if (lbl12pm.Visible == true)
                targTotal += Int32.Parse(lbl12Target.Text);
            if (lbl1pm.Visible == true)
                targTotal += Int32.Parse(lbl1Target.Text);
            if (lbl2pm.Visible == true)
                targTotal += Int32.Parse(lbl2Target.Text);
            if (lbl3pm.Visible == true)
                targTotal += Int32.Parse(lbl3Target.Text);
            if (lbl4pm.Visible == true)
                targTotal += Int32.Parse(lbl4Target.Text);
            lblDailyTarget.Text = targTotal.ToString();
            double perc = CalcTargetPer(Int32.Parse(lblDailyTarget.Text), Int32.Parse(lblDailyTotal.Text));
            lblDailyPer.Text = perc.ToString("0%");
            if (perc <= 0.33)
            {
                lblDailyTotal.BackColor = Color.Red;
                lblDailyDate.BackColor = Color.Red;
                lblDailyTarget.BackColor = Color.Red;
                lblDailyPer.BackColor = Color.Red;
            }
            else if (perc >= 0.34 && perc <= 0.66)
            {
                lblDailyTotal.BackColor = Color.Orange;
                lblDailyDate.BackColor = Color.Orange;
                lblDailyTarget.BackColor = Color.Orange;
                lblDailyPer.BackColor = Color.Orange;
            }
            else if (perc >= 0.67 && perc < 1.00)
            {
                lblDailyTotal.BackColor = Color.Yellow;
                lblDailyDate.BackColor = Color.Yellow;
                lblDailyTarget.BackColor = Color.Yellow;
                lblDailyPer.BackColor = Color.Yellow;
            }
            else if (perc >= 1.00)
            {
                lblDailyTotal.BackColor = Color.Green;
                lblDailyDate.BackColor = Color.Green;
                lblDailyTarget.BackColor = Color.Green;
                lblDailyPer.BackColor = Color.Green;
            }
            lblDailyDate.Text = DateTime.Today.ToShortDateString();
        }

        private List<string> MissingHour(List<string> hours)
        {
            List<string> newHour = new List<string>();
            List<string> missing = new List<string>();
            string[] hoursAc = new string[] {"8", "9", "10", "11", "12", "1", "2", "3", "4"};
            foreach (string h in hours)
            {
                string n;
                n = h.Substring(0, 5);
                n = n.Trim();
                n = n.Remove(n.Length - 2, 2);
                newHour.Add(n);
            }
            int hour = DateTime.Now.Hour;
            if (hour < 7)
                return hours;
            int tot = hour - 7;

            if(newHour.Count != tot)
            {
                for(int a = 0; a < tot; a++)
                {
                    if(!newHour.Contains(hoursAc[a]))
                    {
                        if(a > 4)
                        {
                            hours.Add(hoursAc[a] + "PM  0");
                        }
                        else
                        {
                            hours.Add(hoursAc[a] + "AM  0");
                        }
                    }
                       
                }
            }
            return hours;
        }

        private void DrawHour(Label hourTot, Label hour, Label hourTar, Label hourPer, string total, string target)
        {
            hour.Visible = true;
            hourPer.Visible = true;
            hourTar.Visible = true;
            hourTot.Visible = true;
            hourTot.Text = total;
            hourTar.Text = target;
            double perc = CalcTargetPer(Int32.Parse(target), Int32.Parse(total));
            hourPer.Text = perc.ToString("0%");
            panDTotal.Location = new Point(panDTotal.Location.X, panDTotal.Location.Y + 50);
            if (perc <= 0.33)
            {
                hourTot.BackColor = Color.Red;
                hour.BackColor = Color.Red;
                hourTar.BackColor = Color.Red;
                hourPer.BackColor = Color.Red;
            }
            else if (perc >= 0.34 && perc <= 0.66)
            {
                hourTot.BackColor = Color.Orange;
                hour.BackColor = Color.Orange;
                hourTar.BackColor = Color.Orange;
                hourPer.BackColor = Color.Orange;
            }
            else if (perc >= 0.67 && perc < 1.00)
            {
                hourTot.BackColor = Color.Yellow;
                hour.BackColor = Color.Yellow;
                hourTar.BackColor = Color.Yellow;
                hourPer.BackColor = Color.Yellow;
            }
            else if (perc >= 1.00)
            {
                hourTot.BackColor = Color.Green;
                hour.BackColor = Color.Green;
                hourTar.BackColor = Color.Green;
                hourPer.BackColor = Color.Green;
            }
        }

        private double CalcTargetPer(int target, int total)
        {
            double per = (double)total / (double)target;
            per = Math.Round(per, 2);
            return per;
        }

        private int DefaultTargets()
        {
            int target = 0;
            if(DateTime.Today.DayOfWeek == DayOfWeek.Friday)
            {
                target = orderedTotal / 6;
            }
            else
            {
                target = orderedTotal / 8;
            }
            return target + 1;
        }

    }
}
