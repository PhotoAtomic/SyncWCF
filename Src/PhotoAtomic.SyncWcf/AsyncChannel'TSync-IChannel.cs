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
using System.Windows.Threading;

namespace PhotoAtomic.SyncWcf
{
    public partial class AsyncChannel<TSync>
    {

        public T GetProperty<T>() where T : class
        {
            return channel.GetProperty<T>();
        }

        /// <summary>
        /// Causes a communication object to transition immediately from its current state into the closed state.
        /// </summary>
        public void Abort()
        {
            channel.Abort();
        }

        public void Open()
        {
            channel.Open();
        }

        public void Open(TimeSpan timeout)
        {
            channel.Open(timeout);
        }


        public IAsyncResult Open(Action responseAction)
        {
            return Open(null, (asyncState) => responseAction(), null);
        }

        public IAsyncResult Open(object asyncState, Action<object> responseAction)
        {
            return Open(asyncState, (state) => responseAction(state), null);
        }

        public IAsyncResult Open(Action responseAction, Action<Exception> onException)
        {
            return Open(null, (asyncState) => responseAction(), (ex, asyncState) => onException(ex));
        }

        public IAsyncResult Open(object asyncState, Action<object> responseAction, Action<Exception,object> onException)
        {
            Dispatcher dispatcher = System.Windows.Deployment.Current.Dispatcher;

            AsyncCallback callback = asyncResult =>
            {
                try
                {
                    channel.EndOpen(asyncResult);
                    dispatcher.BeginInvoke(() =>
                    {                        
                        if (responseAction == null) return;
                        responseAction(asyncResult.AsyncState);
                    });
                }
                catch (Exception ex)
                {
                    if (onException == null) return;
                    Exception exception = ex.InnerException ?? ex;
                    dispatcher.BeginInvoke(() => onException(ex, asyncResult.AsyncState));
                }
            };

            IAsyncResult invokationResult = channel.BeginOpen(callback, asyncState);

            return invokationResult;
        }


        public void Close()
        {
            channel.Close();
        }

        public void Close(TimeSpan timeout)
        {
            channel.Close(timeout);
        }

        public IAsyncResult Close(Action responseAction)
        {
            return Close(null, (asyncState) => responseAction(), null);
        }

        public IAsyncResult Close(object asyncState, Action<object> responseAction)
        {
            return Close(asyncState, (state) => responseAction(state), null);
        }

        public IAsyncResult Close(Action responseAction, Action<Exception> onException)
        {
            return Close(null, (asyncState) => responseAction(), (ex, asyncState) => onException(ex));
        }

        public IAsyncResult Close(object asyncState, Action<object> responseAction, Action<Exception, object> onException)
        {
            Dispatcher dispatcher = System.Windows.Deployment.Current.Dispatcher;

            AsyncCallback callback = asyncResult =>
            {
                try
                {
                    channel.EndClose(asyncResult);
                    dispatcher.BeginInvoke(() =>
                    {
                        if (responseAction == null) return;
                        responseAction(asyncResult.AsyncState);
                    });
                }
                catch (Exception ex)
                {
                    if (onException == null) return;
                    Exception exception = ex.InnerException ?? ex;
                    dispatcher.BeginInvoke(() => onException(ex, asyncResult.AsyncState));
                }
            };

            IAsyncResult invokationResult = channel.BeginClose(callback, asyncState);

            return invokationResult;
        }
    }
}
