namespace PhotoAtomic.Communication.Wcf.Silverlight
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.ServiceModel.Channels;
    using System.Windows.Threading;

    /// <summary>
    /// represent an async channel that could be used to invoke the server
    /// </summary>
    /// <typeparam name="TSync">the sync type on wich the channel is based on</typeparam>
    public class AsyncChannel<TSync>
    {
        /// <summary>
        /// dictionary of the begin methods
        /// </summary>
        private Dictionary<string, Delegate> beginMethods = new Dictionary<string, Delegate>();
        /// <summary>
        /// dictionary of the end methods
        /// </summary>
        private Dictionary<string, Delegate> endMethods = new Dictionary<string, Delegate>();
        /// <summary>
        /// async inner channel
        /// </summary>
        private IChannel channel;

        /// <summary>
        /// creates and initialize an AsyncChannel
        /// </summary>
        /// <param name="asynchType">the async type used internally by the inner channel</param>
        /// <param name="channel">the inner channel</param>
        protected internal AsyncChannel(Type asynchType, IChannel channel)
        {
            var syncType = typeof(TSync);
            var proxyType = channel.GetType();
            this.channel = channel;

            foreach (var method in syncType.GetMethods())
            {
                //Begin method
                var beginMethod = proxyType.GetMethod(string.Format("Begin{0}",method.Name));
                beginMethods.Add(
                    method.Name,
                    Delegate.CreateDelegate(
                        GetFuncDelegateTypeFor(beginMethod),
                        channel,
                        beginMethod));


                //EndMethod
                var endMethod = proxyType.GetMethod(string.Format("End{0}", method.Name));
                endMethods.Add(
                    method.Name,
                    Delegate.CreateDelegate(
                        GetFuncDelegateTypeFor(endMethod),
                        channel,
                        endMethod));                                
            }
        }

        /// <summary>
        /// return a Func type that represent the passe dmethod
        /// </summary>
        /// <param name="method">a method descriptor</param>
        /// <returns>returns a type that represent the method with the appropriate Func type delegate</returns>
        private Type GetFuncDelegateTypeFor(MethodInfo method)
        {
            var parameters = method.GetParameters();
            var genericFunction = GetFunctionType(parameters.Length);
            var parameterTypes = parameters.Select(x=>x.ParameterType).Union(new []{method.ReturnType}).ToArray();
            var function = genericFunction.MakeGenericType(parameterTypes);
            return function;
        }

        /// <summary>
        /// returns the correct func type with the required number of input parameres
        /// </summary>
        /// <param name="argumentNumber">the number of parameters of the function</param>
        /// <returns>a func type with the correct number of parameters</returns>
        private Type GetFunctionType(int argumentNumber)
        {
            switch (argumentNumber)
            {
                case 0: return typeof(Func<>);
                case 1: return typeof(Func<,>);
                case 2: return typeof(Func<,,>);
                case 3: return typeof(Func<,,,>);
                case 4: return typeof(Func<,,,,>);
                case 5: return typeof(Func<,,,,,>);
                case 6: return typeof(Func<,,,,,,>);
                case 7: return typeof(Func<,,,,,,,>);
                case 8: return typeof(Func<,,,,,,,,>);
                case 9: return typeof(Func<,,,,,,,,,>);
                case 10: return typeof(Func<,,,,,,,,,,>);
                case 11: return typeof(Func<,,,,,,,,,,,>);
                case 12: return typeof(Func<,,,,,,,,,,,,>);
                case 13: return typeof(Func<,,,,,,,,,,,,,>);
                case 14: return typeof(Func<,,,,,,,,,,,,,,>);
                case 15: return typeof(Func<,,,,,,,,,,,,,,,>);
                case 16: return typeof(Func<,,,,,,,,,,,,,,,,>);
            }
            throw new IndexOutOfRangeException("Function could have a maximum of 16 arguments");
        }

        /// <summary>
        /// Executes a synchronous operation defined on the Sync interface in an asynch fashon
        /// </summary>
        /// <typeparam name="TOut">the type of result expected from the invokation of the operaton</typeparam>
        /// <param name="requestInvokation">the request to perform on the channel expressed as Sync</param>
        /// <param name="responseAction">the action to perform when the reply arrives</param>
        /// <returns>an IAsyncResult for the asyncronous operation, it could be useful to check when the operation is completed</returns>
        public IAsyncResult ExecuteAsync<TOut>(Expression<Func<TSync, TOut>> requestInvokation, Action<TOut> responseAction, Action<Exception> onException = null)
        {
            var lambda = requestInvokation as LambdaExpression;
            var method = lambda.Body as MethodCallExpression;

            var argumentsValues = ExtractArgumentValuesList(method);
            
            Dispatcher dispatcher = System.Windows.Deployment.Current.Dispatcher;

            AsyncCallback callback = asyncResult =>
                {
                    try
                    {
                        TOut result = (TOut)endMethods[method.Method.Name].DynamicInvoke(asyncResult);
                        dispatcher.BeginInvoke(() => responseAction(result));
                    }
                    catch (Exception ex)
                    {
                        if (onException == null) return;
                        Exception exception = ex.InnerException ?? ex;
                        dispatcher.BeginInvoke(() => onException(ex));
                    }
                };
            
            argumentsValues.Add(callback);
            argumentsValues.Add(null);

            IAsyncResult invokationResult = BeginInvokation(method.Method.Name, argumentsValues);

            return invokationResult;                       
        }

        /// <summary>
        /// Executes a synchronous operation defined on the Sync interface in an asynch fashon
        /// </summary>
        /// <typeparam name="TOut">the type of result expected from the invokation of the operaton</typeparam>
        /// <param name="requestInvokation">the request to perform on the channel expressed as Sync</param>
        /// <param name="asyncState">an user defined object to transmit to the ascyn reply of the operation</param>
        /// <param name="responseAction">the action to perform when the reply arrives</param>
        /// <returns>an IAsyncResult for the asyncronous operation, it could be useful to check when the operation is completed</returns>
        public IAsyncResult ExecuteAsync<TOut>(Expression<Func<TSync, TOut>> requestInvokation, object asyncState, Action<TOut, object> responseAction, Action<Exception, object> onException = null)
        {
            var lambda = requestInvokation as LambdaExpression;
            var method = lambda.Body as MethodCallExpression;

            var argumentsValues = ExtractArgumentValuesList(method);

            Dispatcher dispatcher = System.Windows.Deployment.Current.Dispatcher;

            AsyncCallback callback = asyncResult =>
            {
                try
                {
                    TOut result = (TOut)endMethods[method.Method.Name].DynamicInvoke(asyncResult);
                    dispatcher.BeginInvoke(() => responseAction(result,asyncResult.AsyncState));
                }
                catch (Exception ex)
                {
                    if (onException == null) return;
                    Exception exception = ex.InnerException ?? ex;
                    dispatcher.BeginInvoke(() => onException(ex,asyncResult.AsyncState));
                }
            };

            argumentsValues.Add(callback);
            argumentsValues.Add(asyncState);

            IAsyncResult invokationResult = BeginInvokation(method.Method.Name, argumentsValues);

            return invokationResult;
        }


        /// <summary>
        /// begin the invokation of the method
        /// </summary>
        /// <param name="method">the method to invoke</param>
        /// <param name="argumentsValues">parameters to pass to the method</param>
        /// <returns>returns the asynch result status</returns>
        private IAsyncResult BeginInvokation(string methodName, List<object> argumentsValues)
        {
            IAsyncResult invokationResult = (IAsyncResult)
            beginMethods[methodName]
                .DynamicInvoke(argumentsValues.ToArray());
            return invokationResult;
        }

        /// <summary>
        /// generates a list of argument values decoding the expression
        /// </summary>
        /// <param name="method">the method to decode</param>
        /// <returns>a list ov values extracted fom the method request</returns>
        private static List<object> ExtractArgumentValuesList(MethodCallExpression method)
        {
            var argumentsValues =
                method
                .Arguments
                .Select(x =>
                    Expression
                    .Lambda(x)
                    .Compile()
                    .DynamicInvoke(null))
                .ToList();
            return argumentsValues;
        }
    }
}
