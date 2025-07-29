using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using WintabDN;

namespace Bonsai.Wintab
{
    /// <summary>
    /// Represents an operator that produces a sequence of data packets from a
    /// Wacom tablet device.
    /// </summary>
    [Combinator(MethodName = nameof(Generate))]
    [Description("Produces a sequence of data packets from a Wacom tablet device.")]
    [WorkflowElementCategory(ElementCategory.Source)]
    public class WintabCapture
    {
        /// <summary>
        /// Gets or sets a value indicating whether the Wacom tablet data will directly
        /// control the operating system cursor position.
        /// </summary>
        public bool SystemCursor { get; set; }

        /// <summary>
        /// Produces a sequence of data packets from a Wacom tablet device.
        /// </summary>
        /// <returns>
        /// An observable sequence of <see cref="WintabPacket"/> objects.
        /// </returns>
        public IObservable<WintabPacket> Generate()
        {
            var systemCursor = SystemCursor;
            return Observable.Create<WintabPacket>(observer =>
            {
                var logContext = CWintabInfo.GetDefaultSystemContext(ECTXOptionValues.CXO_MESSAGES);
                if (systemCursor)
                    logContext.Options |= (uint)ECTXOptionValues.CXO_SYSTEM;
                else
                    logContext.Options &= ~(uint)ECTXOptionValues.CXO_SYSTEM;

                if (!logContext.Open())
                    throw new InvalidOperationException("Failed to get default Wintab context.");

                var wtData = new CWintabData(logContext);
                EventHandler<MessageReceivedEventArgs> handler = (sender, e) =>
                {
                    uint pktID = (uint)e.Message.WParam;
                    if (pktID != 0)
                    {
                        var pkt = wtData.GetDataPacket((uint)e.Message.LParam, pktID);
                        if (pkt.pkContext != 0)
                            observer.OnNext(pkt);
                    }
                };

                wtData.SetWTPacketEventHandler(handler);
                return Disposable.Create(() =>
                {
                    wtData.RemoveWTPacketEventHandler(handler);
                    logContext.Close();
                });
            });
        }
    }
}
