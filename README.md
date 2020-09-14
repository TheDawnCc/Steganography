# Steganography
一个用于图像数字水印加解密算法的实现,其中包含最低有效位算法,离散傅里叶变换算法对图像进行加解密水印的处理基于EmguCV(OpenCV .net封装)

环境:emgucv-windesktop_x64-cuda 3.3.0.2826 <https://sourceforge.net/projects/emgucv/files/emgucv/3.3/>

搭建:目前EmguCV框架已采用Nuget进行管理,如果采用引用DLL的方式:安装完emgucv-windesktop_x64-cuda 3.3.0.2826后项目引用emgucv-windesktop_x64-cuda 3.3.0.2826/bin下的Emgu.CV.UI.dll,Emgu.CV.World.dll,ZedGraph.dll 然后拷贝emgucv-windesktop_x64-cuda 3.3.0.2826/bin/x64文件夹到项目的bin/Debug目录下,否则会报DLLNotFound异常

使用说明:点击Select按钮选择图片进行水印解密,程序会对图片做预处理,当出现预览图片后可以由最上方选择RGB(三通道)或者Gray模式(单通道),选择LSB,DFT,DCT分别是对应三种不同的加密算法,DFT和DCT在右边有相应的幅度图预览.然后将想要加密的密钥输入在右侧的输入框中,点击Preview按钮则相应的水印加密并弹出加密后图片的预览,之后再点击Save按钮则将加密后的图片进行保存.


预览:

![image](https://github.com/TheDawnCc/Steganography/blob/master/Preview/Preview.png)

Read:

![image](https://github.com/TheDawnCc/Steganography/blob/master/Preview/GIF.gif)
