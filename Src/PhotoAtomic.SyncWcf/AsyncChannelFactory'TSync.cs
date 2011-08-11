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
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace PhotoAtomic.SyncWcf
{
    public partial class AsyncChannelFactory<TSync> where TSync : class
    {
        /// <summary>
        /// the internal wcf channel factory
        /// </summary>
        private ChannelFactory ChannelFactory { get; set; }             
      
        /// <summary>
        /// Builds an async channel factory
        /// </summary>
        /// <param name="endpointConfigurationName">the configuration name (if not specified uses the default one)</param>
        public AsyncChannelFactory(string endpointConfigurationName = "*")
        {
            ChannelFactory = MakeChannelFactoryInstance(endpointConfigurationName);
        }

        /// <summary>
        /// Builds an async channel factory
        /// </summary>
        /// <param name="binding">the binding to use</param>
        /// <param name="remoteAddress">the remote addess</param>
        public AsyncChannelFactory(Binding binding, EndpointAddress remoteAddress)
        {
            ChannelFactory = MakeChannelFactoryInstance(binding, remoteAddress);
        }
        
        /// <summary>
        /// Builds an async channel factory
        /// </summary>
        /// <param name="endpointConfigurationName">the endpoint configuration name</param>
        /// <param name="remoteAddress">the remote adress</param>
        public AsyncChannelFactory(string endpointConfigurationName, EndpointAddress remoteAddress)
        {
            ChannelFactory = MakeChannelFactoryInstance(endpointConfigurationName, remoteAddress);
        }

        /// <summary>
        /// makes an instance of the required channel factory type
        /// </summary>
        /// <param name="arguments">argument to pass to the internal channel factory constructor</param>
        /// <returns></returns>
        private static ChannelFactory MakeChannelFactoryInstance(params object[] arguments)
        {
            Type asyncType = GenerateAsyncType<TSync>();
            Type factoryType = GenerateFactoryType(asyncType);

            ChannelFactory channelFactory;

            channelFactory = (ChannelFactory)Activator.CreateInstance(factoryType, arguments);

            return channelFactory;
        }

        /// <summary>
        /// generates a channel factory for the asyncType
        /// </summary>
        /// <param name="asyncType">the type that represent the asyncrhounos version of the interface of the service</param>
        /// <returns>a type for the factory for the asyncType</returns>
        private static Type GenerateFactoryType(Type asyncType)
        {
            Type genericFactoryType = typeof(ChannelFactory<>);
            Type factoryType = genericFactoryType.MakeGenericType(new[] { asyncType });
            return factoryType;
        }

        /// <summary>
        /// transforms a synchronous type in its asynchronous version
        /// </summary>
        /// <typeparam name="T">the synchronous version of the service interfce</typeparam>
        /// <returns>a type that represents the asynchronous version of T</returns>
        private static Type GenerateAsyncType<T>() where T : class
        {
            var generator = new TypeGenerator();
            Type asynchType = generator.GenerateAsyncInterfaceFor<T>();
            return asynchType;
        }
    }
}
