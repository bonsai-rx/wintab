using System;
using System.Runtime.InteropServices;
using WintabDN;

namespace Bonsai.Wintab
{
    /// <summary>
    /// Represents a data object containing all acquired values from the Wintab
    /// tablet device.
    /// </summary>
    public class WintabDataFrame
    {
        internal WintabDataFrame(WintabPacket packet)
        {
            Time = packet.pkTime;
            FrameCounter = packet.pkSerialNumber;
            Status = (WintabPacketStatus)packet.pkStatus;
            X = packet.pkX;
            Y = packet.pkY;
            Z = packet.pkZ;
            NormalPressure = packet.pkNormalPressure;
            TangentPressure = packet.pkTangentPressure;
            Orientation = new(packet.pkOrientation);
        }

        /// <summary>
        /// Gets a value specifying various status and error conditions.
        /// </summary>
        public WintabPacketStatus Status { get; }

        /// <summary>
        /// Gets the system time at which the event was posted.
        /// </summary>
        public uint Time { get; }

        /// <summary>
        /// Gets the sequential counter assigned to the data frame by the acquisition context.
        /// </summary>
        public uint FrameCounter { get; }

        /// <summary>
        /// Gets the scaled cursor location along the X axis.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets the scaled cursor location along the Y axis.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Gets the scaled cursor location along the Z axis.
        /// </summary>
        public int Z { get; }

        /// <summary>
        /// Gets the adjusted state of the normal pen pressure.
        /// </summary>
        public uint NormalPressure { get; }

        /// <summary>
        /// Gets the adjusted state of the tangential pen pressure.
        /// </summary>
        public uint TangentPressure { get; }

        /// <summary>
        /// Gets the pen orientation.
        /// </summary>
        public WintabOrientation Orientation { get; }
    }

    /// <summary>
    /// Represents the Wintab pen orientation.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct WintabOrientation
    {
        /// <summary>
        /// The clockwise rotation of the pen about the Z axis through a full circular range.
        /// </summary>
        public int Azimuth;

        /// <summary>
        /// The angle of the pen with the X-Y plane through a signed, semicircular range.
        /// Positive values specify an angle upward toward the positive Z axis; negative 
        /// values specify an angle downward toward the negative Z axis.
        /// </summary>
        public int Altitude;

        /// <summary>
        /// The clockwise rotation of the pen about its own major axis.
        /// </summary>
        public int Twist;

        internal WintabOrientation(WTOrientation orientation)
        {
            Azimuth = orientation.orAzimuth;
            Altitude = orientation.orAltitude;
            Twist = orientation.orTwist;
        }
    }

    /// <summary>
    /// Specifies various Wintab status and error conditions.
    /// </summary>
    [Flags]
    public enum WintabPacketStatus : uint
    {
        /// <summary>
        /// Specifies that the cursor is out of context.
        /// </summary>
        Proximity = EWintabPacketStatusValue.TPS_PROXIMITY,

        /// <summary>
        /// Specifies that the event queue for the context has overflowed.
        /// </summary>
        QueueError = EWintabPacketStatusValue.TPS_QUEUE_ERR,

        /// <summary>
        /// Specifies that the cursor is in the margin of the context.
        /// </summary>
        Margin = EWintabPacketStatusValue.TPS_MARGIN,

        /// <summary>
        /// Specifies that the cursor is out of the context, but that the 
        /// context has grabbed input while waiting for a button release event.
        /// </summary>
        Grab = EWintabPacketStatusValue.TPS_GRAB,

        /// <summary>
        /// Specifies that the cursor is in its inverted state.
        /// </summary>
        Invert = EWintabPacketStatusValue.TPS_INVERT
    }
}
