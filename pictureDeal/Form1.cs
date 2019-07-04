using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
//using System.Speech.Synthesis;
using System.Speech.Recognition;
using SpeechLib;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using MySql.Data.MySqlClient;
using System.Drawing.Drawing2D;

namespace pictureDeal
{
    //MouseWheel += new MouseEventHandler(Form1_MouseWheel);
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            this.Load += new EventHandler(Form1_Load);
        }
        Bitmap bitmap;
        //Bitmap newbitmap;
        Stopwatch sw = new Stopwatch();
        SpeechRecognitionEngine recEngine = new SpeechRecognitionEngine();
        //贴图
        private void button1_Click(object sender, EventArgs e)
        {

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = openFileDialog1.FileName;
                bitmap = (Bitmap)Image.FromFile(path);
                pictureBox1.Image = bitmap.Clone() as Image;

            }
        }
        //鼠标滚轮放大
        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            //判断上滑还是下滑
            if (e.Delta < 0)
            {
                //计算缩放大小
                this.pictureBox1.Width = this.pictureBox1.Width * 9 / 10;
                this.pictureBox1.Height = this.pictureBox1.Height * 9 / 10;
            }
            else
            {
                this.pictureBox1.Width = this.pictureBox1.Width * 11 / 10;
                this.pictureBox1.Height = this.pictureBox1.Height * 11 / 10;
            }

        }
        //private void button2_Click(object sender, EventArgs e)
        //{
        //    if (bitmap != null)
        //    {
        //        Graphics g = this.CreateGraphics();
        //        //g.TranslateTransform(200.0f, -100.0f);
        //        g.DrawImage(bitmap, 200, 0, bitmap.Width / 2, bitmap.Height / 2);
        //       // pictureBox1.Image = bitmap.Clone() as Image;
        //        // g.FillEllipse(new SolidBrush(Color.FromArgb(50, Color.Green)), 120, 30, 200, 100);//平移
        //    }
        //    }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        int xPos;
        int yPos;
        bool MoveFlag;
        //移动图片
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            //鼠标已经抬起
            MoveFlag = false;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            //只在鼠标按下时绘制移动
            if (MoveFlag)
            {
                pictureBox1.Left += Convert.ToInt16(e.X - xPos);//设置x坐标.
                pictureBox1.Top += Convert.ToInt16(e.Y - yPos);//设置y坐标.
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            this.pictureBox1.Focus();
            MoveFlag = true;//已经按下.
            xPos = e.X;//当前x坐标.
            yPos = e.Y;//当前y坐标.
        }

        //灰度
        private void button2_Click_1(object sender, EventArgs e)
        {

            if (bitmap != null)
            {
                //  newbitmap = bitmap.Clone() as Bitmap;
                sw.Reset();
                sw.Restart();
                Color pixel;
                int gray;
                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        pixel = bitmap.GetPixel(x, y);
                        gray = (int)(0.3 * pixel.R + 0.59 * pixel.G + 0.11 * pixel.B);
                        bitmap.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                    }
                }
                sw.Stop();
                // timer.Text = sw.ElapsedMilliseconds.ToString();
                pictureBox1.Image = bitmap.Clone() as Image;
            }

        }

        //边缘检测算法
        private static Bitmap smoothed(Bitmap a)
        {
            int w = a.Width;
            int h = a.Height;
            try
            {
                Bitmap dstBitmap = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                System.Drawing.Imaging.BitmapData srcData = a.LockBits(new Rectangle
                    (0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                BitmapData dstData = dstBitmap.LockBits(new Rectangle
                    (0, 0, w, h), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                unsafe
                {
                    byte* pIn = (byte*)srcData.Scan0.ToPointer();
                    byte* pOut = (byte*)dstData.Scan0.ToPointer();
                    byte* p;
                    int stride = srcData.Stride;
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            //边缘八个点像素不变
                            if (x == 0 || x == w - 1 || y == 0 || y == h - 1)
                            {
                                pOut[0] = pIn[0];
                                pOut[1] = pIn[1];
                                pOut[2] = pIn[2];

                            }
                            else
                            {
                                int r0, r1, r2, r3, r4, r5, r6, r7, r8;
                                int g1, g2, g3, g4, g5, g6, g7, g8, g0;
                                int b1, b2, b3, b4, b5, b6, b7, b8, b0;
                                double vR, vG, vB;
                                //左上
                                p = pIn - stride - 3;
                                r1 = p[2];
                                g1 = p[1];
                                b1 = p[0];
                                //正上
                                p = pIn - stride;
                                r2 = p[2];
                                g2 = p[1];
                                b2 = p[0];
                                //右上
                                p = pIn - stride + 3;
                                r3 = p[2];
                                g3 = p[1];
                                b3 = p[0];
                                //左
                                p = pIn - 3;
                                r4 = p[2];
                                g4 = p[1];
                                b4 = p[0];
                                //右
                                p = pIn + 3;
                                r5 = p[2];
                                g5 = p[1];
                                b5 = p[0];
                                //左下
                                p = pIn + stride - 3;
                                r6 = p[2];
                                g6 = p[1];
                                b6 = p[0];
                                //正下
                                p = pIn + stride;
                                r7 = p[2];
                                g7 = p[1];
                                b7 = p[0];
                                // 右下 
                                p = pIn + stride + 3;
                                r8 = p[2];
                                g8 = p[1];
                                b8 = p[0];
                                //中心点
                                p = pIn;
                                r0 = p[2];
                                g0 = p[1];
                                b0 = p[0];
                                //使用模板
                                vR = (double)(Math.Abs(r3 + r5 + r8 - r1 - r4 - r6) + Math.Abs(r1 + r2 + r3 - r6 - r7 - r8));
                                vG = (double)(Math.Abs(g3 + g5 + g8 - g1 - g4 - g6) + Math.Abs(g1 + g2 + g3 - g6 - g7 - g8));
                                vB = (double)(Math.Abs(b3 + b5 + b8 - b1 - b4 - b6) + Math.Abs(b1 + b2 + b3 - b6 - b7 - b8));
                                if (vR > 0)
                                {
                                    vR = Math.Min(255, vR);
                                }
                                else
                                {
                                    vR = Math.Max(0, vR);
                                }

                                if (vG > 0)
                                {
                                    vG = Math.Min(255, vG);
                                }
                                else
                                {
                                    vG = Math.Max(0, vG);
                                }

                                if (vB > 0)
                                {
                                    vB = Math.Min(255, vB);
                                }
                                else
                                {
                                    vB = Math.Max(0, vB);
                                }
                                pOut[0] = (byte)vB;
                                pOut[1] = (byte)vG;
                                pOut[2] = (byte)vR;

                            }
                            pIn += 3;
                            pOut += 3;
                        }
                        pIn += srcData.Stride - w * 3;
                        pOut += srcData.Stride - w * 3;

                    }
                }
                a.UnlockBits(srcData);
                dstBitmap.UnlockBits(dstData);

                return dstBitmap;
            }
            catch
            {
                return null;
            }
        }

        //边缘检测
        private void button4_Click(object sender, EventArgs e)
        {
            bitmap = smoothed(bitmap);
            pictureBox1.Image = bitmap.Clone() as Image;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Choices preCmd = new Choices();
            preCmd.Add(new string[] { "天气", "你几岁啦" ,"许陈飞是猪吗","叶涵是仙女吗"});
            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(preCmd);
            Grammar gr = new Grammar(gb);
            recEngine.LoadGrammarAsync(gr);
            recEngine.SetInputToDefaultAudioDevice();
            recEngine.SpeechRecognized += recEngine_SpeechRecognized;
        }
        void recEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            switch (e.Result.Text)
            {
                case "天气":
                    richTextBox1.Text += "\n多云转晴";
                    break;
                case "你几岁啦":
                    richTextBox1.Text += "\n永远18 :)";
                    break;
                case "许陈飞是猪吗":
                    richTextBox1.Text += "\n是肥猪！";
                    break;
                case "涵涵是仙女吗":
                    richTextBox1.Text += "\n是的呀";
                    break;
            }
        }
        //语音识别开启
        private void button5_Click(object sender, EventArgs e)
        {
            recEngine.RecognizeAsync(RecognizeMode.Multiple);
            button5.Enabled = true;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            recEngine.RecognizeAsyncStop();
            button6.Enabled = false;
        }
    
        //语音合成
        private void button7_Click(object sender, EventArgs e)
        {
            SpVoice voice = new SpVoice();
            voice.Voice = voice.GetVoices(string.Empty, string.Empty).Item(0);
            voice.Speak(this.textBox1.Text, SpeechVoiceSpeakFlags.SVSFDefault);

            string constructorString = "server=localhost;User Id=root;password=1604010225;Database=hanhan";
            MySqlConnection myConnnect = new MySqlConnection(constructorString);
            myConnnect.Open();
            MySqlCommand myCmd = new MySqlCommand("insert into infor(infor) values('" + this.textBox1.Text+"')", myConnnect);
         //   Console.WriteLine(myCmd.CommandText);
            if (myCmd.ExecuteNonQuery() > 0)
            {
                Console.WriteLine("数据插入成功！");
            }

            myCmd.Dispose();
            myConnnect.Close();
        }
        void method()
        {

            // 设置APPID/AK/SK
            var APP_ID = "16604945";
            var API_KEY = "qTvV0EqG1Mmaavs9wIGzNBPS";                   //你的 Api Key
            var SECRET_KEY = "wwckh4CQm2NR5tEZPu0nzNmSzg9Q3KDT";        //你的 Secret Key
            var client = new Baidu.Aip.Face.Face(API_KEY, SECRET_KEY);
            client.Timeout = 60000;  // 修改超时时间


            //取决于image_type参数，传入BASE64字符串或URL字符串或FACE_TOKEN字符串
            //你共享的图片路径（点击路径可直接查看图片）
            var image = "http://b117.photo.store.qq.com/psb?/V10bnVD11OXPw5/QWpK4tAnWQ0G2t*V7d.MYQXiAoniZpYvqqjGn5kPoPA!/c/dHUAAAAAAAAA&bo=nAU4BJ0FOAQRECY!&rf=mood_app";
            var imageType = "URL";


            //注册人脸
            var groupId = "group1";
            var userId = "user1";
            // 调用人脸注册，可能会抛出网络等异常，请使用try/catch捕获
            var result = client.UserAdd(image, imageType, groupId, userId);
            Console.WriteLine(result);
            // 如果有可选参数
            var options = new Dictionary<string, object>{
                        {"user_info", "user's info"},
                        {"quality_control", "NORMAL"},
                        {"liveness_control", "LOW"}
                    };
            // 带参数调用人脸注册
            result = client.UserAdd(image, imageType, groupId, userId, options);
            Console.WriteLine(result);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            method();
        }
    }
}
