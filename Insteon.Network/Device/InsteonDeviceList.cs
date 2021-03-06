﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Insteon.Network.Device
{
    /// <summary>
    /// Represents the collection of known INSTEON devices within the INSTEON network.
    /// </summary>
    public class InsteonDeviceList : IEnumerable<InsteonDevice>
    {
        private readonly Dictionary<int, InsteonDevice> devices = new Dictionary<int, InsteonDevice>();
        private readonly InsteonNetwork network;

        internal InsteonDeviceList(InsteonNetwork network)
        {
            this.network = network;
        }

        /// <summary>
        /// Returns the number of devices in the known INSTEON device collection.
        /// </summary>
        public int Count => devices.Count;

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the collection.</returns>
        public IEnumerator GetEnumerator()
        {
            return devices.Values.GetEnumerator();
        }

        IEnumerator<InsteonDevice> IEnumerable<InsteonDevice>.GetEnumerator()
        {
            return devices.Values.GetEnumerator();
        }

        /// <summary>
        /// Invoked whenever a device is added to the network.
        /// </summary>
        public event InsteonDeviceEventHandler DeviceAdded;

        /// <summary>
        /// Invoked when a device fails to respond to a command within the timeout period of 2 seconds.
        /// </summary>
        public event InsteonDeviceEventHandler DeviceCommandTimeout;

        /// <summary>
        /// Invoked when a device is identified.
        /// </summary>
        public event InsteonDeviceEventHandler DeviceIdentified;

        /// <summary>
        /// Invoked when a status message is received from any known INSTEON device in the network, for example when a device turns on or off.
        /// </summary>
        public event InsteonDeviceStatusChangedEventHandler DeviceStatusChanged;

        /// <summary>
        /// Adds an INSTEON device to the list of known devices.
        /// </summary>
        /// <param name="address">The INSTEON address of the device to add.</param>
        /// <param name="identity">The INSTEON identity of the device to add.</param>
        /// <returns>Returns an object representing the specified device.</returns>
        /// <remarks>This method does not perform any INSTEON messaging, it only adds the specified device to a list of known devices.</remarks>
        public InsteonDevice Add(InsteonAddress address, InsteonIdentity identity)
        {
            if (identity.IsEmpty)
            {
                
                throw new Exception("Identity is empty.");    
            }

            if (devices.ContainsKey(address.Value))
            {
                return devices[address.Value];
            }

            InsteonDevice device = identity.GetDevice(network, address); 
            devices.Add(address.Value, device);
            OnDeviceAdded(device);
            return device;
        }

        /// <summary>
        /// Determiens whether the specified INSTEON device address is contained within the list of known devices.
        /// </summary>
        /// <param name="address">The specified INSTEON address.</param>
        /// <returns>Returns true if the list contains the specified INSTEON device.</returns>
        public bool ContainsKey(int address)
        {
            return devices.ContainsKey(address);
        }

        /// <summary>
        /// Determiens whether the specified INSTEON device address is contained within the list of known devices.
        /// </summary>
        /// <param name="address">The specified INSTEON address.</param>
        /// <returns>Returns true if the list contains the specified INSTEON device.</returns>
        public bool ContainsKey(InsteonAddress address)
        {
            return devices.ContainsKey(address.Value);
        }

        /// <summary>
        /// Finds the object representation of the specified device within the list of known devices.
        /// </summary>
        /// <param name="address">The specified INSTEON address.</param>
        /// <returns>Returns an object representing the specified INSTEON device.</returns>
        public InsteonDevice Find(int address)
        {
            return devices[address];
        }

        /// <summary>
        /// Finds the object representation of the specified device within the list of known devices.
        /// </summary>
        /// <param name="address">The specified INSTEON address.</param>
        /// <returns>Returns an object representing the specified INSTEON device.</returns>
        public InsteonDevice Find(InsteonAddress address)
        {
            return devices[address.Value];
        }

        internal void OnDeviceAdded(InsteonDevice device)
        {
            DeviceAdded?.Invoke(this, new InsteonDeviceEventArgs(device));
        }

        internal void OnDeviceCommandTimeout(InsteonDevice device)
        {
            DeviceCommandTimeout?.Invoke(this, new InsteonDeviceEventArgs(device));
        }

        internal void OnDeviceIdentified(InsteonDevice device)
        {
            DeviceIdentified?.Invoke(this, new InsteonDeviceEventArgs(device));
        }

        internal void OnDeviceStatusChanged(InsteonDevice device, InsteonDeviceStatus status)
        {
            DeviceStatusChanged?.Invoke(this, new InsteonDeviceStatusChangedEventArgs(device, status));
        }
    }
}