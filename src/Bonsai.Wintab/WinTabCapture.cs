using Bonsai;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Reactive.Subjects;
using WintabDN;

namespace Bonsai.Wintab
{
    /// <summary>
    /// Represents an operator that produces a sequence of values.
    /// </summary>
    [Description("Produces a sequence of values from a Wacom Device.")]
    [Combinator(MethodName = nameof(Generate))]
    [WorkflowElementCategory(ElementCategory.Source)]
    public class WintabCapture
    {
        /// <summary>
        /// "Produces a sequence of values from a Wacom Device."
        /// </summary>
        /// <returns>
        /// WacomWintabDN data
        /// </returns>
        //private WintabDN.CWintabContext logContext = null;
        //private CWintabData wtData = null;


        public bool MouseControl { get; set; }
        public IObservable<WintabPacket> Generate()
        {
            var wacom = new InterceptWintab(MouseControl);
            return wacom.WacomData;

        }
       

        
    }
}
