namespace PhotoAtomic.Communication.Wcf.Silverlight
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /// <summary>
    /// creates asynch channel for the specified sync type
    /// </summary>
    /// <typeparam name="TSync">the sync type to use as reference</typeparam>
    public class AsyncChannelFactory<TSync> where TSync : class
    {
        /// <summary>
        /// generated an async channel for the specified sync type
        /// </summary>
        /// <param name="configurationName">configuration name (it will be used to retrieve the configuration in the configuration file for WCF)</param>
        /// <returns>an async channel for the synch type ready for the use</returns>
        /// <remarks>
        /// for the ServiceReferences.ClientConfig file: the async type will be generated in a .Async sub namespace of the TSync type
        /// if TSync is for example in MyApplication.MyNamespace.TSync
        /// the async version will be created with the name MyApplication.MyNamespace.Async.TSync
        /// use this async fullyqualified name in the ServiceReferences.ClientConfig for the contract attribute
        /// </remarks>
        public AsyncChannel<TSync> CreateChannel(string configurationName = "*")
        {
            Type genericFactoryType = typeof(ChannelFactory<>);
            var generator = new TypeGenerator();
            Type asynchType = generator.GenerateAsyncInterfaceFor<TSync>();

            Type factoryType = genericFactoryType.MakeGenericType(new[] { asynchType });

            ChannelFactory channelFactory = (ChannelFactory)Activator.CreateInstance(factoryType, configurationName);
            var createChannelMethod = factoryType.GetMethod("CreateChannel", new Type[0]);
            IChannel channel = (IChannel)createChannelMethod.Invoke(channelFactory, null);

            return new AsyncChannel<TSync>(asynchType, channel);
        }
    }
}
