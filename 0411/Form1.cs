using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.VisualBasic.PowerPacks;

namespace _0411
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        UdpClient U;
        Thread th;
        ShapeContainer C; //畫布物件(本機繪圖)
        ShapeContainer D; //畫布物件(遠端)  
        Point stP;         //繪圖起點        
        string p;         //筆畫座標字串 

        private void Form1_Load(object sender, EventArgs e)
        {
            C = new ShapeContainer(); //建立畫布(本機繪圖用)            
            this.Controls.Add(C);     //加入畫布C到表單            
            D = new ShapeContainer(); //建立畫布(遠端繪圖用)            
            this.Controls.Add(D);     //加入畫布D到表單
        }
        private void listen()
        {
            int Port = int.Parse(textBox3.Text); //設定監聽用的通訊埠            
            U = new UdpClient(Port);                        
                        
            IPEndPoint EP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port);//建立本機端點
            while (true)          
            {
                byte[] B = U.Receive(ref EP);            //訊息存到B陣列                
                string A = Encoding.Default.GetString(B);//翻譯B陣列為字串A                
                string[] Z = A.Split('_');               //切割顏色與座標資訊
                string[] Q = Z[1].Split('/');            //切割座標點資訊             
                Point[] R = new Point[Q.Length];//宣告座標點陣列        
                            

                for (int i = 0; i < Q.Length; i++)
                {
                    string[] K = Q[i].Split(',');        //切割X與Y座標                    
                    R[i].X = int.Parse(K[0]);            //定義第i點X座標                    
                    R[i].Y = int.Parse(K[1]);            //定義第i點Y座標                
                }
                for (int i = 0; i < Q.Length - 1; i++)
                {
                    LineShape L = new LineShape();       //建立線段物件                    
                    L.StartPoint = R[i];                 //線段起點                    
                    L.EndPoint = R[i + 1];               //線段終點
                    switch (Z[0])                        //筆色
                    {
                        case "1":                        //紅筆
                            L.BorderColor = Color.Red;
                            break;
                        case "2":                        //亮綠色筆
                            L.BorderColor = Color.Lime;
                            break;
                        case "3":                        //藍筆
                            L.BorderColor = Color.Blue;
                            break;
                        case "4":                        //黑筆
                            L.BorderColor = Color.Black;
                            break;
                    }
                    L.Parent = D;                        //線段L加入畫布D(遠端使用者繪圖)                
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false; 
            th = new Thread(listen) ; //建立監聽執行緒
            th.Start();              //啟動監聽執行緒  
            button1.Enabled = false; //不能重複開啟監聽 
        }
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            stP = e.Location;                              //起點            
            p = stP.X.ToString() + "," + stP.Y.ToString(); //起點座標紀錄 
        }
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                LineShape L = new LineShape();  //建立線段物件                
                L.StartPoint = stP;  //線段起點               
                L.EndPoint = e.Location;   //線段終點
                if (radioButton1.Checked) { L.BorderColor = Color.Red; }   
                if (radioButton2.Checked) { L.BorderColor = Color.Lime; }  
                if (radioButton3.Checked) { L.BorderColor = Color.Blue; }  
                if (radioButton4.Checked) { L.BorderColor = Color.Black; }  
                L.Parent = C;                                           //線段加入畫布C                
                stP = e.Location;                                         //終點變起點                
                p += "/" + stP.X.ToString() + "," + stP.Y.ToString();      //持續紀錄座標
            }
        }
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            int Port = int.Parse(textBox2.Text);  //設定發送的目標Port            
            UdpClient S = new UdpClient(textBox1.Text, Port); //建立UDP物件
            if (radioButton1.Checked) { p = "1_" + p; }       
            if (radioButton2.Checked) { p = "2_" + p; }      
            if (radioButton3.Checked) { p = "3_" + p; }       
            if (radioButton4.Checked) { p = "4_" + p; }      
            byte[] B = Encoding.Default.GetBytes(p); //翻譯p字串為B陣列            
            S.Send(B, B.Length);   //發送資料            
            S.Close(); 
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
               th.Abort();  
                U.Close();  
            }
            catch
            {
                //忽略錯誤，程式繼續執行！  
            }
        }
    }
}
