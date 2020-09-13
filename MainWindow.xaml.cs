using Emgu.CV.Structure;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Emgu.CV;
using Emgu.CV.Util;
using SteganographyWPF.Util;

//StopWatch

namespace SteganographyWPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Mat mMat;
        private ImageProcess imageProcess;

        public MainWindow()
        {
            InitializeComponent();
        }

        //1:lsb 2:dft 3:dct
        private int mode = 1;

        private bool imageMode = true;
        
        private void MenuOpen_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            {
                openFileDialog.Filter = "PNG图片|*.png|JPG图片|*.jpg|所有文件|*.*";
            }
            if (openFileDialog.ShowDialog() == true)
            {
                mMat = CvInvoke.Imread(openFileDialog.FileName);

                SourcePic.ImageSource = BitmapHelper.BitmapToBitmapSource(mMat.ToBitmap());
                imageProcess = new ImageProcess(mMat, imageMode);

                if (imageMode)
                {
                    ImageRchannel.ImageSource = BitmapHelper.BitmapToBitmapSource(imageProcess.rImage.ToBitmap());
                    ImageGchannel.ImageSource = BitmapHelper.BitmapToBitmapSource(imageProcess.gImage.ToBitmap());
                    ImageBchannel.ImageSource = BitmapHelper.BitmapToBitmapSource(imageProcess.bImage.ToBitmap());
                }
                else
                {
                    ImageGchannel.ImageSource = BitmapHelper.BitmapToBitmapSource(imageProcess.grayImage.ToBitmap());
                }
                modeChange();
            }
            else
            {
                MessageBox.Show("没有选择图片");
            }
        }

        private void MenuExit_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void MenuAbout_OnClick(object sender, RoutedEventArgs e)
        {
            string messageText = "DFT:离散傅里叶变换\nDCT:离散余弦变换\nLSB:最低有效位";
            string caption = "使用说明";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBox.Show(messageText, caption, button);
        }

        private void MenuSave_OnClick(object sender, RoutedEventArgs e)
        {

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG图片|*.png|JPG图片|*.jpg";
            if (sfd.ShowDialog() == true)
            {
                string path = sfd.FileName;
                Mat temp = imageProcess.MergeMat();
                temp.Save(path);
                MessageBox.Show("保存成功", "提示");
            }
        }


        private void BtnPreView_OnClick(object sender, RoutedEventArgs e)
        {
            if (imageProcess == null)
                return;
            if (imageMode)
            {
                List<string> list = new List<string>();
                
                list.Add(TextBoxR.Text);
                list.Add(TextBoxG.Text);
                list.Add(TextBoxB.Text);

                TextBoxR.Text = "";
                TextBoxG.Text = "";
                TextBoxB.Text = "";



                if (mode == 1)
                {
                    imageProcess.LsbSte(list);
                }


                if (mode == 2)
                {
                    imageProcess.DftSte(list);
                }

                if (mode == 3)
                {
                    imageProcess.DctSte(list);
                }
            }
            else
            {
                List<string> list = new List<string>();
                list.Add(TextBoxG.Text);
                TextBoxG.Text = "";


                if (mode == 1)
                {
                    imageProcess.LsbSte(list);
                }
                if (mode == 2)
                {
                    imageProcess.DftSte(list);
                }
                if (mode == 3)
                {
                    imageProcess.DctSte(list);
                }
            }

            CvInvoke.Imshow("Preview", imageProcess.MergeMat());
            subText.Text = "平均修改像素比率: " + imageProcess.subCalculate() * 100 + "%";
            magText.Text = "平均修改像素的幅度为: " + imageProcess.magCalculate();
            MessageBox.Show("The steganography is done");
            modeChange();
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG图片|*.png|JPG图片|*.jpg";
            if (sfd.ShowDialog() == true)
            {
                string path = sfd.FileName;
                Mat temp=imageProcess.MergeMat();
                temp.Save(path);
                MessageBox.Show("保存成功", "提示");
            }
        }   

        private void BtnSelect_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            {
                openFileDialog.Filter = "PNG图片|*.png|JPG图片|*.jpg|所有文件|*.*";
            }
            if (openFileDialog.ShowDialog() == true)
            {
                mMat = CvInvoke.Imread(openFileDialog.FileName);


                SourcePic.ImageSource = BitmapHelper.BitmapToBitmapSource(mMat.ToBitmap());
                imageProcess = new ImageProcess(mMat, imageMode);

                if (imageMode)
                {
                    ImageRchannel.ImageSource = BitmapHelper.BitmapToBitmapSource(imageProcess.rImage.ToBitmap());
                    ImageGchannel.ImageSource = BitmapHelper.BitmapToBitmapSource(imageProcess.gImage.ToBitmap());
                    ImageBchannel.ImageSource = BitmapHelper.BitmapToBitmapSource(imageProcess.bImage.ToBitmap());
                }
                else
                {
                    ImageGchannel.ImageSource = BitmapHelper.BitmapToBitmapSource(imageProcess.grayImage.ToBitmap());
                }
                modeChange();
                subText.Text = "";
                magText.Text = "";
            }
            else
            {
                MessageBox.Show("没有选择图片");
            }
        }

        private void GrayRadio_OnChecked(object sender, RoutedEventArgs e)
        {
            imageMode = false;
            if (imageProcess != null)
                imageProcess.changeMode(false);

            ImageRchannel.ImageSource = null;
            ImageRProcessed.ImageSource = null;
            ImageBchannel.ImageSource = null;
            ImageBProcessed.ImageSource = null;
            TextBoxR.Text = "";
            TextBoxB.Text = "";
        }

        private void RgbRadio_OnChecked(object sender, RoutedEventArgs e)
        {
            imageMode = true;
            if (imageProcess != null)
            {
                imageProcess.changeMode(true);

                ImageRchannel.ImageSource = BitmapHelper.BitmapToBitmapSource(imageProcess.rImage.ToBitmap());
                ImageBchannel.ImageSource = BitmapHelper.BitmapToBitmapSource(imageProcess.bImage.ToBitmap());
            }
        }

        private void BtnLSB_OnChecked(object sender, RoutedEventArgs e)
        {
            mode = 1;
            modeChange();
        }

        private void BtnDFT_OnChecked(object sender, RoutedEventArgs e)
        {
            mode = 2;
            modeChange();
        }

        private void BtnDCT_OnChecked(object sender, RoutedEventArgs e)
        {
            mode = 3;
            modeChange();
        }

        private void modeChange()
        {
            if (imageProcess == null)
                return;
            if (imageMode)
            {
                if (mode == 1)
                {
                    List<string> list = new List<string>();

                    list = imageProcess.Lsbspy();

                    TextBoxR.Text = list[0];
                    TextBoxG.Text = list[1];
                    TextBoxB.Text = list[2];
                }
                else
                {
                    TextBoxR.Text = "";
                    TextBoxG.Text = "";
                    TextBoxB.Text = "";
                }


                if (mode == 2)
                {
                    VectorOfMat vector = new VectorOfMat();

                    vector = imageProcess.Dftspy();

                    ImageRProcessed.ImageSource = BitmapHelper.BitmapToBitmapSource(vector[0].ToBitmap());
                    ImageGProcessed.ImageSource = BitmapHelper.BitmapToBitmapSource(vector[1].ToBitmap());
                    ImageBProcessed.ImageSource = BitmapHelper.BitmapToBitmapSource(vector[2].ToBitmap());
                }


                if (mode == 3)
                {
                    VectorOfMat vector = new VectorOfMat();

                    vector = imageProcess.Dctspy();

                    ImageRProcessed.ImageSource = BitmapHelper.BitmapToBitmapSource(vector[0].ToBitmap());
                    ImageGProcessed.ImageSource = BitmapHelper.BitmapToBitmapSource(vector[1].ToBitmap());
                    ImageBProcessed.ImageSource = BitmapHelper.BitmapToBitmapSource(vector[2].ToBitmap());
                }
            }
            else
            {
                if (mode == 1)
                {
                    List<string> list = imageProcess.Lsbspy();

                    TextBoxG.Text = list.First();
                }
                else
                {
                    TextBoxG.Text = "";
                }


                if (mode == 2)
                {
                    VectorOfMat vector = new VectorOfMat();

                    vector = imageProcess.Dftspy();

                    ImageGProcessed.ImageSource = BitmapHelper.BitmapToBitmapSource(vector[0].ToBitmap());
                }


                if (mode == 3)
                {
                    VectorOfMat vector = new VectorOfMat();

                    vector = imageProcess.Dctspy();

                    ImageGProcessed.ImageSource = BitmapHelper.BitmapToBitmapSource(vector[0].ToBitmap());
                }
            }
        }
    }
}