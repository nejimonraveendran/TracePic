using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TracePic
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("dwmapi.dll")]
        static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref int[] pMargins);

        const int GWL_EXSTYLE = -20;
        const int WS_EX_LAYERED = 0x80000;
        const int WS_EX_TRANSPARENT = 0x20;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
     
            this.TopMost = true;
            notifyIcon1.Icon = this.Icon;

            cmbColor.SelectedIndex = 0;
        }

        int initialStyle;
        private void btnToggle_Click(object sender, EventArgs e)
        {
            this.TopMost = true;
            initialStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(this.Handle, GWL_EXSTYLE, initialStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("test");
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            openDlg.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*";
            var dlgResult = openDlg.ShowDialog();
            if (dlgResult != DialogResult.OK)
                return;

            picBox.SizeMode = PictureBoxSizeMode.Zoom;
            picBox.ImageLocation = openDlg.FileName;
        }

        private void nudOpacity_ValueChanged(object sender, EventArgs e)
        {
            this.Opacity = Convert.ToDouble(nudOpacity.Value);
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetWindowLong(this.Handle, -20, initialStyle);
            //nudOpacity.Value = 1;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            string tempLocation = @"c:\tmp\TracePic";

            if (!Directory.Exists(tempLocation))
                Directory.CreateDirectory(tempLocation);

            var curFileInfo = new FileInfo(picBox.ImageLocation);

            string griddedFile = tempLocation + @"\" + curFileInfo.Name.ToLower().Replace(curFileInfo.Extension.ToLower(), "") + "-gridded" + curFileInfo.Extension;
            string canvasFile = griddedFile.Replace("-gridded", "-canvas");

            var sourceImage = picBox.Image;

            Color lineColor;

            switch (cmbColor.Text) 
            {
                case "Red": 
                    lineColor = Color.Red;
                    break;
                case "Black":
                    lineColor = Color.Black;
                    break;
                case "White":
                    lineColor = Color.White;
                    break;
                case "Yellow":
                    lineColor = Color.Yellow;
                    break;
                case "Blue":
                    lineColor = Color.Blue;
                    break;
                default:
                    lineColor = Color.Red;
                    break;
            }

            createGriddedImage(sourceImage, Convert.ToInt32(nudGridCols.Value), (float) nudLineWidth.Value, lineColor, griddedFile);
            createGriddedImage(sourceImage, Convert.ToInt32(nudGridCols.Value), (float) nudLineWidth.Value, lineColor, canvasFile, true);

            picBox.ImageLocation = griddedFile;

        }


        private void createGriddedImage(Image sourceImage, int rowCols, float lineWidth, Color lineColor, string targetFileLocation, bool createCanvas = false)
        {
            int side = sourceImage.Width < sourceImage.Height ? sourceImage.Width : sourceImage.Height;
            var unit = Convert.ToInt32(side / rowCols);


            using (var targetBitmap = new Bitmap(sourceImage.Width, sourceImage.Height))
            {
                using (var g = Graphics.FromImage(targetBitmap))
                {
                    
                    if (createCanvas)
                    {
                        var brush = new SolidBrush(Color.White);
                        var rect = new Rectangle(0, 0, targetBitmap.Width, targetBitmap.Height);
                        g.FillRectangle(brush, rect);
                    }
                    else
                    {
                        g.DrawImage(sourceImage, 0, 0, sourceImage.Width, sourceImage.Height);
                    }

                    //draw lines
                    var pen = new Pen(lineColor, lineWidth);
                    int startX, startY, endX = unit, endY;
                    int counter;
                    Font font = new Font("Arial", 10);
                    var fontBrush = new SolidBrush(lineColor);


                    startX = unit;
                    endX = unit;
                    startY = 0;
                    endY = sourceImage.Height;
                    counter = 1;
                   
                    while (startX < sourceImage.Width)
                    {
                        g.DrawLine(pen, startX, startY, endX, endY);
                        g.DrawString(counter.ToString(), font, fontBrush, startX + 1, startY + 1);

                        startX = startX + unit;
                        endX = endX + unit;
                        counter++;
                    }

                    startY = unit;
                    endY = unit;
                    startX = 0;
                    endX = sourceImage.Width;
                    counter = 1;

                    while (startY < sourceImage.Height)
                    {
                        g.DrawLine(pen, startX, startY, endX, endY);
                        g.DrawString(counter.ToString(), font, fontBrush, startX + 1, startY + 1);

                        startY = startY + unit;
                        endY = endY + unit;
                        counter++;                   }

                    //g.DrawLine(pen, 10, 0, 10, targetBitmap.Height);

                    targetBitmap.Save(targetFileLocation);
                }
            }
        }

    }
}
