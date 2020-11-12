using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MvCamCtrl.NET;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace GrabImage
{
    class GrabStrategies
    {
        public static void UpcomingThread(object obj)
        {
            Thread.Sleep(3000);

            MyCamera device = obj as MyCamera;
            device.MV_CC_SetCommandValue_NET("TriggerSoftware");
        }

        static void Main(string[] args)
        {
            int nRet = MyCamera.MV_OK;
            MyCamera device = new MyCamera();
            do
            {
                // ch:枚举设备 | en:Enum device
                MyCamera.MV_CC_DEVICE_INFO_LIST stDevList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
                nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref stDevList);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Enum device failed:{0:x8}", nRet);
                    break;
                }
                Console.WriteLine("Enum device count : " + Convert.ToString(stDevList.nDeviceNum));
                if (0 == stDevList.nDeviceNum)
                {
                    break;
                }

                MyCamera.MV_CC_DEVICE_INFO stDevInfo;                            // 通用设备信息

                // ch:打印设备信息 en:Print device info
                for (Int32 i = 0; i < stDevList.nDeviceNum; i++)
                {
                    stDevInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(stDevList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));

                    if (MyCamera.MV_GIGE_DEVICE == stDevInfo.nTLayerType)
                    {
                        MyCamera.MV_GIGE_DEVICE_INFO stGigEDeviceInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(stDevInfo.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                        uint nIp1 = ((stGigEDeviceInfo.nCurrentIp & 0xff000000) >> 24);
                        uint nIp2 = ((stGigEDeviceInfo.nCurrentIp & 0x00ff0000) >> 16);
                        uint nIp3 = ((stGigEDeviceInfo.nCurrentIp & 0x0000ff00) >> 8);
                        uint nIp4 = (stGigEDeviceInfo.nCurrentIp & 0x000000ff);
                        Console.WriteLine("\n" + i.ToString() + ": [GigE] User Define Name : " + stGigEDeviceInfo.chUserDefinedName);
                        Console.WriteLine("device IP :" + nIp1 + "." + nIp2 + "." + nIp3 + "." + nIp4);
                    }
                    else if (MyCamera.MV_USB_DEVICE == stDevInfo.nTLayerType)
                    {
                        MyCamera.MV_USB3_DEVICE_INFO stUsb3DeviceInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(stDevInfo.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                        Console.WriteLine("\n" + i.ToString() + ": [U3V] User Define Name : " + stUsb3DeviceInfo.chUserDefinedName);
                        Console.WriteLine("Serial Number : " + stUsb3DeviceInfo.chSerialNumber);
                        Console.WriteLine("Device Number : " + stUsb3DeviceInfo.chModelName);
                    }
                }

                Int32 nDevIndex = 0;
                Console.Write("\nPlease input index （0 -- {0:d}） : ", stDevList.nDeviceNum - 1);
                try
                {
                    nDevIndex = Convert.ToInt32(Console.ReadLine());
                }
                catch
                {
                    Console.Write("Invalid Input!\n");
                    break;
                }

                if (nDevIndex > stDevList.nDeviceNum - 1 || nDevIndex < 0)
                {
                    Console.Write("Input Error!\n");
                    break;
                }
                stDevInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(stDevList.pDeviceInfo[nDevIndex], typeof(MyCamera.MV_CC_DEVICE_INFO));

                // ch:创建设备 | en:Create device
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

                // ch:设置软触发模式 | en:Set Trigger Mode and Set Trigger Source
                nRet = device.MV_CC_SetEnumValueByString_NET("TriggerMode", "On");
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Set Trigger Mode failed:{0:x8}", nRet);
                    break;
                }
                nRet = device.MV_CC_SetEnumValueByString_NET("TriggerSource", "Software");
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Set Trigger Source failed:{0:x8}", nRet);
                    break;
                }

                UInt32 nImageNodeNum = 5;
                // ch:设置缓存节点个数 | en:Set number of image node
                nRet = device.MV_CC_SetImageNodeNum_NET(nImageNodeNum);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Set number of image node fail:{0:x8}", nRet);
                    break;
                }

                Console.WriteLine("\n**************************************************************************");
                Console.WriteLine("* 0.MV_GrabStrategy_OneByOne;       1.MV_GrabStrategy_LatestImagesOnly;  *");
                Console.WriteLine("* 2.MV_GrabStrategy_LatestImages;   3.MV_GrabStrategy_UpcomingImage;     *");
                Console.WriteLine("**************************************************************************");

                Console.Write("Please Input Grab Strategy:");
                UInt32 nGrabStrategy = 0;
                try
                {
                    nGrabStrategy = (UInt32)Convert.ToInt32(Console.ReadLine());
                }
                catch
                {
                    Console.Write("Invalid Input!\n");
                    break;
                }

                // ch:U3V相机不支持MV_GrabStrategy_UpcomingImage | en:U3V device not support UpcomingImage
                if (nGrabStrategy == (UInt32)MyCamera.MV_GRAB_STRATEGY.MV_GrabStrategy_UpcomingImage
                    && MyCamera.MV_USB_DEVICE == stDevInfo.nTLayerType)
                {
                    Console.Write("U3V device not support UpcomingImage\n");
                    break;
                }

                switch(nGrabStrategy)
                {
                case (UInt32)MyCamera.MV_GRAB_STRATEGY.MV_GrabStrategy_OneByOne:
                    {
                        Console.Write("Grab using the MV_GrabStrategy_OneByOne default strategy\n");
                        nRet = device.MV_CC_SetGrabStrategy_NET(MyCamera.MV_GRAB_STRATEGY.MV_GrabStrategy_OneByOne);
                        if (MyCamera.MV_OK != nRet)
                        {
                            Console.WriteLine("Set Grab Strategy fail:{0:x8}", nRet);
                            break;
                        }
                    }
                    break;
                case (UInt32)MyCamera.MV_GRAB_STRATEGY.MV_GrabStrategy_LatestImagesOnly:
                    {
                        Console.Write("Grab using strategy MV_GrabStrategy_LatestImagesOnly\n");
                        nRet = device.MV_CC_SetGrabStrategy_NET(MyCamera.MV_GRAB_STRATEGY.MV_GrabStrategy_LatestImagesOnly);
                        if (MyCamera.MV_OK != nRet)
                        {
                            Console.WriteLine("Set Grab Strategy fail:{0:x8}", nRet);
                            break;
                        }
                    }
                    break;
                case (UInt32)MyCamera.MV_GRAB_STRATEGY.MV_GrabStrategy_LatestImages:
                    {
                        Console.Write("Grab using strategy MV_GrabStrategy_LatestImages\n");
                        nRet = device.MV_CC_SetGrabStrategy_NET(MyCamera.MV_GRAB_STRATEGY.MV_GrabStrategy_LatestImages);
                        if (MyCamera.MV_OK != nRet)
                        {
                            Console.WriteLine("Set Grab Strategy fail:{0:x8}", nRet);
                            break;
                        }

                        // ch:设置输出缓存个数 | en:Set Output Queue Size
                        nRet = device.MV_CC_SetOutputQueueSize_NET(2);
                        if (MyCamera.MV_OK != nRet)
                        {
                            Console.WriteLine("Set Grab Strategy fail:{0:x8}", nRet);
                            break;
                        }
                    }
                    break;
                case (UInt32)MyCamera.MV_GRAB_STRATEGY.MV_GrabStrategy_UpcomingImage:
                    {
                        Console.Write("Grab using strategy MV_GrabStrategy_UpcomingImage\n");
                        nRet = device.MV_CC_SetGrabStrategy_NET(MyCamera.MV_GRAB_STRATEGY.MV_GrabStrategy_UpcomingImage);
                        if (MyCamera.MV_OK != nRet)
                        {
                            Console.WriteLine("Set Grab Strategy fail:{0:x8}", nRet);
                            break;
                        }

                        Thread hUpcomingThread = new Thread(UpcomingThread);
                        hUpcomingThread.Start(device);
                    }
                    break;
                default:
                    Console.Write("Input error!Use default strategy:MV_GrabStrategy_OneByOne\n");
                    break;
                }

                // ch:开启抓图 | en:start grab
                nRet = device.MV_CC_StartGrabbing_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Start grabbing failed:{0:x8}", nRet);
                    break;
                }

                // ch:发送软触发命令 | en:Send Trigger Software command
                for (UInt32 i = 0;i < nImageNodeNum;i++)
                {
                    nRet = device.MV_CC_SetCommandValue_NET("TriggerSoftware");
                    if (MyCamera.MV_OK != nRet)
                    {
                        Console.WriteLine("Send Trigger Software command fail:{0:x8}", nRet);
                        break;
                    }
                    Thread.Sleep(500);//如果帧率过小或TriggerDelay很大，可能会出现软触发命令没有全部起效而导致取不到数据的情况
                }

                MyCamera.MV_FRAME_OUT stOutFrame = new MyCamera.MV_FRAME_OUT();
                if (nGrabStrategy != (UInt32)MyCamera.MV_GRAB_STRATEGY.MV_GrabStrategy_UpcomingImage)
                {
                    while(true)
                    {
                        nRet = device.MV_CC_GetImageBuffer_NET(ref stOutFrame, 0);
                        if (MyCamera.MV_OK == nRet)
                        {
                            Console.WriteLine("Get Image Buffer:" + "Width[" + Convert.ToString(stOutFrame.stFrameInfo.nWidth) + "] , Height[" + Convert.ToString(stOutFrame.stFrameInfo.nHeight)
                                        + "] , FrameNum[" + Convert.ToString(stOutFrame.stFrameInfo.nFrameNum) + "]");
                        }
                        else
                        {
                            Console.WriteLine("No data:{0:x8}", nRet);
                            break;
                        }

                        nRet = device.MV_CC_FreeImageBuffer_NET(ref stOutFrame);
                        if (MyCamera.MV_OK != nRet)
                        {
                            Console.WriteLine("Free Image Buffer fail:{0:x8}", nRet);
                        }
                    }
                }
                else//仅用于upcoming
                {
                    nRet = device.MV_CC_GetImageBuffer_NET(ref stOutFrame, 5000);
                    if (MyCamera.MV_OK == nRet)
                    {
                        Console.WriteLine("Get Image Buffer:" + "Width[" + Convert.ToString(stOutFrame.stFrameInfo.nWidth) + "] , Height[" + Convert.ToString(stOutFrame.stFrameInfo.nHeight)
                                        + "] , FrameNum[" + Convert.ToString(stOutFrame.stFrameInfo.nFrameNum) + "]");

                        nRet = device.MV_CC_FreeImageBuffer_NET(ref stOutFrame);
                        if (MyCamera.MV_OK != nRet)
                        {
                            Console.WriteLine("Free Image Buffer fail:{0:x8}", nRet);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No data:{0:x8}", nRet);
                    }
                }

                // ch:停止抓图 | en:Stop grab image
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
