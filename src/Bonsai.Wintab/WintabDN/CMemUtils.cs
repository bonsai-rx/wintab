///////////////////////////////////////////////////////////////////////////////
//
//	PURPOSE
//		Memory utility functions for WintabDN
//
//	COPYRIGHT
//		Copyright (c) 2010-2020 Wacom Co., Ltd.
//
//		The text and information contained in this file may be freely used,
//		copied, or distributed without compensation or licensing restrictions.
//
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace WintabDN
{
    /// <summary>
    /// Provide utility methods for unmanaged memory management.
    /// </summary>
    internal class CMemUtils
    {
        /// <summary>
        /// Allocates a pointer to unmanaged heap memory of sizeof(val_I).
        /// </summary>
        /// <param name="val_I">managed object that determines #bytes of unmanaged buf</param>
        /// <returns>Unmanaged buffer pointer.</returns>
        public static IntPtr AllocUnmanagedBuf(object val_I)
        {
            // Allocate a buffer of the correct size.
            int numBytes = Marshal.SizeOf(val_I);
            return Marshal.AllocHGlobal(numBytes);
        }

        /// <summary>
        /// Allocates a pointer to unmanaged heap memory of given size.
        /// </summary>
        /// <param name="size_I">number of bytes to allocate</param>
        /// <returns>Unmanaged buffer pointer.</returns>
        public static IntPtr AllocUnmanagedBuf(int size_I)
        {
            return Marshal.AllocHGlobal(size_I);
        }

        /// <summary>
        /// Marshals specified buf to the specified type.
        /// </summary>
        /// <typeparam name="T">type to which buf_I is marshalled</typeparam>
        /// <param name="buf_I">unmanaged heap pointer</param>
        /// <param name="size">expected size of buf_I</param>
        /// <returns>Managed object of specified type.</returns>
        public static T MarshalUnmanagedBuf<T>(IntPtr buf_I, int size)
        {
            // If size doesn't match type size, then return a zeroed struct.
            if (size != Marshal.SizeOf(typeof(T)))
            {
                int typeSize = Marshal.SizeOf(typeof(T));
                byte[] byteArray = new byte[typeSize];
                Marshal.Copy(byteArray, 0, buf_I, typeSize);
            }

            return (T)Marshal.PtrToStructure(buf_I, typeof(T));
        }

        /// <summary>
        /// Free unmanaged memory pointed to by buf_I.
        /// </summary>
        /// <param name="buf_I">pointer to unmanaged heap memory</param>
        public static void FreeUnmanagedBuf(IntPtr buf_I)
        {
            Marshal.FreeHGlobal(buf_I);
        }

        /// <summary>
        /// Marshals a string from an unmanaged buffer.
        /// </summary>
        /// <param name="buf_I">pointer to unmanaged heap memory</param>
        /// <param name="size_I">size of ASCII string, includes null termination</param>
        /// <returns></returns>
        public static string MarshalUnmanagedString(IntPtr buf_I, int size_I)
        {
            byte[] byteArray = new byte[size_I];
            Marshal.Copy(buf_I, byteArray, 0, size_I);
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;
            return encoding.GetString(byteArray);
        }

        /// <summary>
        /// Marshal unmanaged data packets into managed WintabPacket data.
        /// </summary>
        /// <param name="numPkts_I">number of packets to marshal</param>
        /// <param name="buf_I">pointer to unmanaged heap memory containing data packets</param>
        /// <returns></returns>
        public static WintabPacket[] MarshalDataPackets(uint numPkts_I, IntPtr buf_I)
        {
            WintabPacket[] packets = new WintabPacket[numPkts_I];
            if (numPkts_I == 0 || buf_I == IntPtr.Zero)
            {
                return packets;
            }

            int pktSize = Marshal.SizeOf(new WintabPacket());

            for (int pktsIdx = 0; pktsIdx < numPkts_I; pktsIdx++)
            {
                // Tracing can be added here to capture raw packet data if desired
                packets[pktsIdx] = (WintabPacket)Marshal.PtrToStructure(IntPtr.Add(buf_I, pktsIdx * pktSize), typeof(WintabPacket));
            }

            return packets;
        }

        /// <summary>
        /// Marshal unmanaged Extension data packets into managed WintabPacketExt data.
        /// </summary>
        /// <param name="numPkts_I">number of packets to marshal</param>
        /// <param name="buf_I">pointer to unmanaged heap memory containing data packets</param>
        /// <returns></returns>
        public static WintabPacketExt[] MarshalDataExtPackets(uint numPkts_I, IntPtr buf_I)
        {
            WintabPacketExt[] packets = new WintabPacketExt[numPkts_I];
            if (numPkts_I == 0 || buf_I == IntPtr.Zero)
            {
                return packets;
            }

            // Marshal each WintabPacketExt in the array separately.
            // This is "necessary" because none of the other ways I tried to marshal
            // seemed to work.  It's ugly, but it works.
            int pktSize = Marshal.SizeOf(new WintabPacketExt());
            byte[] byteArray = new byte[numPkts_I * pktSize];
            Marshal.Copy(buf_I, byteArray, 0, (int)numPkts_I * pktSize);

            byte[] byteArray2 = new byte[pktSize];

            for (int pktsIdx = 0; pktsIdx < numPkts_I; pktsIdx++)
            {
                for (int idx = 0; idx < pktSize; idx++)
                {
                    byteArray2[idx] = byteArray[pktsIdx * pktSize + idx];
                }

                IntPtr tmp = AllocUnmanagedBuf(pktSize);
                Marshal.Copy(byteArray2, 0, tmp, pktSize);

                packets[pktsIdx] = MarshalUnmanagedBuf<WintabPacketExt>(tmp, pktSize);
            }

            return packets;
        }
    }
}
