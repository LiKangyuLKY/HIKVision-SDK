using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MvCamCtrl.NET;
using System.Runtime.InteropServices;
using System.IO;

namespace SavePonitCloudData_3D
{
    class SavePonitCloudData_3D
    {
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

                // ch:判断设备是否是设置的3D格式 | en:Judge Whether the device is set to 3D format
                MyCamera.MVCC_ENUMVALUE EnumValue = new MyCamera.MVCC_ENUMVALUE();
                nRet = device.MV_CC_GetEnumValue_NET("PixelFormat", ref EnumValue);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Get the Camera format fail:{0:x8}", nRet);
                    break;
                }

				MyCamera.MvGvspPixelType ePixelFormat = (MyCamera.MvGvspPixelType)EnumValue.nCurValue;
                switch (ePixelFormat)
                {
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_Coord3D_ABC32:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_Coord3D_ABC32f:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_Coord3D_AB32:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_Coord3D_AB32f:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_Coord3D_AC32:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_Coord3D_AC32f:
                        {
                            nRet = MyCamera.MV_OK;
                            break;
                        }

                    default:
                        {
                            nRet = MyCamera.MV_E_SUPPORT;
                            break;
                        }
                }
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("This is not a supported 3D format!");
                    break;
                }
                
                // ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only works for the GigE camera)
                if (stDevInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    int nPacketSize = device.MV_CC_GetOptimalPacketSize_NET();
                    if (nPacketSize > 0)
                    {
                        nRet = device.MV_CC_SetIntValue_NET("GevSCPSPacketSize", Convert.ToUInt32(nPacketSize));
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

                // ch:获取触发模式的访问模式 | en:Get Access mode of trigger mode
                MyCamera.MV_XML_AccessMode pAccessMode = MyCamera.MV_XML_AccessMode.AM_NI;
                if (MyCamera.MV_OK != device.MV_XML_GetNodeAccessMode_NET("TriggerMode", ref pAccessMode))
                {
                    Console.WriteLine("Get Access mode of trigger mode fail! nRet [0x%x]\n", nRet);
                }
                else
                {
                    // ch:设置触发模式为off || en:set trigger mode as off
                    if (MyCamera.MV_OK != device.MV_CC_SetEnumValue_NET("TriggerMode", 0))
                    {
                        Console.WriteLine("Set TriggerMode failed!");
                        break;
                    }
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

                // ch:开启抓图 | en:start grab
                nRet = device.MV_CC_StartGrabbing_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Start grabbing failed:{0:x8}", nRet);
                    break;
                }

                uint nImageNum = 100;
                byte[] bSaveImageBuf = null;

                try
                {
                    // ch:申请足够大的缓存，用于保存获取到的图像
                    bSaveImageBuf = new byte[nPayloadSize * nImageNum];
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Malloc  Save buffer fail!\n");
                    break;
                }

                uint nSaveImageSize = nPayloadSize * nImageNum;

                // ch:已获取的总图片大小
                uint nSaveDataLen = 0;

                MyCamera.MV_FRAME_OUT stOutFrame = new MyCamera.MV_FRAME_OUT();
                for(uint i = 0;i < nImageNum; i++)
                {
                    nRet = device.MV_CC_GetImageBuffer_NET(ref stOutFrame, 1000);
                    if (nRet == MyCamera.MV_OK)
                    {
                        Console.WriteLine("Get One Frame:" + "Width[" + Convert.ToString(stOutFrame.stFrameInfo.nWidth) + "] , Height[" + Convert.ToString(stOutFrame.stFrameInfo.nHeight)
                                        + "] , FrameNum[" + Convert.ToString(stOutFrame.stFrameInfo.nFrameNum) + "]");
                        
                        if (nSaveImageSize > (nSaveDataLen + stOutFrame.stFrameInfo.nFrameLen))
                        {
                            // ch:将图像拷贝到pSaveImageBuf | Copy one frame of image to the buffer named pSaveImageBuf
                            Marshal.Copy(stOutFrame.pBufAddr, bSaveImageBuf, Convert.ToInt32(nSaveDataLen), Convert.ToInt32(stOutFrame.stFrameInfo.nFrameLen));
                            nSaveDataLen += stOutFrame.stFrameInfo.nFrameLen;
                        }

                        nRet = device.MV_CC_FreeImageBuffer_NET(ref stOutFrame);
                        if(nRet != MyCamera.MV_OK)
                        {
                            Console.WriteLine("Free Image Buffer fail:{0:x8}", nRet);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No data:{0:x8}", nRet);
                    }
                }

                MyCamera.MV_SAVE_POINT_CLOUD_PARAM stSavePoCloudPar = new MyCamera.MV_SAVE_POINT_CLOUD_PARAM();

                stSavePoCloudPar.nLinePntNum = stOutFrame.stFrameInfo.nWidth;
                stSavePoCloudPar.nLineNum = stOutFrame.stFrameInfo.nHeight * nImageNum;

                byte[] bDstImageBuf = new byte[stSavePoCloudPar.nLineNum * stSavePoCloudPar.nLinePntNum * (16 * 3 + 4) + 2048];
                uint nDstImageSize = stSavePoCloudPar.nLineNum * stSavePoCloudPar.nLinePntNum * (16 * 3 + 4) + 2048;

                stSavePoCloudPar.enPointCloudFileType = MyCamera.MV_SAVE_POINT_CLOUD_FILE_TYPE.MV_PointCloudFile_PLY;
                stSavePoCloudPar.enSrcPixelType = stOutFrame.stFrameInfo.enPixelType;
                stSavePoCloudPar.nSrcDataLen = nSaveDataLen;

                GCHandle hSrcData = GCHandle.Alloc(bSaveImageBuf, GCHandleType.Pinned);
                stSavePoCloudPar.pSrcData = hSrcData.AddrOfPinnedObject();

                GCHandle hDstData = GCHandle.Alloc(bDstImageBuf, GCHandleType.Pinned);
                stSavePoCloudPar.pDstBuf = hDstData.AddrOfPinnedObject();

                stSavePoCloudPar.nDstBufSize = nDstImageSize;

                //Save point cloud data
                nRet = device.MV_CC_SavePointCloudData_NET(ref stSavePoCloudPar);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Save point cloud data fail:{0:x8}", nRet);
                    break;
                }

                FileStream file = new FileStream("PointCloudData.ply", FileMode.Create, FileAccess.Write);
                file.Write(bDstImageBuf, 0, Convert.ToInt32(stSavePoCloudPar.nDstBufLen));
                file.Close();
                Console.WriteLine("Save point cloud data succeed");

                hSrcData.Free();
                hDstData.Free();

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
