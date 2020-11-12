using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MvCamCtrl.NET;
using System.Runtime.InteropServices;
using System.IO;

namespace CamLBasicDemo
{
    class CamLBasicDemo
    {

        static MyCamera.cbExceptiondelegate pCallBackFunc;
        // Callback function
        static void cbExceptiondelegate(uint nMsgType, IntPtr pUser)
        {
            if (nMsgType == MyCamera.MV_EXCEPTION_DEV_DISCONNECT)
            {
                Console.WriteLine("MV_EXCEPTION_DEV_DISCONNECT");
            }
        }


        // Get the value of various feature nodes
        static int GetParameters(ref MyCamera device)
        {
            if (null == device)
            {
                return MyCamera.MV_E_PARAMETER;
            }

            int nRet = MyCamera.MV_OK;

            // Get value of Integer nodes. Such as, 'width' etc.
            MyCamera.MVCC_INTVALUE stIntVal = new MyCamera.MVCC_INTVALUE();
            nRet = device.MV_CC_GetIntValue_NET("Width", ref stIntVal);
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Get width failed:{0:x8}", nRet);
                return nRet;
            }
            Console.WriteLine("Current Width:{0:d}", stIntVal.nCurValue);

            // Get value of Enum nodes. Such as, 'TriggerMode' etc.
            MyCamera.MVCC_ENUMVALUE stEnumVal = new MyCamera.MVCC_ENUMVALUE();
            nRet = device.MV_CC_GetEnumValue_NET("TriggerMode", ref stEnumVal);
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Get Trigger Mode failed:{0:x8}", nRet);
                return nRet;
            }
            Console.WriteLine("Current TriggerMode:{0:d}", stEnumVal.nCurValue);

            // Get value of float nodes. Such as, 'AcquisitionFrameRate' etc.
            MyCamera.MVCC_FLOATVALUE stFloatVal = new MyCamera.MVCC_FLOATVALUE();
            nRet = device.MV_CC_GetFloatValue_NET("AcquisitionFrameRate", ref stFloatVal);
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Get AcquisitionFrameRate failed:{0:x8}", nRet);
                return nRet;
            }
            Console.WriteLine("Current AcquisitionFrameRate:{0:f}Fps", stFloatVal.fCurValue);

            // Get value of bool nodes. Such as, 'AcquisitionFrameRateEnable' etc.
            bool bBoolVal = false;
            nRet = device.MV_CC_GetBoolValue_NET("AcquisitionFrameRateEnable", ref bBoolVal);
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Get AcquisitionFrameRateEnable failed:{0:x8}", nRet);
                return nRet;
            }
            Console.WriteLine("Current AcquisitionFrameRateEnable:{0:d}", bBoolVal);

            // Get value of String nodes. Such as, 'DeviceUserID' etc.
            MyCamera.MVCC_STRINGVALUE stStrVal = new MyCamera.MVCC_STRINGVALUE();
            nRet = device.MV_CC_GetStringValue_NET("DeviceUserID", ref stStrVal);
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Get DeviceUserID failed:{0:x8}", nRet);
                return nRet;
            }
            Console.WriteLine("Current DeviceUserID:{0:s}", stStrVal.chCurValue);

            return MyCamera.MV_OK;
        }


        // Set the value of various feature nodes
        static int SetParameters(ref MyCamera device)
        {
            if (null == device)
            {
                return MyCamera.MV_E_PARAMETER;
            }

            int nRet = MyCamera.MV_OK;

            // Set value of Integer nodes. Such as, 'width' etc.
            nRet = device.MV_CC_SetIntValue_NET("Width", 200);
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Set Width failed:{0:x8}", nRet);
                return nRet;
            }

            // Set value of float nodes. Such as, 'AcquisitionFrameRate' etc.
            nRet = device.MV_CC_SetFloatValue_NET("AcquisitionFrameRate", 8.8f);
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Set AcquisitionFrameRate failed:{0:x8}", nRet);
                return nRet;
            }

            // Set value of bool nodes. Such as, 'AcquisitionFrameRateEnable' etc.
            nRet = device.MV_CC_SetBoolValue_NET("AcquisitionFrameRateEnable", true);
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Set AcquisitionFrameRateEnable failed:{0:x8}", nRet);
                return nRet;
            }

            // Set value of String nodes. Such as, 'DeviceUserID' etc.
            nRet = device.MV_CC_SetStringValue_NET("DeviceUserID", "UserIDChanged");
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Set DeviceUserID failed:{0:x8}", nRet);
                return nRet;
            }

            // Execute Command nodes. Such as, 'TriggerSoftware' etc.
            // precondition
            // Set value of Enum nodes. Such as, 'TriggerMode' etc.
            nRet = device.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Set TriggerMode failed:{0:x8}", nRet);
                return nRet;
            }
            nRet = device.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Set TriggerSource failed:{0:x8}", nRet);
                return nRet;
            }
            // execute command
            nRet = device.MV_CC_SetCommandValue_NET("TriggerSoftware");
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Execute TriggerSoftware failed:{0:x8}", nRet);
                return nRet;
            }

            return MyCamera.MV_OK;
        }


        static void Main(string[] args)
        {
            int nRet = MyCamera.MV_OK;
            MyCamera device = new MyCamera();
            bool bDevConnected = false;  //whether a device is conncected

            do
            {
                // Enum device
                MyCamera.MV_CC_DEVICE_INFO_LIST stDevList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
                nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_CAMERALINK_DEVICE, ref stDevList);
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

                MyCamera.MV_CC_DEVICE_INFO stDevInfo;
                // Print device info
                for (Int32 i = 0; i < stDevList.nDeviceNum; i++)
                {
                    stDevInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(stDevList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                    if (MyCamera.MV_CAMERALINK_DEVICE == stDevInfo.nTLayerType)
                    {
                        MyCamera.MV_CamL_DEV_INFO stCamLDeviceInfo = (MyCamera.MV_CamL_DEV_INFO)MyCamera.ByteToStruct(stDevInfo.SpecialInfo.stCamLInfo, typeof(MyCamera.MV_CamL_DEV_INFO));
                        Console.WriteLine(i.ToString() + ": [CamL] Serial Number : " + stCamLDeviceInfo.chSerialNumber);
                        Console.WriteLine("PortID : " + stCamLDeviceInfo.chPortID);
                        Console.WriteLine("chManufacturerName : " + stCamLDeviceInfo.chManufacturerName);
                    }
                    else
                    {
                        Console.WriteLine("Unknown Error.");
                    }
                }
                Console.WriteLine("Enum finish.");

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

                // Create device
                nRet = device.MV_CC_CreateDevice_NET(ref stDevInfo);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Create device failed:{0:x8}", nRet);
                    break;
                }

                // Open device
                nRet = device.MV_CC_OpenDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Open device failed:{0:x8}", nRet);
                    break;
                }
                Console.WriteLine("Open finish.");
                bDevConnected = true;

                // Register Exception Callback
                pCallBackFunc = new MyCamera.cbExceptiondelegate(cbExceptiondelegate);
                nRet = device.MV_CC_RegisterExceptionCallBack_NET(pCallBackFunc, IntPtr.Zero);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Register expection callback failed:{0:x8}", nRet);
                    break;
                }
                GC.KeepAlive(pCallBackFunc);

                /*******************characteristic interfaces for CameraLink device*********************/
                // Get supported bauderates of the combined device and host interface
                uint nBaudrateAblity = 0;
                nRet = device.MV_CAML_GetSupportBauderates_NET(ref nBaudrateAblity);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Get supported bauderate fail:{0:x8}", nRet);
                    break;
                }
                Console.WriteLine("Current device supported bauderate:{0:x8}", nBaudrateAblity);

                // Set device bauderate
                nRet = device.MV_CAML_SetDeviceBauderate_NET((uint)MyCamera.MV_CAML_BAUDRATE_115200);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Set device bauderate fail:{0:x8}", nRet);
                    break;
                }

                // Get the current device bauderate
                uint nCurrentBaudrate = 0;
                nRet = device.MV_CAML_GetDeviceBauderate_NET(ref nCurrentBaudrate);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Get device bauderate fail:{0:x8}", nRet);
                    break;
                }
                Console.WriteLine("Current device bauderate:{0:x8}", nCurrentBaudrate);

                /****************************properties configuration**********************************/
                // Get the value of various feature nodes
                nRet = GetParameters(ref device);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("GetParameters failed:{0:x8}", nRet);
                    break;
                }

                // Set the value of various feature nodes
                nRet = SetParameters(ref device);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("SetParameters failed:{0:x8}", nRet);
                    break;
                }

                // Close device
                nRet = device.MV_CC_CloseDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Close device failed{0:x8}", nRet);
                    break;
                }
                bDevConnected = false;

                // Destroy device
                nRet = device.MV_CC_DestroyDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Destroy device failed:{0:x8}", nRet);
                    break;
                }
                Console.WriteLine("\n Close finish.");
            } while (false);

            if (MyCamera.MV_OK != nRet)
            {
                // Ensure that the device is closed
                if ( bDevConnected )
                {
                    device.MV_CC_CloseDevice_NET();
                    bDevConnected = false;
                }
                // Destroy device
                device.MV_CC_DestroyDevice_NET();
            }

            Console.WriteLine("Press enter to exit");
            Console.ReadKey();
        }
    }
}
