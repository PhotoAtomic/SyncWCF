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
using System.ServiceModel;

namespace PhotoAtomic.SyncWcf.TypeConverters
{
    /// <summary>
    /// Helper extensions for the AsyncChannel
    /// </summary>
    public static class AsyncChannelExtensions
    {
        /// <summary>
        /// provides a conversion of the AsyncChannel in a ContextChannel, useful to work with the OperationCOntext for example
        /// </summary>
        /// <typeparam name="TSync">the type of the async channel communication interface</typeparam>
        /// <param name="asyncChannel">the async channel to be converted</param>
        /// <returns>a IContextChannel version of the async comunication channel</returns>
        //HINT: probably it is better to duplicate the entire class hierarchy to enable the use with the 
        //OperationContextScope object so to have for example an AsyncOperationContextScope that natively operates with AsyncChannel instead of return on the WCF types
        //so to avoid misure of the internal channel object
        public static IContextChannel ToContextChannel<TSync>(this AsyncChannel<TSync> asyncChannel)
        {
            return asyncChannel.channel as IContextChannel;
        }
    }    
}
