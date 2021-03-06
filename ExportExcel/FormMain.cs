﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading; 

namespace ExportExcel
{
    public partial class FormMain : Form
    {
        public EnergyData mEnergyData;
        
        public FormMain()
        {
            InitializeComponent();
            this.label3.Text = ExportExcel.Properties.Resources.Version;
            CheckForIllegalCrossThreadCalls = false;

            if ((Myutility.GetMajorVersionNumber() == "V1.1")
                || (Myutility.GetMajorVersionNumber() == "V1.3"))
            {
                labelNumber.Visible = true;
                textBoxNumber.Visible = true;
                label1.Visible = true;
                textBox_V_type.Visible = true;
            }
        }

        public void SetEnergyDataFromFile(String filename)
        {
            mEnergyData = new EnergyData(filename);
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show("Exception Occured while releasing object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }
        
        public void setExportExcelStatus(string status)
        {
            bool enable = true;
            if (status.Equals("unknown-data"))
            {
                MessageBox.Show("请先导入正确的数据文件！");
                enable = true;
            }
            else if (status.Equals("exporting"))
            {
                buttonExportExcel.Text = "导出...";
                enable = false;
            }
            else if (status.Equals("export-success"))
            {
                MessageBox.Show("导出成功！");
                buttonExportExcel.Text = "导出为Excel";
                enable = true;
            }
            else if (status.Equals("export-fail"))
            {
                buttonExportExcel.Text = "导出为Excel";
                enable = true;
            }

            if (enable == true)
            {
                buttonExportExcel.Text = "导出为Excel";
                buttonExportExcel.Enabled = true;
            }
            else {
                buttonExportExcel.Enabled = false;
            }
        }
        private string GetExcelFileNameV1_2()
        {
            string ret;
            string carType = "";
            if (mEnergyData.carType[0] == 0x01)
            {
                carType = "CRH1A";
            }
            else if (mEnergyData.carType[0] == 0x02)
            {
                carType = "CRH1E";
            }
            else if (mEnergyData.carType[0] == 0x03)
            {
                carType = "CRH380D";
            }
            else
            {
                MessageBox.Show("未识别的车型");
                return null;
            }

            UInt16 num = System.BitConverter.ToUInt16(mEnergyData.carNum, 0);
            string carNum = num.ToString();
            string pre = System.Environment.CurrentDirectory + "\\";
            ret = pre + carType + "-" + carNum + "_" + DateTime.Now.ToString("yyyyMMdd");
            return ret;
        }
        private void buttonExportExcel_Click(object sender, EventArgs e)
        {
            if (null == mEnergyData) {
                MessageBox.Show("请先导入数据文件");
                return;
            }
            setExportExcelStatus("exporting");

            ExportExcelThread mExportExcelThread;
            //mExportExcelThread = new ExportExcelThread(this, mEnergyData, GetExcelFileName(textBoxNumber.Text));
            // V1.2
            string filename = null;
            if ((Myutility.GetMajorVersionNumber() == "V1.1")
                || (Myutility.GetMajorVersionNumber() == "V1.3"))
            {
                filename = GetExcelFileName(textBoxNumber.Text);
            } else {
                filename = GetExcelFileNameV1_2();
            }
            if (null == filename)
            {
                MessageBox.Show("无效文件名");
                setExportExcelStatus("export-fail");
                return;
            }
            mExportExcelThread = new ExportExcelThread(this, mEnergyData, filename);
            Thread th = new Thread(mExportExcelThread.ThreadMethod);

            th.Start();
        }

        public String GetExcelFileName(String append)
        {
            if (append.Equals("")) {
                append = "xxxx";
            }
            String curDate = DateTime.Now.ToString("yyyyMMdd");
            String ret = System.Environment.CurrentDirectory
                + "\\" + textBox_V_type.Text + "-" + append + "_" + curDate;

            return ret;
        }

        private void labelTitle_Click(object sender, EventArgs e)
        {
            ;
        }

        private void buttonSelect_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.Filter = "数据文件(*.txt)|*.txt|所有文件(*.*)|*.*";
            this.openFileDialog1.FileName = "电能列表2016-03-02.TXT";
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string FileName = this.openFileDialog1.FileName;
                SetEnergyDataFromFile(FileName);
                this.dataGridViewEnergy.RowCount = mEnergyData.mEnergyDataRawList.Count;
                if (this.dataGridViewEnergy.RowCount == 0)
                {
                    MessageBox.Show("数据文件内容为空");
                    return;
                }
                for (int j = 0,i = this.dataGridViewEnergy.RowCount - 1; i >= 0; i--)
                {
                    string v0_0, v0_1, v0_2, v0_3, v1, v2, v3, v4, v5, v6;

                    v0_0 = mEnergyData.mEnergyDataRawList[i].year[0].ToString();
                    v0_1 = mEnergyData.mEnergyDataRawList[i].year[1].ToString();
                    v0_2 = Int32.Parse(BitConverter.ToString(mEnergyData.mEnergyDataRawList[i].mouth), System.Globalization.NumberStyles.HexNumber).ToString();
                    v0_3 = Int32.Parse(BitConverter.ToString(mEnergyData.mEnergyDataRawList[i].day), System.Globalization.NumberStyles.HexNumber).ToString();
                    
                    v1 = BitConverter.ToUInt32(Myutility.ToHostEndian(mEnergyData.mEnergyDataRawList[i].power1), 0).ToString();
                    v2 = BitConverter.ToUInt32(Myutility.ToHostEndian(mEnergyData.mEnergyDataRawList[i].power2), 0).ToString();
                    v3 = BitConverter.ToUInt32(Myutility.ToHostEndian(mEnergyData.mEnergyDataRawList[i].powerAll), 0).ToString();
                    if (j == this.dataGridViewEnergy.RowCount - 1)
                    {
                        v4 = "0";
                        v5 = "0";
                        v6 = "0";
                    }
                    else
                    {
                        v4 = (BitConverter.ToUInt32(Myutility.ToHostEndian(mEnergyData.mEnergyDataRawList[i].power1), 0)
                            - BitConverter.ToUInt32(Myutility.ToHostEndian(mEnergyData.mEnergyDataRawList[i - 1].power1), 0)).ToString();
                        v5 = (BitConverter.ToUInt32(Myutility.ToHostEndian(mEnergyData.mEnergyDataRawList[i].power2), 0)
                            - BitConverter.ToUInt32(Myutility.ToHostEndian(mEnergyData.mEnergyDataRawList[i - 1].power2), 0)).ToString();
                        v6 = (BitConverter.ToUInt32(Myutility.ToHostEndian(mEnergyData.mEnergyDataRawList[i].powerAll), 0)
                            - BitConverter.ToUInt32(Myutility.ToHostEndian(mEnergyData.mEnergyDataRawList[i - 1].powerAll), 0)).ToString();
                    }

                    dataGridViewEnergy.Rows[j].Cells[0].Value = v0_0 + "年" +　v0_1 + "月" + v0_2 + "日";
                    if (Myutility.GetMajorVersionNumber() == "V1.3")
                    {
                        // 验证有效性： 大于31或者小于等于0；时：大于等于24；分大于等于60；秒大于等于60；就丢弃这16个字节。
                        Int32 day = Int32.Parse(v0_0);
                        Int32 hour = Int32.Parse(v0_1);
                        Int32 minuts = Int32.Parse(v0_2);
                        Int32 second = Int32.Parse(v0_3);
                        
                        if (!(Myutility.InInt32Scope(day, 1, 31)
                            && Myutility.InInt32Scope(hour, 0, 23)
                            && Myutility.InInt32Scope(minuts, 0, 59)
                            && Myutility.InInt32Scope(second, 0, 59)))
                        {
                            Console.WriteLine("无效数据" + v0_0 + "日" + v0_1 + "时" + v0_2 + "分" + v0_3 + "秒");
                            continue;
                        }
                        dataGridViewEnergy.Rows[j].Cells[0].Value = v0_0 + "日" + v0_1 + "时" + v0_2 + "分" + v0_3 + "秒";
                    }
                    dataGridViewEnergy.Rows[j].Cells[1].Value = v1 + " kW.h";
                    dataGridViewEnergy.Rows[j].Cells[2].Value = v2 + " kW.h";
                    dataGridViewEnergy.Rows[j].Cells[3].Value = v3 + " kW.h";
                    dataGridViewEnergy.Rows[j].Cells[4].Value = v4 + " kW.h";
                    dataGridViewEnergy.Rows[j].Cells[5].Value = v5 + " kW.h";
                    dataGridViewEnergy.Rows[j].Cells[6].Value = v6 + " kW.h";
                    j++;
                 }
            }
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            dataGridViewEnergy.Font = new Font("Arial",9);
        }

        private void dataGridViewEnergy_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                e.CellStyle.Font = new Font("微软雅黑",9);  
                return;
            }

            try
            {
                if (e.ColumnIndex == 0)//定位到第1列日期 
                {
                    e.CellStyle.Font = new Font("微软雅黑",9);  
                }
            }
            catch
            {

            }  
        }


    }
}
