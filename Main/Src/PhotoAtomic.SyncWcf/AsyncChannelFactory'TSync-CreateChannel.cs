namespace PhotoAtomic.SyncWcf
{
    using System;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    /// <summary>
    /// creates asynch channel for the specified sync type
    /// </summary>
    /// <typeparam name="TSync">the sync type to use as reference</typeparam>
    public partial class AsyncChannelFactory<TSync> where TSync : class
    {      

        /// <summary>
        /// Credentials
        /// </summary>
        public ClientCredentials Credentials
        {
            get
            {
                return ChannelFactory.Credentials;
            }
        }
        

        /// <summary>
        /// generated an async channel for the specified sync type
        /// </summary>        
        /// <returns>the channel created by the factory</returns>
        /// <remarks>
        /// for the ServiceReferences.ClientConfig file: the async type will be generated in a .Async sub namespace of the TSync type
        /// if TSync is for example in MyApplication.MyNamespace.TSync
        /// the async version will be created with the name MyApplication.MyNamespace.Async.TSync
        /// use this async fullyqualified name in the ServiceReferences.ClientConfig for the contract attribute
        /// </remarks>
        public AsyncChannel<TSync> CreateChannel()
        {
            return CreateAsyncChannel<TSync>(ChannelFactory);
        }

        /// <summary>
        /// Creates a channel that is used to send messages to a service at a specific endpoint address.
        /// </summary>
        /// <param name="address">the address</param>
        /// <returns>the channel created by the factory</returns>
        /// <remarks>
        /// for the ServiceReferences.ClientConfig file: the async type will be generated in a .Async sub namespace of the TSync type
        /// if TSync is for example in MyApplication.MyNamespace.TSync
        /// the async version will be created with the name MyApplication.MyNamespace.Async.TSync
        /// use this async fullyqualified name in the ServiceReferences.ClientConfig for the contract attribute
        /// </remarks>
        public AsyncChannel<TSync> CreateChannel(EndpointAddress address)
        {
            return CreateAsyncChannel<TSync>(ChannelFactory, address);
        }

        /// <summary>
        /// Creates a channel that is used to send messages to a service at a specific endpoint address through a specified transport address.
        /// </summary>
        /// <param name="address">The EndpointAddress that provides the location of the service.</param>
        /// <param name="via">The Uri that contains the transport address to which the channel sends messages.</param>
        /// <returns>the channel created by the factory</returns>
        /// <remarks>
        /// for the ServiceReferences.ClientConfig file: the async type will be generated in a .Async sub namespace of the TSync type
        /// if TSync is for example in MyApplication.MyNamespace.TSync
        /// the async version will be created with the name MyApplication.MyNamespace.Async.TSync
        /// use this async fullyqualified name in the ServiceReferences.ClientConfig for the contract attribute
        /// </remarks>
        public AsyncChannel<TSync> CreateChannel(EndpointAddress address, Uri via)
        {
            return CreateAsyncChannel<TSync>(ChannelFactory, address, via);
        }

        /// <summary>
        /// Creates a channel
        /// </summary>
        /// 
        /// <param name="channelFactory">the channel factory instace for the channel creation</param>
        /// <returns></returns>
        private static AsyncChannel<T> CreateAsyncChannel<T>(ChannelFactory channelFactory,params object[] arguments)
        {
            Type factoryType = channelFactory.GetType();
            var createChannelMethod = factoryType.GetMethod("CreateChannel", arguments.Select(x=>x.GetType()).ToArray());
            IChannel channel = (IChannel)createChannelMethod.Invoke(channelFactory, arguments);
            return new AsyncChannel<T>(channel);
        }


    }
}
