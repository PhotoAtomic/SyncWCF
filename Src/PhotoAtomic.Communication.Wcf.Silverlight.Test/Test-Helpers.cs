using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Silverlight.Testing;

namespace PhotoAtomic.Reflection.Silverlight.Test
{
    public partial class Tests : SilverlightTest
    {
        private class MultipartComplete
        {
            private object syncRoot = new object();
            private int totalParts;
            private int partsCompleted = 0;
            private Action whenDone;

            public MultipartComplete(int totalParts, Action whenDone)
            {
                this.totalParts = totalParts;
                this.whenDone = whenDone;
            }

            public void Completed()
            {
                lock (syncRoot)
                {
                    partsCompleted++;
                    if (partsCompleted >= totalParts) whenDone();
                }
            }
        }
    }
}
