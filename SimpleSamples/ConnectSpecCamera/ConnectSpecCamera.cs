using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MvCamCtrl.NET;
using System.Runtime.InteropServices;
using System.IO;

namespace ConnectSpecCamera
{
    class ConnectSpecCamera
    {
        static void Main(string[] args)
        {
            MyCamera.MV_CC_DEVICE_INFO stDevInfo = new MyCamera.MV_CC_DEVICE_INFO();
            stDevInfo.nTLayerType = MyCamera.MV_GIGE_DEVICE;
            MyCamera.MV_GIGE_DEVICE_INFO stGigEDev= new MyCamera.MV_GIGE_DEVICE_INFO();
            int nRet = MyCamera.MV_OK;
            MyCamera device = new MyCamera();

            do
            {
                Console.Write("Please input Device Ip : ");
                string strCurrentIp = Convert.ToString(Console.ReadLine());// ch:需要连接的相机ip(根据实际填充) 
                                                        // en:The camera IP that needs to be connected (based on actual padding)
                Console.Write("Please input Net Export Ip : ");
                string strNetExport = Convert.ToString(Console.ReadLine());   // ch:相机对应的网卡ip(根据实际填充) 
                                                        // en:The pc IP that needs to be connected (based on actual padding)
                var parts = strCurrentIp.Split('.');
                try
                {
                    int nIp1 = Convert.ToInt32(parts[0]);
                    int nIp2 = Convert.ToInt32(parts[1]);
                    int nIp3 = Convert.ToInt32(parts[2]);
                    int nIp4 = Convert.ToInt32(parts[3]);
                    stGigEDev.nCurrentIp = (uint)((nIp1 << 24) | (nIp2 << 16) | (nIp3 << 8) | nIp4);

                    parts = strNetExport.Split('.');
                    nIp1 = Convert.ToInt32(parts[0]);
                    nIp2 = Convert.ToInt32(parts[1]);
                    nIp3 = Convert.ToInt32(parts[2]);
                    nIp4 = Convert.ToInt32(parts[3]);
                    stGigEDev.nNetExport = (uint)((nIp1 << 24) | (nIp2 << 16) | (nIp3 << 8) | nIp4);
                }
                catch
                {
                    Console.Write("Invalid Input!\n");
                    break;
                }

                // stGigEDev结构体转为stDevInfo.SpecialInfo.stGigEInfo(Byte[])
                IntPtr stGigeInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(stGigEDev));
                Marshal.StructureToPtr(stGigEDev, stGigeInfoPtr, false);
                stDevInfo.SpecialInfo.stGigEInfo = new Byte[Marshal.SizeOf(stDevInfo.SpecialInfo)];
                Marshal.Copy(stGigeInfoPtr, stDevInfo.SpecialInfo.stGigEInfo, 0, Marshal.SizeOf(stDevInfo.SpecialInfo));
                Marshal.Release(stGigeInfoPtr);

                // ch:创建设备 | en: Create device
                nRet = device.MV_CC_CreateDevice_NET(ref stDevInfo);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Create device failed:{0:x8}", nRet);
                    break;
                }

                // ch:打开设备 | en:Open device
                nRet = device.MV_CC_OpenDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Open device failed:{0:x8}", nRet);
                    break;
                }

                // ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only works for the GigE camera)
                if (stDevInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    int nPacketSize = device.MV_CC_GetOptimalPacketSize_NET();
                    if (nPacketSize > 0)
                    {
                        nRet = device.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                        if (nRet != MyCamera.MV_OK)
                        {
                            Console.WriteLine("Warning: Set Packet Size failed {0:x8}", nRet);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Warning: Get Packet Size failed {0:x8}", nPacketSize);
                    }
                }

                // ch:设置触发模式为off || en:set trigger mode as off
                nRet = device.MV_CC_SetEnumValue_NET("TriggerMode", 0);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Set TriggerMode failed!");
                    break;
                }

                // ch:开启抓图 || en: start grab image
                nRet = device.MV_CC_StartGrabbing_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Start grabbing failed:{0:x8}", nRet);
                    break;
                }

                // ch:获取包大小 || en: Get Payload Size
                MyCamera.MVCC_INTVALUE stParam = new MyCamera.MVCC_INTVALUE();
                nRet = device.MV_CC_GetIntValue_NET("PayloadSize", ref stParam);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Get PayloadSize failed:{0:x8}", nRet);
                    break;
                }
                UInt32 nPayloadSize = stParam.nCurValue;

                int nCount = 0;
                IntPtr pBufForDriver = Marshal.AllocHGlobal((int)nPayloadSize);     // ch: 裸数据缓存 | en: raw data buff
                IntPtr pBufForSaveImage = IntPtr.Zero;                              // ch: 图片数据缓存 | en: Image data buff
                MyCamera.MV_FRAME_OUT_INFO_EX FrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();
                while (nCount++ != 10)
                {
                    nRet = device.MV_CC_GetOneFrameTimeout_NET(pBufForDriver, nPayloadSize, ref FrameInfo, 1000);
                    // ch:获取一帧图像 | en:Get one image
                    if (MyCamera.MV_OK == nRet)
                    {
                        Console.WriteLine("Get One Frame: Width[{0}] , Height[{1}] , FrameNum[{2}]", FrameInfo.nWidth, FrameInfo.nHeight, FrameInfo.nFrameNum);
                        if (pBufForSaveImage == IntPtr.Zero)
                        {
                            pBufForSaveImage = Marshal.AllocHGlobal((int)(FrameInfo.nHeight * FrameInfo.nWidth * 3 + 2048));
                        }
                        MyCamera.MV_SAVE_IMAGE_PARAM_EX stSaveParam = new MyCamera.MV_SAVE_IMAGE_PARAM_EX();
                        stSaveParam.enImageType = MyCamera.MV_SAVE_IAMGE_TYPE.MV_Image_Bmp;
                        stSaveParam.enPixelType = FrameInfo.enPixelType;
                        stSaveParam.pData = pBufForDriver;
                        stSaveParam.nDataLen = FrameInfo.nFrameLen;
                        stSaveParam.nHeight = FrameInfo.nHeight;
                        stSaveParam.nWidth = FrameInfo.nWidth;
                        stSaveParam.pImageBuffer = pBufForSaveImage;
                        stSaveParam.nBufferSize = (uint)(FrameInfo.nHeight * FrameInfo.nWidth * 3 + 2048);
                        stSaveParam.nJpgQuality = 80;
                        nRet = device.MV_CC_SaveImageEx_NET(ref stSaveParam);
                        if (MyCamera.MV_OK != nRet)
                        {
                            Console.WriteLine("Save Image failed:{0:x8}", nRet);
                            continue;
                        }

                        // ch:将图像数据保存到本地文件 | en:Save image data to local file
                        byte[] data = new byte[stSaveParam.nImageLen];
                        Marshal.Copy(pBufForSaveImage, data, 0, (int)stSaveParam.nImageLen);
                        FileStream pFile = null;
                        try
                        {
                            pFile = new FileStream("frame" + nCount.ToString() + ".bmp", FileMode.Create);
                            pFile.Write(data, 0, data.Length);
                        }
                        catch
                        {
                            Console.WriteLine("保存失败");
                            continue;
                        }
                        finally
                        {
                            pFile.Close();
                        }
                    }
                    else
                    {
                        Console.WriteLine("No data:{0:x8}", nRet);
                        break;
                    }
                }
                Marshal.FreeHGlobal(pBufForDriver);
                Marshal.FreeHGlobal(pBufForSaveImage);

                // ch:停止抓图 | en:Stop grabbing
                nRet = device.MV_CC_StopGrabbing_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Stop grabbing failed{0:x8}", nRet);
                    break;
                }

                // ch:关闭设备 | en:Close device
                nRet = device.MV_CC_CloseDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Close device failed{0:x8}", nRet);
                    break;
                }

                // ch:销毁设备 | en:Destroy device
                nRet = device.MV_CC_DestroyDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Destroy device failed:{0:x8}", nRet);
                    break;
                }
            } while (false);

            if (MyCamera.MV_OK != nRet)
            {
                // ch:销毁设备 | en:Destroy device
                nRet = device.MV_CC_DestroyDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Destroy device failed:{0:x8}", nRet);
                }
            }

            Console.WriteLine("Press enter to exit");
            Console.ReadKey();
        }
    }
}
