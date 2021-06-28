using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace SteganographyWPF.Util
{
    public class DftProcess
    {
        //源图,幅度谱,相位谱
        private Mat sourceMat;

        private Mat magnitudeMat;
        private Mat phaseMat;

        //处理过的幅度图
        private Mat magPMat;

        //FFT 行宽
        private int fftRows;

        private int fftCols;


        public DftProcess(Mat sourceMat)
        {
            this.sourceMat = new Mat();
            magnitudeMat = new Mat();
            phaseMat = new Mat();
            magPMat = new Mat();

            sourceMat.CopyTo(this.sourceMat);
            
            if(this.sourceMat.NumberOfChannels!=1)
                throw new InvalidCastException("this Mat's channels is not equal 1");

            if (this.sourceMat.Depth != DepthType.Cv32F)
                this.sourceMat.ConvertTo(this.sourceMat, DepthType.Cv32F);

            //FFT扩展后的长宽
            fftRows = CvInvoke.GetOptimalDFTSize(sourceMat.Rows);
            fftCols = CvInvoke.GetOptimalDFTSize(sourceMat.Cols);

            DFT();
        }

        public void DFT()
        {
            Mat padded = new Mat();

            //扩展行宽进行快速,其他位置用0扩充
            CvInvoke.CopyMakeBorder(sourceMat, padded, 0, fftRows - sourceMat.Rows, 0, fftCols - sourceMat.Cols,
                BorderType.Constant,
                new MCvScalar(0));

            //构造虚部
            Mat tmp = new Mat(padded.Size, DepthType.Cv32F, 1);
            tmp.SetTo(new MCvScalar(0));

            VectorOfMat planes = new VectorOfMat(padded, tmp);

            Mat complexI = new Mat();

            //多通道混合
            CvInvoke.Merge(planes, complexI);

            //进行DFT变换
            CvInvoke.Dft(complexI, complexI, DxtType.Forward, padded.Rows);

            //将混合的单通道分离实数域虚数域
            CvInvoke.Split(complexI, planes);

            //计算幅度谱,相位谱
            CudaInvoke.Magnitude(planes[0], planes[1], magnitudeMat);
            CudaInvoke.Phase(planes[0], planes[1], phaseMat);

            
            magnitudeMat.CopyTo(magPMat);
        }

        public Mat IDFT()
        {
            Mat realMat = new Mat();
            Mat complexMat = new Mat();

            //用幅度谱和相位谱计算实部和虚部
            CvInvoke.PolarToCart(magPMat, phaseMat, realMat, complexMat);

            Mat idft = new Mat();

            CvInvoke.Merge(new VectorOfMat(realMat, complexMat), idft);

            //逆傅里叶变换
            CvInvoke.Dft(idft, idft, DxtType.Inverse, idft.Rows);

            VectorOfMat planes = new VectorOfMat();
            CvInvoke.Split(idft, planes);

            //裁剪因FFT而扩展的区域
            Mat resMat = new Mat(planes[0], new Rectangle(0, 0, sourceMat.Width, sourceMat.Height));

            //将数值缩放到0-255并将float转为byte格式以便识别
            CvInvoke.Normalize(resMat, resMat, 0, 255, NormType.MinMax);
            resMat.ConvertTo(resMat, DepthType.Cv8U);

            return resMat;
        }


        //对magPMat处理操作
        public void stringSte(String str,FontFace fontFace, double fontScale, int thickness)
        {

            Mat magI = new Mat();
            magPMat.CopyTo(magI);

            int baseline = 0;
            Size tSize = CvInvoke.GetTextSize(str, fontFace, fontScale, thickness, ref baseline);

            
            //对隐写区域大小进行判断
            if (tSize.Width * tSize.Height > magI.Width * magI.Height * 0.14)
            {
                MessageBox.Show("超出图片所能隐写最大容量,请重试");
                return;
            }
            else
            {
                double temp = (double) tSize.Width / magI.Width * 2;
                int rowsNum = (int) Math.Ceiling(temp);
                temp = (double) str.Length / rowsNum;
                int rowsLength = (int) Math.Ceiling(temp);
                int ls = 0;

                for (int i = 0; i < rowsNum; i++)
                {
                    string s;
                    if (ls + rowsLength < str.Length)
                    {
                        s = str.Substring(ls, rowsLength);
                        ls += rowsLength;
                    }
                    else
                    {
                        s = str.Substring(ls);
                    }

                    tSize = CvInvoke.GetTextSize(s, fontFace, fontScale, thickness, ref baseline);

                    Point tPoint = new Point(magI.Width / 4 - tSize.Width / 2,
                        magI.Height / 4 + (i + 1) * tSize.Height);
                    CvInvoke.PutText(magPMat, s, tPoint,
                        fontFace, fontScale, new MCvScalar(255), thickness);
                    CvInvoke.Flip(magPMat, magPMat, FlipType.Horizontal | FlipType.Vertical);
                    CvInvoke.PutText(magPMat, s, tPoint,
                        fontFace, fontScale, new MCvScalar(255), thickness);
                    CvInvoke.Flip(magPMat, magPMat, FlipType.Horizontal | FlipType.Vertical);
                }

            }
        }

        public Mat DftShift()
        {
            Mat magI = new Mat();
            magPMat.CopyTo(magI);

            //对数变换
            CvInvoke.Add(magI, new ScalarArray(new MCvScalar(1)), magI);
            CvInvoke.Log(magI, magI);

            //中心化,&-2转为偶数行列,便于均分
            magI = new Mat(magI, new Rectangle(0, 0, magI.Cols & -2, magI.Rows & -2));

            int cx = magI.Cols / 2;
            int cy = magI.Rows / 2;

            //因为傅里叶变换的共轭对称性,将低频分量移到原点向外为高频,两两交换便于观察
            Mat q0 = new Mat(magI, new Rectangle(0, 0, cx, cy));
            Mat q1 = new Mat(magI, new Rectangle(cx, 0, cx, cy));
            Mat q2 = new Mat(magI, new Rectangle(0, cy, cx, cy));
            Mat q3 = new Mat(magI, new Rectangle(cx, cy, cx, cy));

            Mat temp = new Mat();
            q0.CopyTo(temp);
            q3.CopyTo(q0);
            temp.CopyTo(q3);

            q1.CopyTo(temp);
            q2.CopyTo(q1);
            temp.CopyTo(q2);

            CvInvoke.Normalize(magI, magI, 0, 255, NormType.MinMax);
            magI.ConvertTo(magI, DepthType.Cv8U);

            return magI;
        }
    }
}