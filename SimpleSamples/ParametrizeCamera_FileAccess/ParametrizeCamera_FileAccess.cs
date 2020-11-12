using System;
using System.Collections.Generic;
using MvCamCtrl.NET;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;

namespace ParametrizeCamera_FileAccess
{
    class Program
    {
        public static MyCamera device;
        public static uint g_nMode = 0;
        public static int g_nRet = MyCamera.MV_OK;

        static void FileAccessProgress()
        {
            int nRet = MyCamera.MV_OK;
            MyCamera.MV_CC_FILE_ACCESS_PROGRESS stFileAccessProgress = new MyCamera.MV_CC_FILE_ACCESS_PROGRESS();

            while (true)
            {
                //ch:获取文件存取进度 |en:Get progress of file access
                nRet = device.MV_CC_GetFileAccessProgress_NET(ref stFileAccessProgress);
                Console.WriteLine("State = {0:x8},Completed = {1},Total = {2}", nRet , stFileAccessProgress.nCompleted , stFileAccessProgress.nTotal);
                if (nRet != MyCamera.MV_OK || (stFileAccessProgress.nCompleted != 0 && stFileAccessProgress.nCompleted == stFileAccessProgress.nTotal))
                {
                    break;
                }

                Thread.Sleep(50);
            }
        }

        static void FileAccessThread()
        {
            MyCamera.MV_CC_FILE_ACCESS stFileAccess = new MyCamera.MV_CC_FILE_ACCESS();

            stFileAccess.pUserFileName = "UserSet1.bin";
            stFileAccess.pDevFileName = "UserSet1";
            if (1 == g_nMode)
            {
                //ch:读模式 |en:Read mode
                g_nRet = device.MV_CC_FileAccessRead_NET(ref stFileAccess);
                if (MyCamera.MV_OK != g_nRet)
                {
                    Console.WriteLine("File Access Read failed:{0:x8}", g_nRet);
                }
            }
            else if (2 == g_nMode)
            {
                //ch:写模式 |en:Write mode
                g_nRet = device.MV_CC_FileAccessWrite_NET(ref stFileAccess);
                if (MyCamera.MV_OK != g_nRet)
                {
                    Console.WriteLine("File Access Write failed:{0:x8}", g_nRet);
                }
            }
        }

        static void Main(string[] args)
        {
            device = new MyCamera();
            int nRet = MyCamera.MV_OK;

            do
            {
                // 枚举设备
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
                        Console.WriteLine("\n Serial Number : " + stUsb3DeviceInfo.chSerialNumber);
                        Console.WriteLine("\n Device Number : " + stUsb3DeviceInfo.nDeviceNumber);
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

                // 创建设备
                nRet = device.MV_CC_CreateDevice_NET(ref stDevInfo);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Create device failed:{0:x8}", nRet);
                    break;
                }

                // 打开设备
                nRet = device.MV_CC_OpenDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Open device failed:{0:x8}", nRet);
                    break;
                }

                //ch:读模式 |en:Read mode
                Console.WriteLine("Read to file");
                g_nMode = 1;

                Thread hReadHandle = new Thread(FileAccessThread);
                hReadHandle.Start();

                Thread.Sleep(5);

                Thread hReadProgressHandle = new Thread(FileAccessProgress);
                hReadProgressHandle.Start();

                hReadProgressHandle.Join();
                hReadHandle.Join();
                if (MyCamera.MV_OK == g_nRet)
                {
                    Console.WriteLine("File Access Read Success");
                }

                Console.WriteLine("");

                //ch:写模式 |en:Write mode
                Console.WriteLine("Write to file");
                g_nMode = 2;

                Thread hWriteHandle = new Thread(FileAccessThread);
                hWriteHandle.Start();
                
                Thread.Sleep(5);

                Thread hWriteProgressHandle = new Thread(FileAccessProgress);
                hWriteProgressHandle.Start();

                hWriteProgressHandle.Join();
                hWriteHandle.Join();
                if (MyCamera.MV_OK == g_nRet)
                {
                    Console.WriteLine("File Access Write Success");
                }

                // 关闭设备
                nRet = device.MV_CC_CloseDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Close device failed{0:x8}", nRet);
                    break;
                }

                // 销毁设备
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
