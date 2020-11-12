using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MvCamCtrl.NET;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

namespace SetIODemo
{
    public partial class SetIODemo : Form
    {
        MyCamera.MV_CC_DEVICE_INFO_LIST m_pDeviceList;
        private MyCamera m_pMyCamera;
        bool m_bGrabbing;
        public SetIODemo()
        {
            InitializeComponent();
            m_pDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
            m_pMyCamera = new MyCamera();
            m_bGrabbing = false;
            DeviceListAcq();
        }

        // ch:显示错误信息 | en:Show error message
        private void ShowErrorMsg(string csMessage, int nErrorNum)
        {
            string errorMsg;
            if (nErrorNum == 0)
            {
                errorMsg = csMessage;
            }
            else
            {
                errorMsg = csMessage + ": Error =" + String.Format("{0:X}", nErrorNum);
            }

            switch (nErrorNum)
            {
                case MyCamera.MV_E_HANDLE: errorMsg += " Error or invalid handle "; break;
                case MyCamera.MV_E_SUPPORT: errorMsg += " Not supported function "; break;
                case MyCamera.MV_E_BUFOVER: errorMsg += " Cache is full "; break;
                case MyCamera.MV_E_CALLORDER: errorMsg += " Function calling order error "; break;
                case MyCamera.MV_E_PARAMETER: errorMsg += " Incorrect parameter "; break;
                case MyCamera.MV_E_RESOURCE: errorMsg += " Applying resource failed "; break;
                case MyCamera.MV_E_NODATA: errorMsg += " No data "; break;
                case MyCamera.MV_E_PRECONDITION: errorMsg += " Precondition error, or running environment changed "; break;
                case MyCamera.MV_E_VERSION: errorMsg += " Version mismatches "; break;
                case MyCamera.MV_E_NOENOUGH_BUF: errorMsg += " Insufficient memory "; break;
                case MyCamera.MV_E_UNKNOW: errorMsg += " Unknown error "; break;
                case MyCamera.MV_E_GC_GENERIC: errorMsg += " General error "; break;
                case MyCamera.MV_E_GC_ACCESS: errorMsg += " Node accessing condition error "; break;
                case MyCamera.MV_E_ACCESS_DENIED: errorMsg += " No permission "; break;
                case MyCamera.MV_E_BUSY: errorMsg += " Device is busy, or network disconnected "; break;
                case MyCamera.MV_E_NETER: errorMsg += " Network error "; break;
            }

            MessageBox.Show(errorMsg, "PROMPT");
        }

        private void bnEnum_Click(object sender, EventArgs e)
        {
            DeviceListAcq();
        }

        private void DeviceListAcq()
        {
            int nRet;
            //ch:创建设备列表 | en:Create Device List
            System.GC.Collect();
            cbDeviceList.Items.Clear();
            nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_pDeviceList);
            if (0 != nRet)
            {
                ShowErrorMsg("Enumerate devices fail!", nRet);
                return;
            }

            //ch:在窗体列表中显示设备名 | en:Display device name in the form list
            for (int i = 0; i < m_pDeviceList.nDeviceNum; i++)
            {
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    if (gigeInfo.chUserDefinedName != "")
                    {
                        cbDeviceList.Items.Add("GigE: " + gigeInfo.chUserDefinedName);
                    }
                    else
                    {
                        cbDeviceList.Items.Add("GigE: " + gigeInfo.chManufacturerName + " " + gigeInfo.chModelName + " (" + gigeInfo.chSerialNumber + ")");
                    }
                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    if (usbInfo.chUserDefinedName != "")
                    {
                        cbDeviceList.Items.Add("USB: " + usbInfo.chUserDefinedName);
                    }
                    else
                    {
                        cbDeviceList.Items.Add("USB: " + usbInfo.chManufacturerName + " " + usbInfo.chModelName + " (" + usbInfo.chSerialNumber + ")");
                    }
                }

            }

            //ch:选择第一项 | en:Select the first item
            if (m_pDeviceList.nDeviceNum != 0)
            {
                cbDeviceList.SelectedIndex = 0;
            }
        }

        private void SetCtrlWhenOpen()
        {
            bnOpen.Enabled = false;

            bnClose.Enabled = true;

            bnGetLineSel.Enabled  = true;
            bnSetLineSel.Enabled  = true;
            bnGetLineMode.Enabled = true;
            bnSetLineMode.Enabled = true;

        }

        private void bnOpen_Click(object sender, EventArgs e)
        {
            if (m_pDeviceList.nDeviceNum == 0 || cbDeviceList.SelectedIndex == -1)
            {
                ShowErrorMsg("No device, please select", 0);
                return;
            }
            int nRet = -1;

            //ch:获取选择的设备信息 | en:Get selected device information
            MyCamera.MV_CC_DEVICE_INFO device = 
                (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[cbDeviceList.SelectedIndex],
                                                              typeof(MyCamera.MV_CC_DEVICE_INFO));

            //ch:打开设备 | en:Open device
            //ch:打开设备 | en:Open Device
            nRet = m_pMyCamera.MV_CC_CreateDevice_NET(ref device);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("Create Camera failed", nRet);
                m_pMyCamera.MV_CC_CloseDevice_NET();
                return;
            }

            nRet = m_pMyCamera.MV_CC_OpenDevice_NET();
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("Device open fail!", nRet);
                m_pMyCamera.MV_CC_CloseDevice_NET();
                return;
            }

            //ch:设置采集连续模式 | en:Set Continues Aquisition Mode
            m_pMyCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", 2);
            m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", 0);

            //ch:控件操作 | en:Control Operation
            SetCtrlWhenOpen();

        }

        private void SetCtrlWhenClose()
        {
            bnOpen.Enabled = true;

            bnClose.Enabled = false;

            bnGetLineSel.Enabled = false;
            bnSetLineSel.Enabled = false;
            bnGetLineMode.Enabled = false;
            bnSetLineMode.Enabled = false;
        }

        private void bnClose_Click(object sender, EventArgs e)
        {

            //ch:关闭设备 | en:Close Device
            m_pMyCamera.MV_CC_CloseDevice_NET();

            //ch:控件操作 | en:Control Operation
            SetCtrlWhenClose();

            //ch:取流标志位清零 | en:Reset flow flag bit
            m_bGrabbing = false;
        }

        private void bnGetLineSel_Click(object sender, EventArgs e)
        {
            int nRet;
            MyCamera.MVCC_ENUMVALUE stSelValue = new MyCamera.MVCC_ENUMVALUE();
            nRet = m_pMyCamera.MV_CC_GetEnumValue_NET("LineSelector", ref stSelValue);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("Get Fail!", nRet);
                return;
            }

            cbLineSel.Items.Clear();

            for (int i = 0; i < stSelValue.nSupportedNum; i++ )
            {
                cbLineSel.Items.Add("LineSelector" + stSelValue.nSupportValue[i]);
                if (stSelValue.nCurValue == stSelValue.nSupportValue[i])
                {
                    cbLineSel.SelectedIndex = i;
                }
            }

        }

        private void bnGetLineMode_Click(object sender, EventArgs e)
        {
            int nRet;
            MyCamera.MVCC_ENUMVALUE stModeValue = new MyCamera.MVCC_ENUMVALUE();
            nRet = m_pMyCamera.MV_CC_GetEnumValue_NET("LineMode", ref stModeValue);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("Get Fail!", nRet);
                return;
            }

            cbLineMode.Items.Clear();

            for (int i = 0; i < stModeValue.nSupportedNum; i++)
            {
                cbLineMode.Items.Add("LineMode" + stModeValue.nSupportValue[i]);
                if (stModeValue.nCurValue == stModeValue.nSupportValue[i])
                {
                    cbLineMode.SelectedIndex = i;
                }
            }
        }

        private void bnSetLineSel_Click(object sender, EventArgs e)
        {
            int nRet;

            if (cbLineSel.SelectedIndex == -1)
            {
                ShowErrorMsg("Please Select Output!", 0);
                return;
            }

            String strValue = cbLineSel.SelectedItem.ToString().Substring(12);
            UInt32 nValue = Convert.ToUInt32(strValue);
            nRet = m_pMyCamera.MV_CC_SetEnumValue_NET("LineSelector", nValue);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("Set Fail!", nRet);
                return;
            }

            ShowErrorMsg("Set Succeed!", 0);
        }

        private void bnSetLineMode_Click(object sender, EventArgs e)
        {
            int nRet;

            if (cbLineMode.SelectedIndex == -1)
            {
                ShowErrorMsg("Please Select Output!", 0);
                return;
            }

            String strValue = cbLineMode.SelectedItem.ToString().Substring(8);
            UInt32 nValue = Convert.ToUInt32(strValue);
            nRet = m_pMyCamera.MV_CC_SetEnumValue_NET("LineMode", nValue);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("Set Fail!", nRet);
                return;
            }

            ShowErrorMsg("Set Succeed!", 0);
        }

    }
}
