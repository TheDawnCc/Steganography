using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace SteganographyWPF.Util
{
    public class DctProcess
    {
        private Mat sourceMat;
        private Mat dctMat;

        private Mat dctPMat;


        public DctProcess(Mat sourceMat)
        {
            this.sourceMat = new Mat();
            dctMat = new Mat();
            dctPMat = new Mat();

            sourceMat.CopyTo(this.sourceMat);

            if (this.sourceMat.NumberOfChannels != 1)
                throw new InvalidCastException("this Mat's channels is not equal 1");

            if (this.sourceMat.Depth != DepthType.Cv32F)
                this.sourceMat.ConvertTo(this.sourceMat, DepthType.Cv32F);

            //转化为偶数行列,DCT变换不支持奇数行列
            this.sourceMat = new Mat(this.sourceMat, new Rectangle(0, 0, this.sourceMat.Cols & -2, this.sourceMat.Rows & -2));

            DCT();
        }

        private Mat DCT()
        {
            CvInvoke.Dct(sourceMat, dctMat, DctType.Forward);
            dctMat.CopyTo(dctPMat);

            return dctMat;
        }

        public Mat IDCT()
        {
            Mat idctMat = new Mat();
            CvInvoke.Dct(dctPMat, idctMat, DctType.Inverse);

            idctMat.ConvertTo(idctMat, DepthType.Cv8U);

            return idctMat;
        }

        public Mat GetDctMat()
        {
            Mat dMat = new Mat();

            dctPMat.ConvertTo(dMat, DepthType.Cv8U);

            return dMat;
        }
        public Mat AugmentShow()
        {
            Mat pMat = new Mat();

            dctPMat.CopyTo(pMat);

            Mat tmp = new Mat(pMat.Size, pMat.Depth, 1);
            tmp.SetTo(new MCvScalar(255));
            CvInvoke.Multiply(pMat, tmp, pMat);

            pMat.ConvertTo(pMat, DepthType.Cv8U);

            return pMat;
        }

        public void StringSte(String str, FontFace fontFace, double fontScale, int thickness)
        {
            Mat magI = new Mat();
            dctPMat.CopyTo(magI);

            int baseline = 0;
            Size tSize = CvInvoke.GetTextSize(str, fontFace, fontScale, thickness, ref baseline);


            //对隐写区域大小进行判断
            if (tSize.Width * tSize.Height > magI.Width * magI.Height * 0.75)
            {
                MessageBox.Show("超出图片所能隐写最大容量,请重试");
                return;
            }
            else
            {
                double temp = (double) 2 * tSize.Width / magI.Width;
                int rowsNum = (int) Math.Ceiling(temp);


                temp = (double) str.Length / rowsNum;
                int rowsLength1 = (int) Math.Floor(temp);
                int rowsLength2 = rowsLength1 * 2;
                int iHeight = magI.Height / 2;
                
                int ls = 0;

                for (int i = 0; i < rowsNum; i++)
                {
                    bool flag = false;
                    string s;
                    int rowsLength = 0;

                    Point tPoint = new Point(magI.Width / 2,
                        (i + 1) * tSize.Height);

                    rowsLength = tPoint.Y >= iHeight ? rowsLength2 : rowsLength1;
                    
                    if (ls + rowsLength < str.Length)
                    {
                        s = str.Substring(ls, rowsLength);
                        ls += rowsLength;
                    }
                    else
                    {
                        s = str.Substring(ls);
                        flag = true;
                    }


                    if (tPoint.Y >= magI.Height / 2)
                    {
                        tPoint.X = 0;
                    }
                    tPoint.X = tPoint.Y >= iHeight ? 0 : magI.Width / 2;

                    CvInvoke.PutText(dctPMat, s, tPoint,
                        fontFace, fontScale, new MCvScalar(-10), thickness);

                    if(flag)
                        break;
                }
            }
        }
    }
}