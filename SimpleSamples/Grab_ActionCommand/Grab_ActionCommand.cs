using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MvCamCtrl.NET;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace Grab_ActionCommand
{
    class Grab_ActionCommand
    {
        static bool g_bExit = false;
        static uint g_DeviceKey = 1;
        static uint g_GroupKey = 1;
        static uint g_GroupMask = 1;
        static uint g_nPayloadSize = 0;

        public static void ActionCommandWorkThread(object obj)
        {
            MyCamera device = obj as MyCamera;
            int nRet = MyCamera.MV_OK;
            MyCamera.MV_ACTION_CMD_INFO stActionCmdInfo = new MyCamera.MV_ACTION_CMD_INFO();
            MyCamera.MV_ACTION_CMD_RESULT_LIST stActionCmdResults = new MyCamera.MV_ACTION_CMD_RESULT_LIST();

            stActionCmdInfo.nDeviceKey = g_DeviceKey;
            stActionCmdInfo.nGroupKey = g_GroupKey;
            stActionCmdInfo.nGroupMask = g_GroupMask;
            stActionCmdInfo.pBroadcastAddress = "255.255.255.255";
            stActionCmdInfo.nTimeOut = 100;
            stActionCmdInfo.bActionTimeEnable = 0;

            MyCamera.MV_ACTION_CMD_RESULT pResults = new MyCamera.MV_ACTION_CMD_RESULT();
            int size = Marshal.SizeOf(pResults);
            while (!g_bExit)
            {
                //Send the PTP clock photo command
                nRet = device.MV_GIGE_IssueActionCommand_NET(ref stActionCmdInfo, ref stActionCmdResults);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Issue Action Command failed! nRet {0:x8}", nRet);
                    continue;
                }


                MyCamera.MV_ACTION_CMD_RESULT stTempActionCmd = new MyCamera.MV_ACTION_CMD_RESULT();
                var len = Marshal.SizeOf(stTempActionCmd) * stActionCmdResults.nNumResults;
                var targetPtr = Marshal.AllocHGlobal((int)len);
                unsafe
                {
                    byte* srcPtr = (byte*)stActionCmdResults.pResults.ToPointer();
                    byte* tmpPtr = (byte*)targetPtr.ToPointer();

                    for (int i = 0; i < len; i++)
                    {
                        *(tmpPtr + i) = *(srcPtr + i);
                    }

                    MyCamera.MV_ACTION_CMD_RESULT[] arrayMvActionCmdResult = PtrToStructurs<MyCamera.MV_ACTION_CMD_RESULT>(targetPtr, (int)stActionCmdResults.nNumResults);

                    for (uint i = 0; i < stActionCmdResults.nNumResults; i++)
                    {
                        //print the device infomation
                        Console.WriteLine("Ip == " + arrayMvActionCmdResult[i].strDeviceAddress + ", Status ==" + Convert.ToInt32(arrayMvActionCmdResult[i].nStatus));
                    }
                }
                Marshal.FreeHGlobal(targetPtr);
            }
        }
 
      public unsafe static T[] PtrToStructurs<T>(IntPtr pt, int lenth)
        {
            T[] structurs = new T[lenth];
            for (int i = 0; i < lenth; i++)
            {
                IntPtr ptr =new IntPtr((int)pt + (i * Marshal.SizeOf(typeof(T))));
                structurs[i] = (T)Marshal.PtrToStructure(ptr, typeof(T));
            }
            return structurs;
        } 

        public static void ReceiveImageWorkThread(object obj)
        {
            int nRet = MyCamera.MV_OK;
            MyCamera device = obj as MyCamera;
            MyCamera.MV_FRAME_OUT_INFO_EX stImageInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();
            IntPtr pData = Marshal.AllocHGlobal((int)g_nPayloadSize);
            if (pData == IntPtr.Zero)
            {
                return;
            }
            uint nDataSize = g_nPayloadSize;

            while (true)
            {
                nRet = device.MV_CC_GetOneFrameTimeout_NET(pData, nDataSize, ref stImageInfo, 1000);
                if (nRet == MyCamera.MV_OK)
                {
                    Console.WriteLine("Get One Frame:" + "Width[" + Convert.ToString(stImageInfo.nWidth) + "] , Height[" + Convert.ToString(stImageInfo.nHeight)
                                    + "] , FrameNum[" + Convert.ToString(stImageInfo.nFrameNum) + "]");
                }
                else
                {
                    Console.WriteLine("No data:{0:x8}", nRet);
                }
                if (g_bExit)
                {
                    break;
                }
            }
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
                    else
                    {
                        Console.Write("Not Support!\n");
                        break;
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

                // ch:设置触发模式为on | en:Set trigger mode as on
                nRet = device.MV_CC_SetEnumValue_NET("TriggerMode", 1);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Set Trigger Mode failed! {0:x8}", nRet);
                    break;
                }

                // ch:设置触发源为Action1 | en:Set trigger source as Action1
                nRet = device.MV_CC_SetEnumValue_NET("TriggerSource", 9);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Set Trigger Source failed! {0:x8}", nRet);
                    break;
                }

                // ch:设置Action Device Key | en:Set Action Device Key
                nRet = device.MV_CC_SetIntValue_NET("ActionDeviceKey", g_DeviceKey);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Set Action Device Key failed! {0:x8}", nRet);
                    break;
                }

                // ch:设置Action Group Key | en:Set Action Group Key
                nRet = device.MV_CC_SetIntValue_NET("ActionGroupKey", g_GroupKey);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Set Action Group Key failed! {0:x8}", nRet);
                    break;
                }

                // ch:设置Action Group Mask | en:Set Action Group Mask
                nRet = device.MV_CC_SetIntValue_NET("ActionGroupMask", g_GroupMask);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Set Action Group Mask fail! {0:x8}", nRet);
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
                g_nPayloadSize = stParam.nCurValue;

                // ch:开启抓图 | en:start grab
                nRet = device.MV_CC_StartGrabbing_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Start grabbing failed:{0:x8}", nRet);
                    break;
                }

                Thread hActionCommandThreadHandle = new Thread(ActionCommandWorkThread);
                hActionCommandThreadHandle.Start(device);

                Thread hReceiveImageThreadHandle = new Thread(ReceiveImageWorkThread);
                hReceiveImageThreadHandle.Start(device);

                Console.WriteLine("Press enter to exit");
                Console.ReadKey();

                g_bExit = true;
                Thread.Sleep(1000);

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