﻿namespace PhotoAtomic.SyncWcf
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Windows.Threading;
    using System.Threading.Tasks;
    using System.Threading;
    /// <summary>
    /// represent an async channel that could be used to invoke the server
    /// </summary>  
    /// <typeparam name="TSync">type of synchronous channel</typeparam>
    public partial class AsyncChannel<TSync>
    {
        /// <summary>
        /// dictionary of the begin methods
        /// </summary>
        private Dictionary<string, Delegate> beginOperations = new Dictionary<string, Delegate>();

        /// <summary>
        /// dictionary of the end methods
        /// </summary>
        private Dictionary<string, Delegate> endOperations = new Dictionary<string, Delegate>();

        /// <summary>
        /// async inner channel
        /// </summary>
        internal IChannel channel;

        /// <summary>
        /// creates and initialize an AsyncChannel
        /// </summary>
        /// <param name="channel">the inner channel</param>
        protected internal AsyncChannel(IChannel channel)
        {
            var syncType = typeof(TSync);
            var proxyType = channel.GetType();
            this.channel = channel;

            foreach (var method in syncType.GetAllInterfaceMethods())
            {
                // Begin method
                var beginMethod = proxyType.GetMethod(string.Format("Begin{0}", method.Name));
                beginOperations.Add(
                    method.Name,
                    Delegate.CreateDelegate(
                        GetFuncDelegateTypeFor(beginMethod),
                        channel,
                        beginMethod));

                // End Method
                var endMethod = proxyType.GetMethod(string.Format("End{0}", method.Name));
                endOperations.Add(
                    method.Name,
                    Delegate.CreateDelegate(
                        GetFuncDelegateTypeFor(endMethod),
                        channel,
                        endMethod));
            }
        }

        /// <summary>
        /// Gets the current state of the communication-oriented object.
        /// </summary>
        public CommunicationState State
        {
            get
            {
                return channel.State;
            }
        }

        /// <summary>
        /// Executes a synchronous operation defined on the Sync interface in an asynch fashon
        /// </summary>        
        /// <param name="requestInvokation">the request to perform on the channel expressed as Sync</param>
        /// <param name="asyncState">an user defined object to transmit to the ascyn reply of the operation</param>
        /// <param name="responseAction">the action to perform when the reply arrives</param>
        /// <param name="onException">invoked when exception occurs</param>
        /// <returns>an IAsyncResult for the asyncronous operation, it could be useful to check when the operation is completed</returns>
        public IAsyncResult ExecuteAsync(Expression<Action<TSync>> requestInvokation, object asyncState, Action<object> responseAction, Action<Exception, object> onException)
        {
            var lambda = requestInvokation as LambdaExpression;
            var method = lambda.Body as MethodCallExpression;

            var argumentsValues = ExtractArgumentValuesList(method);

            Dispatcher dispatcher = System.Windows.Deployment.Current.Dispatcher;

            AsyncCallback callback = asyncResult =>
            {
                try
                {
                    var parameters = Enumerable.Repeat<object>(null, method.Arguments.Count + 1).ToArray();
                    parameters[method.Arguments.Count] = asyncResult;

                    endOperations[method.Method.Name].DynamicInvoke(parameters);
                    dispatcher.BeginInvoke(() =>
                        {
                            new Assigner<TSync>().Assign(requestInvokation, parameters);
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

            argumentsValues.Add(callback);
            argumentsValues.Add(asyncState);

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
        /// <param name="onException">invoked when exception occurs</param>
        /// <returns>an IAsyncResult for the asyncronous operation, it could be useful to check when the operation is completed</returns>
        public IAsyncResult ExecuteAsync<TOut>(Expression<Func<TSync, TOut>> requestInvokation, object asyncState, Action<TOut, object> responseAction, Action<Exception, object> onException)
        {
            var lambda = requestInvokation as LambdaExpression;
            var method = lambda.Body as MethodCallExpression;

            var argumentsValues = ExtractArgumentValuesList(method);

            Dispatcher dispatcher = System.Windows.Deployment.Current.Dispatcher;

            AsyncCallback callback = asyncResult =>
            {
                try
                {
                    var parameters = Enumerable.Repeat<object>(null, method.Arguments.Count + 1).ToArray();
                    parameters[method.Arguments.Count] = asyncResult;
                    TOut result = (TOut)endOperations[method.Method.Name].DynamicInvoke(parameters);
                    dispatcher.BeginInvoke(() =>
                        {
                            new Assigner<TSync>().Assign(requestInvokation, parameters);
                            if (responseAction == null) return;
                            responseAction(result, asyncResult.AsyncState);
                        });
                }
                catch (Exception ex)
                {
                    if (onException == null) return;
                    Exception exception = ex.InnerException ?? ex;
                    dispatcher.BeginInvoke(() => onException(ex, asyncResult.AsyncState));
                }
            };

            argumentsValues.Add(callback);
            argumentsValues.Add(asyncState);

            IAsyncResult invokationResult = BeginInvokation(method.Method.Name, argumentsValues);

            return invokationResult;
        }



        /// <summary>
        /// Executes a synchronous operation defined on the Sync interface in a task fashon
        /// </summary>        
        /// <param name="requestInvokation">the method being called</param>
        /// <param name="asyncState">an async state to pass to the task</param>
        /// <param name="options">task option creations</param>
        /// <returns></returns>
        public Task ExecuteTask(Expression<Action<TSync>> requestInvokation, object asyncState, TaskCreationOptions options)
        {
            var lambda = requestInvokation as LambdaExpression;
            var method = lambda.Body as MethodCallExpression;

            var argumentsValues = ExtractArgumentValuesList(method);

            Dispatcher dispatcher = System.Windows.Deployment.Current.Dispatcher;

            Action<IAsyncResult> callback = asyncResult =>
            {
                var parameters = Enumerable.Repeat<object>(null, method.Arguments.Count + 1).ToArray();
                parameters[method.Arguments.Count] = asyncResult;
                try
                {
                    endOperations[method.Method.Name].DynamicInvoke(parameters);
                    dispatcher.BeginInvoke(() =>
                    {
                        new Assigner<TSync>().Assign(requestInvokation, parameters);
                    });                    
                }
                catch (Exception ex)
                {
                    Exception exception = ex.InnerException ?? ex;
                    throw exception;
                }
            };

            argumentsValues.Add(null);
            argumentsValues.Add(asyncState);

            Func<AsyncCallback, object, IAsyncResult> invoker = (endCallback, state) =>
            {
                var result = BeginInvokation(method.Method.Name, argumentsValues);
                ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, (s, timeout) => endCallback(result), state, -1, true);

                return result;
            };

            return Task.Factory.FromAsync(invoker, callback, asyncState, options);
        }


        /// <summary>
        /// Executes a synchronous operation defined on the Sync interface in a task fashon
        /// </summary>
        /// <typeparam name="TOut">the type of the returned value</typeparam>
        /// <param name="requestInvokation">the method being called</param>
        /// <param name="asyncState">an async state to pass to the task</param>
        /// <param name="options">task option creations</param>
        /// <returns></returns>
        public Task<TOut> ExecuteTask<TOut>(Expression<Func<TSync, TOut>> requestInvokation, object asyncState, TaskCreationOptions options)
        {

            var lambda = requestInvokation as LambdaExpression;
            var method = lambda.Body as MethodCallExpression;

            var argumentsValues = ExtractArgumentValuesList(method);

            Dispatcher dispatcher = System.Windows.Deployment.Current.Dispatcher;

            Func<IAsyncResult,TOut> callback = asyncResult =>
            {                
                var parameters = Enumerable.Repeat<object>(null, method.Arguments.Count + 1).ToArray();
                parameters[method.Arguments.Count] = asyncResult;
                try
                {
                    TOut result = (TOut)endOperations[method.Method.Name].DynamicInvoke(parameters);
                    dispatcher.BeginInvoke(() =>
                    {
                        new Assigner<TSync>().Assign(requestInvokation, parameters);
                    });
                    return result;
                }
                catch (Exception ex)
                {
                    Exception exception = ex.InnerException ?? ex;
                    throw exception;                    
                }
            };

            argumentsValues.Add(null);
            argumentsValues.Add(asyncState);

            Func<AsyncCallback, object, IAsyncResult> invoker = (endCallback, state) =>
            {
                var result =  BeginInvokation(method.Method.Name, argumentsValues);
                ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, (s,timeout)=> endCallback(result),state,-1,true);

                return result;
            };
            
            return  Task.Factory.FromAsync<TOut>(invoker, callback, asyncState, options); 
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

        /// <summary>
        /// return a Func type that represent the passe dmethod
        /// </summary>
        /// <param name="method">a method descriptor</param>
        /// <returns>returns a type that represent the method with the appropriate Func type delegate</returns>
        private Type GetFuncDelegateTypeFor(MethodInfo method)
        {
            var parameters = method.GetParameters();
            var parameterTypes = parameters.Select(x => x.ParameterType).ToList();

            parameterTypes.Add(method.ReturnType);

            return Expression.GetDelegateType(parameterTypes.ToArray());
        }

        /// <summary>
        /// begin the invokation of the method
        /// </summary>
        /// <param name="methodName">the method to invoke</param>
        /// <param name="argumentsValues">parameters to pass to the method</param>
        /// <returns>returns the asynch result status</returns>
        private IAsyncResult BeginInvokation(string methodName, List<object> argumentsValues)
        {
            IAsyncResult invokationResult = (IAsyncResult)
            beginOperations[methodName]
                .DynamicInvoke(argumentsValues.ToArray());
            return invokationResult;
        }
    }
}
