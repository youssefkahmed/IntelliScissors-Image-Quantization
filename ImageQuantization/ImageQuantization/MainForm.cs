using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

namespace ImageQuantization
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix; // The Selected Image
        RGBPixel[,] newImage; // The Quantized Image
        int k; // The Number Of Clusters
        bool Quantized = false; // Check If The Image Has Been Quantized


        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
                txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
                txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
            }
            
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            if (ImageMatrix != null)
            {
                double sigma = double.Parse(txtGaussSigma.Text);
                int maskSize = (int)nudMaskSize.Value;
                ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
            }

            else
            {
                MessageBox.Show("Please Select An Image First!");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ImageMatrix != null)
            {
                k = Convert.ToInt32(numericUpDown1.Value);

                //First Create the instance of Stopwatch Class
                Stopwatch sw = new Stopwatch();

                // Start The StopWatch ...From 000
                sw.Start();

                newImage = ImageOperations.createGraph(ImageMatrix, k);

                //Stop the Timer
                sw.Stop();

                //Writing Execution Time in label
                string ExecutionTimeTaken = string.Format("Minutes: {0}\nSeconds: {1}\n Miliseconds: {2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds);
                ExecutionTimeLabel.Text = ExecutionTimeTaken;


                ImageOperations.DisplayImage(newImage, pictureBox2);
                Quantized = true;
            }

            else
            {
                MessageBox.Show("Please Select An Image First!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
           
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void btnNumOfDC_Click(object sender, EventArgs e)
        {
            if (ImageMatrix != null)
            {
                if (Quantized)
                ImageOperations.DisplayNumOfDC();
                else
                    MessageBox.Show("Please Quantize The Image First!");
            }


            else
                MessageBox.Show("Please Select An Image First!");
        }

        private void btnMSTSum_Click(object sender, EventArgs e)
        {
            if (ImageMatrix != null)
            {
                if (Quantized)
                    ImageOperations.DisplayMSTSum();
                else
                    MessageBox.Show("Please Quantize The Image First!");
            }


            else
                MessageBox.Show("Please Select An Image First!");
        }
    }
}