using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Point = System.Drawing.Point;

namespace SteganographyWPF.Util
{
    public class ImageProcess
    {
        private Image<Rgb, byte> mImage; //原图

        public Image<Rgb, byte> rImage;
        public Image<Rgb, byte> gImage;
        public Image<Rgb, byte> bImage;
        public Image<Gray, byte> grayImage;

        private Mat grayMat;

        private Mat redMat;

        private Mat greenMat;

        private Mat blueMat;

        private LsbProcess lsbR;
        private LsbProcess lsbG;
        private LsbProcess lsbB;
        private LsbProcess lsbGray;

        private DftProcess dftR;
        private DftProcess dftG;
        private DftProcess dftB;
        private DftProcess dftGray;

        private DctProcess dctR;
        private DctProcess dctG;
        private DctProcess dctB;
        private DctProcess dctGray;

        private bool processMode;

        public ImageProcess(Mat image, bool mode)
        {
            //split 0 b 1 g 2 r
            mImage = new Image<Rgb, byte>(image.Size);
            rImage = new Image<Rgb, byte>(image.Size);
            gImage = new Image<Rgb, byte>(image.Size);
            bImage = new Image<Rgb, byte>(image.Size);
            grayImage = new Image<Gray, byte>(image.Size);

            redMat = new Mat();
            greenMat = new Mat();
            blueMat = new Mat();
            grayMat = new Mat();

            Debug.WriteLine(mode);
            processMode = mode;


            mImage = image.ToImage<Rgb, byte>();

            VectorOfMat vectorOfMat = new VectorOfMat();
            CvInvoke.Split(mImage, vectorOfMat);

            vectorOfMat[0].CopyTo(blueMat);
            vectorOfMat[1].CopyTo(greenMat);
            vectorOfMat[2].CopyTo(redMat);

            
            Stopwatch stopwatch=new Stopwatch();
            stopwatch.Start();
            lsbR = new LsbProcess(redMat);
            lsbG = new LsbProcess(greenMat);
            lsbB = new LsbProcess(blueMat);
            stopwatch.Stop();
            Debug.WriteLine("lsb time is : "+stopwatch.Elapsed);

            stopwatch.Start();
            dftR = new DftProcess(redMat);
            dftG = new DftProcess(greenMat);
            dftB = new DftProcess(blueMat);
            stopwatch.Stop();
            Debug.WriteLine("dft time is : " + stopwatch.Elapsed);

            stopwatch.Start();
            dctR = new DctProcess(redMat);
            dctG = new DctProcess(greenMat);
            dctB = new DctProcess(blueMat);
            stopwatch.Stop();
            Debug.WriteLine("dct time is : " + stopwatch.Elapsed);


            Mat fillMat = new Mat(image.Size, DepthType.Cv8U, 1);
            fillMat.SetTo(new MCvScalar(0));
            Mat tmpMat = new Mat();

            CvInvoke.Merge(new VectorOfMat(redMat.Clone(), fillMat.Clone(), fillMat.Clone()), tmpMat);
            rImage = tmpMat.ToImage<Rgb, byte>();
            CvInvoke.Merge(new VectorOfMat(fillMat.Clone(), greenMat.Clone(), fillMat.Clone()), tmpMat);
            gImage = tmpMat.ToImage<Rgb, byte>();
            CvInvoke.Merge(new VectorOfMat(fillMat.Clone(), fillMat.Clone(), blueMat.Clone()), tmpMat);
            bImage = tmpMat.ToImage<Rgb, byte>();


            grayImage = image.ToImage<Gray, byte>();
            grayImage.Mat.CopyTo(grayMat);

            lsbGray = new LsbProcess(grayMat);

            dftGray = new DftProcess(grayMat);

            dctGray = new DctProcess(grayMat);
        }

        public List<string> Lsbspy()
        {
            List<string> myList = new List<string>();
            if (processMode)
            {
                string s1 = lsbR.LsbSpy();
                string s2 = lsbG.LsbSpy();
                string s3 = lsbB.LsbSpy();

                myList.Add(s1);
                myList.Add(s2);
                myList.Add(s3);
            }
            else
            {
                string s = lsbGray.LsbSpy();
                myList.Add(s);
            }


            return myList;
        }

        public VectorOfMat Dftspy()
        {
            VectorOfMat vector = new VectorOfMat();

            if (processMode)
            {
                Mat tR = new Mat();
                Mat tG = new Mat();
                Mat tB = new Mat();

                dftR.DftShift().CopyTo(tR);
                vector.Push(tR);
                dftG.DftShift().CopyTo(tG);
                vector.Push(tG);
                dftB.DftShift().CopyTo(tB);
                vector.Push(tB);
            }
            else
            {
                Mat temp = new Mat();
                dftGray.DftShift().CopyTo(temp);
                vector.Push(temp);
            }

            return vector;
        }

        public VectorOfMat Dctspy()
        {
            VectorOfMat vector = new VectorOfMat();

            if (processMode)
            {
                Mat tR = new Mat();
                Mat tG = new Mat();
                Mat tB = new Mat();

                dctR.AugmentShow().CopyTo(tR);
                vector.Push(tR);
                dctG.AugmentShow().CopyTo(tG);
                vector.Push(tG);
                dctB.AugmentShow().CopyTo(tB);
                vector.Push(tB);
            }
            else
            {
                Mat temp = new Mat();

                dctGray.AugmentShow().CopyTo(temp);
                vector.Push(temp);
            }

            return vector;
        }

        public void LsbSte(List<string> list)
        {
            if (processMode)
            {
                lsbR.LsbString(list[0]);
                lsbG.LsbString(list[1]);
                lsbB.LsbString(list[2]);

                redMat = lsbR.GetLsbImage().Mat;
                greenMat = lsbG.GetLsbImage().Mat;
                blueMat = lsbB.GetLsbImage().Mat;
            }
            else
            {
                lsbGray.LsbString(list[0]);
                grayMat = lsbGray.GetLsbImage().Mat;
            }
        }

        public void changeMode(bool flag)
        {
            processMode = flag;
        }

        public void DftSte(List<string> list)
        {
            if (processMode)
            {
                dftR.stringSte(list[0], FontFace.HersheyDuplex, 2, 2);
                dftG.stringSte(list[1], FontFace.HersheyDuplex, 2, 2);
                dftB.stringSte(list[2], FontFace.HersheyDuplex, 2, 2);

                redMat = dftR.IDFT();
                greenMat = dftG.IDFT();
                blueMat = dftB.IDFT();
            }
            else
            {
                dftGray.stringSte(list[0], FontFace.HersheyDuplex, 2, 2);

                grayMat = dftGray.IDFT();
            }
        }

        public void DctSte(List<string> list)
        {
            if (processMode)
            {
                dctR.StringSte(list[0], FontFace.HersheyDuplex, 2, 2);
                dctG.StringSte(list[1], FontFace.HersheyDuplex, 2, 2);
                dctB.StringSte(list[2], FontFace.HersheyDuplex, 2, 2);

                redMat = dctR.IDCT();
                greenMat = dctG.IDCT();
                blueMat = dctB.IDCT();
            }
            else
            {
                dctGray.StringSte(list[0], FontFace.HersheyDuplex, 2, 2);

                grayMat = dctGray.IDCT();
            }
        }

        //RGB三通道混合
        public Mat MergeMat()
        {
            Mat temp = new Mat();
            if (processMode)
            {
                VectorOfMat tt = new VectorOfMat(blueMat, greenMat, redMat);
                CvInvoke.Merge(tt, temp);
            }
            else
            {
                temp = grayMat;
            }
            return temp;
        }

        public double subCalculate()
        {
            Mat temp = new Mat();
            if (processMode)
            {
                temp = MergeMat();
                CvInvoke.Subtract(mImage.Mat, temp, temp);
            }
            else
            {
                grayMat.CopyTo(temp);
                CvInvoke.Subtract(grayImage.Mat, temp, temp);
            }

            double sum = 0;
            for (int i = 0; i < temp.Height; i++)
            {
                for (int j = 0; j < temp.Width; j++)
                {
                    if (processMode)
                    {
                        for (int z = 0; z < 3; z++)
                        {
                            if ((byte) temp.GetData(i, j).GetValue(z) != 0)
                                sum++;
                        }
                    }
                    else
                    {
                        if ((byte) temp.GetData(i, j).GetValue(0) != 0)
                            sum++;
                    }
                }
            }
            sum = sum / (temp.Height * temp.Width * 3);
            Debug.WriteLine(sum);
            return sum;
        }

        public double magCalculate()
        {
            Mat temp = new Mat();

            if (processMode)
            {
                temp = MergeMat();
                CvInvoke.Subtract(mImage.Mat, temp, temp);
            }
            else
            {
                grayMat.CopyTo(temp);
                CvInvoke.Subtract(grayImage.Mat, temp, temp);
            }


            double sum = 0;
            double avg = 0;
            for (int i = 0; i < temp.Height; i++)
            {
                for (int j = 0; j < temp.Width; j++)
                {
                    if (processMode)
                    {
                        for (int z = 0; z < 3; z++)
                        {
                            int vv = (byte) temp.GetData(i, j).GetValue(z);
                            if (vv != 0)
                            {
                                sum++;
                                avg += vv;
                            }
                        }
                    }
                    else
                    {
                        int vv = (byte) temp.GetData(i, j).GetValue(0);
                        if (vv != 0)
                        {
                            sum++;
                            avg += vv;
                        }
                    }
                }
            }
            avg /= sum;

            return avg;
        }
    }
}