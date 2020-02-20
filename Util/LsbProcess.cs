using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace SteganographyWPF.Util
{
    public class LsbProcess
    {
        private Image<Gray, byte> sourceImage;
        private Image<Gray, byte> lsbImage;


        static byte[] indexBytes = {1, 2, 4, 8, 16, 32, 64, 128};

        public LsbProcess(Image<Gray, byte>pic)
        {
            sourceImage = new Image<Gray, byte>(pic.Size);
            lsbImage = new Image<Gray, byte>(pic.Size);

            pic.CopyTo(sourceImage);

            PreLSB();
        }

        public LsbProcess(Mat mat)
        {
            sourceImage = new Image<Gray, byte>(mat.Size);
            lsbImage = new Image<Gray, byte>(mat.Size);

            if (mat.NumberOfChannels != 1)
            {
                throw new InvalidCastException("this Mat's channels is not equal 1");
            }
            if (mat.Depth != DepthType.Cv8U)
            {
                mat.ConvertTo(mat, DepthType.Cv8U);
            }

            sourceImage = mat.ToImage<Gray, byte>();
            lsbImage = sourceImage.Clone();
        }

        //LSB预处理,将LSB置0
        public void PreLSB()
        {
            Image<Gray, byte> tImage = new Image<Gray, byte>(sourceImage.Size);
            sourceImage.CopyTo(tImage);

            for (int i = 0; i < tImage.Width; i++)
            {
                for (int j = 0; j < tImage.Height; j++)
                {
                    byte bt = tImage.Data[i, j, 0];

                    bt = setBit(bt, 1, false);
                    tImage.Data[i, j, 0] = bt;
                }
            }

            tImage.CopyTo(lsbImage);
        }

        public Image<Gray, byte> GetLsbImage()
        {
            return lsbImage;
        }


        public void LsbString(string s)
        {
            PreLSB();
            Image<Gray, byte> dImage = new Image<Gray, byte>(sourceImage.Size);
            lsbImage.CopyTo(dImage);

            char[] charArray = s.ToCharArray();

            //容量溢出
            if (charArray.Length * 8 > dImage.Width * dImage.Height)
                throw new ArgumentOutOfRangeException();


            int flag = 0;
            byte ichar = (byte) charArray[flag];

            for (int i = 0; i < dImage.Width; i++)
            {
                int index = 0;
                for (int j = 0; j < dImage.Height; j++)
                {
                    int value = getBit(ichar, 8 - index);

                    byte bt = setBit(dImage.Data[i, j, 0], 1, value == 1 ? true : false);
                    dImage.Data[i, j, 0] = bt;

                    index++;
                    if (index == 8)
                    {
                        index = 0;
                        if (flag < charArray.Length - 1)
                        {
                            flag++;
                            ichar = (byte) charArray[flag];
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (flag >= charArray.Length)
                    break;
            }

            dImage.CopyTo(lsbImage);
        }

        public string LsbSpy()
        {
            Image<Gray, byte> dImage = new Image<Gray, byte>(sourceImage.Size);
            lsbImage.CopyTo(dImage);

            string s = "";
            for (int i = 0; i < dImage.Width; i++)
            {
                int index = 0;
                int ichar = 0;
                for (int j = 0; j < dImage.Height; j++)
                {
                    ichar += getBit(dImage.Data[i, j, 0], 1);

                    index++;
                    if (index == 8)
                    {
                        if (ichar == 0)
                            return s;
                        if (ichar > 31 && ichar < 127)
                        {
                            char ch = (char) ichar;
                            s += ch;
                        }

                        index = 0;

                        ichar = 0;
                    }

                    ichar = ichar << 1;
                }
            }
            return s;
        }


        public byte setBit(byte data, int index, bool flag)
        {
            if (index > 8 || index < 1)
                throw new ArgumentOutOfRangeException();
            int v = index < 2 ? index : (2 << (index - 2));
            return flag ? (byte) (data | v) : (byte) (data & ~v);
        }

        public int getBit(byte data, int index)
        {
            if (index > 8 || index < 1)
                throw new ArgumentOutOfRangeException();
            return (data & indexBytes[index - 1]) == indexBytes[index - 1] ? 1 : 0;
        }
    }
}