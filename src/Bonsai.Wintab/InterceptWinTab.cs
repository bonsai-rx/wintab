
//#define CONSOLE_OUT
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using WintabDN;

namespace Bonsai.Wintab
{
    /// <summary>
    /// Class that hooks to Wintab and get callbacks from the wintab API
    /// Merges Wintab events into bonsai data streams 
    /// </summary>
    public class InterceptWintab
    {
        //ApplicationContext hookContext;
        //int hookCount;
        //Task hookTask;
        object gate;

        //UInt32 maxPackets = 10;
        const bool REMOVE = true;

        public IObservable<WintabPacket> WacomData { get; private set; }

        private Subject<WintabPacket> wacomData = new Subject<WintabPacket>();


        CWintabContext logContext;
        CWintabData wtData;

        //static readonly Lazy<InterceptWacom> instance = new Lazy<InterceptWacom>(() => new InterceptWacom());

        //public static InterceptWacom Instance
        //{
        //    get { return instance.Value; }
        //}

        public InterceptWintab(bool mouseControl)
        {
            wacomData = new Subject<WintabPacket>();
            //hookCount = 0;
            gate = new object();
            WacomData = Observable.Using(
                () => RegisterHook(),
                resource => wacomData)
                .PublishReconnectable()
                .RefCount();

            InitSystemDataCapture(mouseControl);
        }
        private IDisposable RegisterHook()
        {
            lock (gate)
            {
                //if (hookContext == null)
                //{
                //    ;
                //}

                //hookCount++;
            }

            return Disposable.Create(() =>
            {
                lock (gate)
                {
                    //if (--hookCount <= 0)
                    //{
                    //hookContext.ExitThread();
                    //hookContext = null;
                    //hookCount = 0;
                    //proc = null;
                    CloseCurrentContext();
                    //}
                }
            });
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Called when Wintab WT_PACKET events are received.
        /// </summary>
        /// <param name="sender_I">The EventMessage object sending the report.</param>
        /// <param name="eventArgs_I">eventArgs_I.Message.WParam contains ID of packet containing the data.</param>
        public void MyWTPacketEventHandler(Object sender_I, MessageReceivedEventArgs eventArgs_I)
        {
            if (wtData == null)
            {
                return;
            }

#if CONSOLE_OUT
            Console.WriteLine((uint)eventArgs_I.Message.WParam);
#endif
            //try
            //{

            uint pktID = (uint)eventArgs_I.Message.WParam;
            if (pktID != 0)
            {

                WintabPacket pkt;


                try
                {
                    pkt = wtData.GetDataPacket((uint)eventArgs_I.Message.LParam, pktID);
                }

                catch (Exception ex)
                {
                    throw new Exception("FAILED to get packet data: " + ex.ToString());
                }


                if (pkt.pkContext != 0)
                {
                    //m_pkX = pkt.pkX;
                    //m_pkY = pkt.pkY;
                    //m_pressure = pkt.pkNormalPressure;
                    wacomData.OnNext(pkt);
#if CONSOLE_OUT
                    Console.WriteLine("SCREEN: pkX: " + pkt.pkX + "/t pkY:" + pkt.pkY + "/t pressure: " + pkt.pkNormalPressure);
#endif

                    //m_pkTime = pkt.pkTime;

                    //if (m_graphics == null)
                    //{
                    //    // display data mode
                    //    TraceMsg("Received WT_PACKET event[" + pktID + "]: X/Y/NP/TP/time  " +
                    //        pkt.pkX + " \t " + pkt.pkY + "\t" + pkt.pkNormalPressure + "\t" + pkt.pkTangentPressure + "\t" + pkt.pkTime + "\n");
                    //}
                    //else
                    //{
                    //    Point clientPoint = testSplitContainer.Panel2.PointToClient(new Point(m_pkX, m_pkY));
                    //    //Trace.WriteLine("CLIENT:   X: " + clientPoint.X + ", Y:" + clientPoint.Y);

                    //    if (m_lastPoint.Equals(Point.Empty))
                    //    {
                    //        m_lastPoint = clientPoint;
                    //        m_pkTimeLast = m_pkTime;
                    //    }

                    //    float width = (float)m_pressure / (float)m_maxPressure;
                    //    int penIdx = (int)(width * 10) - 1; if (penIdx < 0) { penIdx = 0; }
                    //    //Debug.WriteLine($"pressure: {m_pressure}; width:{m_pen.Width}; penIdx: {penIdx}");

                    //    if (m_pressure > 0)
                    //    {
                    //        m_graphics.DrawLine(m_drawPens[penIdx], clientPoint, m_lastPoint);
                    //    }

                    //    m_lastPoint = clientPoint;
                    //    m_pkTimeLast = m_pkTime;
                    //}
                }

            }
            //}
            //catch (Exception ex)
            //{
            //	throw new Exception("FAILED to get packet data: " + ex.ToString());
            //}
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Opens a test system context for data capture
        /// </summary>
        /// <param name="ctrlSysCursor_I"></param>

        private void InitSystemDataCapture(bool ctrlSysCursor_I = true)
        {
            try
            {
                // Close context from any previous test.
                CloseCurrentContext();
#if CONSOLE_OUT
                Console.WriteLine("Opening context...\n");
#endif

                logContext = OpenDigitizerContext(ctrlSysCursor_I);//CWintabInfo.GetDefaultSystemContext();

                if (logContext == null)
                {
                    Console.Error.WriteLine("Test_DataPacketQueueSize: FAILED OpenTestSystemContext - bailing out...\n");
                    return;
                }

                // Create a data object and set its WT_PACKET handler.
                wtData = new CWintabData(logContext);
                wtData.SetWTPacketEventHandler(MyWTPacketEventHandler);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }

        ///////////////////////////////////////////////////////////////////////

        private void CloseCurrentContext()
        {
            try
            {
#if CONSOLE_OUT
                Console.WriteLine("Closing context...\n");
#endif
                if (wtData != null)
                {
                    wtData.RemoveWTPacketEventHandler(MyWTPacketEventHandler);
                    wtData = null;
                }
                if (logContext != null)
                {
                    logContext.Close();
                    logContext = null;
                }

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Opens a Wintab default system context
        /// </summary>
        /// <param name="ctrlSysCursor"></param>
        /// <returns></returns>

        private CWintabContext OpenDigitizerContext(bool ctrlSysCursor = true)
        {
            bool status = false;
            CWintabContext logContext = null;

            try
            {
                // Get the default system context.
                // Default is to receive data events.
                logContext = CWintabInfo.GetDefaultSystemContext(ECTXOptionValues.CXO_MESSAGES);

                // Set system cursor if caller wants it.
                if (ctrlSysCursor)
                {
                    logContext.Options |= (uint)ECTXOptionValues.CXO_SYSTEM;
                }
                else
                {
                    logContext.Options &= ~(uint)ECTXOptionValues.CXO_SYSTEM;
                }

                if (logContext == null)
                {
                    Console.Error.WriteLine("FAILED to get default wintab context.\n");
                    return null;
                }

                //// ----------------------------------------------------------------------
                //// Modify the tablet extents to set what part of the tablet is used.
                //Rectangle newTabletInRect = new Rectangle();
                //Rectangle newTabletOutRect = new Rectangle();

                //SetTabletExtents(ref logContext, newTabletInRect, newTabletOutRect);

                //// ----------------------------------------------------------------------
                //// Modify the system extents to control where cursor is allowed to move on desktop.
                //Rectangle newScreenRect = new Rectangle();

                //SetSystemExtents(ref logContext, newScreenRect);

                //logContext.Name = "WintabDN Query Data Context";
                //logContext.SysOrgX = logContext.SysOrgY = 0;
                //logContext.SysExtX = SystemInformation.PrimaryMonitorSize.Width;
                //logContext.SysExtY = SystemInformation.PrimaryMonitorSize.Height;
                // Open the context, which will also tell Wintab to send data packets.

                status = logContext.Open();
#if CONSOLE_OUT
                Console.WriteLine("Context Open: " + (status ? "PASSED [ctx=" + logContext.HCtx + "]" : "FAILED") + "\n");
#endif
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("OpenTestDigitizerContext ERROR: " + ex.ToString());
            }

            return logContext;
        }
    }
}
