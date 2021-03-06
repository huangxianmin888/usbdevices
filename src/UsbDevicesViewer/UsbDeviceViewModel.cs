﻿namespace UsbDevicesViewer
{
    using System;

    using Vurdalakov;
    using Vurdalakov.UsbDevicesDotNet;

    public enum UsbDeviceType
    {
        Controller,
        Hub,
        Device
    }

    public class UsbDeviceViewModel : ViewModelBase
    {
        public String Vid { get; private set; }
        public String Pid { get; private set; }
        public String HubAndPort { get; private set; }
        public String DeviceId { get; private set; }
        public String DevicePath { get; private set; }
        public String Description { get; private set; }
        public String ParentDeviceId { get; private set; }
        public UsbDeviceType DeviceType { get; private set; }

        public ThreadSafeObservableCollection<NameValueTypeViewModel> Properties { get; private set; }

        public ThreadSafeObservableCollection<NameValueTypeViewModel> RegistryProperties { get; private set; }

        public ThreadSafeObservableCollection<NameValueTypeViewModel> Interfaces { get; private set; }

        #region treeview support

        public String TreeViewIcon { get; private set; }

        public String TreeViewTitle { get; private set; }

        public ThreadSafeObservableCollection<UsbDeviceViewModel> TreeViewItems { get; private set; }

        public UsbDeviceViewModel() // virtual root item
        {
            this.TreeViewIcon = "Images/usbroot.png";
            this.TreeViewTitle = "My Computer";
            this.TreeViewItems = new ThreadSafeObservableCollection<UsbDeviceViewModel>();
        }
        
        #endregion

        public UsbDeviceViewModel(UsbDevice usbDevice)
        {
            this.Properties = new ThreadSafeObservableCollection<NameValueTypeViewModel>();
            this.RegistryProperties = new ThreadSafeObservableCollection<NameValueTypeViewModel>();
            this.Interfaces = new ThreadSafeObservableCollection<NameValueTypeViewModel>();

            this.TreeViewItems = new ThreadSafeObservableCollection<UsbDeviceViewModel>();

            this.Refresh(usbDevice);
        }

        public void Refresh(UsbDevice usbDevice)
        {
            this.Vid = usbDevice.Vid;
            this.OnPropertyChanged(() => this.Vid);

            this.Pid = usbDevice.Pid;
            this.OnPropertyChanged(() => this.Pid);

            this.HubAndPort = Helpers.MakeHubAndPort(usbDevice.Hub, usbDevice.Port);
            this.OnPropertyChanged(() => this.HubAndPort);

            this.DeviceId = usbDevice.DeviceId;
            this.OnPropertyChanged(() => this.DeviceId);

            this.DevicePath = usbDevice.DevicePath;
            this.OnPropertyChanged(() => this.DevicePath);

            this.Description = usbDevice.BusReportedDeviceDescription;
            this.OnPropertyChanged(() => this.Description);

            this.Properties.Clear();
            foreach (UsbDeviceProperty usbDeviceProperty in usbDevice.Properties)
            {
                String[] values = usbDeviceProperty.GetValues();

                this.Properties.Add(new NameValueTypeViewModel(usbDeviceProperty.GetDescription(), values[0], usbDeviceProperty.GetType()));

                if (usbDeviceProperty.HasSameKey(UsbDeviceWinApi.DevicePropertyKeys.DEVPKEY_Device_DeviceDesc))
                {
                    this.TreeViewTitle = values[0];
                    if (!String.IsNullOrEmpty(this.Description))
                    {
                        this.TreeViewTitle += String.Format(" ({0})", this.Description.Trim());
                    }
                }
                else if (usbDeviceProperty.HasSameKey(UsbDeviceWinApi.DevicePropertyKeys.DEVPKEY_Device_Parent))
                {
                    this.ParentDeviceId = values[0];
                }
                else if (usbDeviceProperty.HasSameKey(UsbDeviceWinApi.DevicePropertyKeys.DEVPKEY_Device_Service))
                {
                    switch (values[0].ToLower())
                    {
                        case "usbxhci": // usb3
                        case "usbuhci": // usb2
                        case "usbohci":
                        case "usbehci":
                        case "openhci": // usb11
                            this.DeviceType = UsbDeviceType.Controller;
                            this.TreeViewIcon = "Images/usbcontroller.png";
                            break;
                        case "usbhub3":
                        case "usbhub":
                            this.DeviceType = UsbDeviceType.Hub;
                            this.TreeViewIcon = "Images/usbhub.png";
                            break;
                        default:
                            this.DeviceType = UsbDeviceType.Device;
                            this.TreeViewIcon = "Images/usbdevice.png";
                            break;
                    }
                }

                for (int i = 1; i < values.Length; i++)
                {
                    this.Properties.Add(new NameValueTypeViewModel(String.Empty, values[i], String.Empty));
                }
            }

            this.RegistryProperties.Clear();
            foreach (UsbDeviceRegistryProperty usbDeviceRegistryProperty in usbDevice.RegistryProperties)
            {
                String[] values = usbDeviceRegistryProperty.GetValue();

                this.RegistryProperties.Add(new NameValueTypeViewModel(usbDeviceRegistryProperty.GetDescription(), values[0], usbDeviceRegistryProperty.GetType()));

                for (int i = 1; i < values.Length; i++)
                {
                    this.RegistryProperties.Add(new NameValueTypeViewModel(String.Empty, values[i], String.Empty));
                }
            }

            this.Interfaces.Clear();
            foreach (var deviceInterface in usbDevice.Interfaces)
            {
                this.Interfaces.Add(new NameValueTypeViewModel(String.Empty, deviceInterface.InterfaceId, String.Empty));
            }
        }
    }
}
