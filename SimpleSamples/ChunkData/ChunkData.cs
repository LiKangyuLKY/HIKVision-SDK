using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MvCamCtrl.NET;
using System.Runtime.InteropServices;
using System.IO;

namespace ChunkData
{
    class ChunkData
    {
        public static MyCamera.cbOutputExdelegate ImageCallback;
        static MyCamera.MV_CHUNK_DATA_CONTENT stChunkInfo;// Chunk结构体信息
        static void ImageCallbackFunc(IntPtr pData, ref MyCamera.MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
        {
            //Print parse the timestamp information in the frame
            Console.WriteLine("ImageCallBack: ExposureTime[" + Convert.ToString(pFrameInfo.fExposureTime) 
                            + "], SecondCount[" + Convert.ToString(pFrameInfo.nSecondCount)
                            + "], CycleCount[" + Convert.ToString(pFrameInfo.nCycleCount) 
                            + "], CycleOffset[" + Convert.ToString(pFrameInfo.nCycleOffset)
                            + "], FrameNum[" + Convert.ToString(pFrameInfo.nFrameNum) + "]");

            int nStrSize = Marshal.SizeOf(stChunkInfo);
            int nUnparsedChunkContent = (int)pFrameInfo.UnparsedChunkList.pUnparsedChunkContent;
            for (int i = 0; i < pFrameInfo.nUnparsedChunkNum; i++)
            {
                stChunkInfo = (MyCamera.MV_CHUNK_DATA_CONTENT)Marshal.PtrToStructure((IntPtr)(nUnparsedChunkContent + i * nStrSize), typeof(MyCamera.MV_CHUNK_DATA_CONTENT));

                Console.WriteLine("ChunkInfo:" + "ChunkID[0x{0:x8}],ChunkLen[" + Convert.ToString(stChunkInfo.nChunkLen)+"]",stChunkInfo.nChunkID);
            }
            Console.WriteLine("************************************");
        }

        static void Main(string[] args)
        {
            int nRet = MyCamera.MV_OK;
            MyCamera device = new MyCamera();
            do
            {
                // ch:枚举设备 | en:Enum device
                MyCamera.MV_CC_DEVICE_INFO_LIST stDevList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
                nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE, ref stDevList);
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

                // ch:注册回调函数 | en:Register image callback
                ImageCallback = new MyCamera.cbOutputExdelegate(ImageCallbackFunc);
                nRet = device.MV_CC_RegisterImageCallBackEx_NET(ImageCallback, IntPtr.Zero);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Register image callback failed!");
                    break;
                }

                // ch:开启Chunk Mode | en:Open Chunk Mode
                nRet = device.MV_CC_SetBoolValue_NET("ChunkModeActive", true);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Set Chunk Mode failed:{0:x8}", nRet);
                    break;
                }

                // ch:Chunk Selector设为Exposure | en: Chunk Selector set as Exposure
                nRet = device.MV_CC_SetEnumValueByString_NET("ChunkSelector", "Exposure");
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Set Exposure Chunk failed:{0:x8}", nRet);
                    break;
                }

                // ch:开启Chunk Enable | en:Open Chunk Enable
                nRet = device.MV_CC_SetBoolValue_NET("ChunkEnable", true);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Set Chunk Enable failed:{0:x8}", nRet);
                    break;
                }

                // ch:Chunk Selector设为Timestamp | en: Chunk Selector set as Timestamp
                nRet = device.MV_CC_SetEnumValueByString_NET("ChunkSelector", "Timestamp");
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Set Timestamp Chunk failed:{0:x8}", nRet);
                    break;
                }

                // ch:开启Chunk Enable | en:Open Chunk Enable
                nRet = device.MV_CC_SetBoolValue_NET("ChunkEnable", true);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Set Chunk Enable failed:{0:x8}", nRet);
                    break;
                }

                // ch:设置触发模式为off || en:set trigger mode as off
                if (MyCamera.MV_OK != device.MV_CC_SetEnumValue_NET("TriggerMode", 0))
                {
                    Console.WriteLine("Set TriggerMode failed!");
                    break;
                }

                // ch:开启抓图 | en:start grab
                nRet = device.MV_CC_StartGrabbing_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Start grabbing failed:{0:x8}", nRet);
                    break;
                }

                Console.WriteLine("Press enter to exit");
                Console.ReadLine();

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
