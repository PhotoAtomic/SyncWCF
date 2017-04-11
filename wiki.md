# How it works

SyncWcf is essentially a wrapper that sits on top of the ChannelFactory and IChannel classes and interfaces of the standard WCF implementation for Silverlight.

It is composed by 2 main classes that “wraps” the ChannelFactory (AsyncChannelFactory<TSync>) and IChannel (AsyncChannel<TSync>), and by two helper class that are used internally (TypeGenerator and Assigner<T>).

[![image](http://download.codeplex.com/download?ProjectName=syncwcf&DownloadId=271595 "image")](http://download.codeplex.com/download?ProjectName=syncwcf&DownloadId=271594)

### AsyncChannel

[![image](http://download.codeplex.com/download?ProjectName=syncwcf&DownloadId=271597 "image")](http://download.codeplex.com/download?ProjectName=syncwcf&DownloadId=271596)

Wraps an asynchronous version of TSync IChannel that implements the Begin<Operation> End<Operation> pattern. by exposing it as it was synchronous-like call on the syncrhonous interface<TSync>

Basically when you have to consume a service in Silverlight you have to re-write an async version of your service that a server side is instead exposed as implementation of an interface (TSync).

The idea of this class is to provide a set of methods that allows the user to consume the service by using the same method on TSync and not their async version, that is complex to maintain, time consuming to write, and difficult to invoke.

Moreover this class exposes similar methods and properties of the ones contained by the IChannel interface, where they have sense for this scenario.

For example the State property will reports the status of the channel, Open, Close, Abort (with all their overloads) performs the same operations supposed by an object that implements IChannel but following the sync-like style.

ExecuteAsync is the heart of this class and of the entire library: it allows to execute operations on the TSync interface, in an asynchronous way.

This class has a private constructor therefore instances of this class can be obtained uniquely by using the AsyncChannelFactory class.

### AsyncChannelFactory

[![image](http://download.codeplex.com/download?ProjectName=syncwcf&DownloadId=271599 "image")](http://download.codeplex.com/download?ProjectName=syncwcf&DownloadId=271598)

This class has the ability to build a new channel, in a similar way the ChannelFactory usually does for WCF.

The main purposes of this class is to create a channel by calling the “CreateChannel” method on it. It has a set of overloads that replicate the normal “CreateChannel” method overloads for WCF (where they have sense for the asynchronous scenario this library targets). Moreover it allows to access to the channel credentials trough the “Credential” property.

the difference form the normal Silverlight Channel factory is that AsyncChannelFactory takes a generic argument that MUST be an interface type, and represent the service interface.

The interesting part is that this interface is in “Synchronous” style, without the Begin<Operation>, End<Operation> method pair for each <Operation> implemented at server side. In fact if you share the service interface in a Portable Library assembly you can share exactly the same interface between your server and your client, and therefore exactly the same data types.

And this is a tremendous power in your hand:

*   no need to rewrite code for the Asynchronous version of your service.
*   zero chance to make errors
*   always updated client version of the service interface
*   no way to call an older version of method interface as you are really sharing the same interface type between server and client.

The CreateChannel method eventually enables to obtain a fully intialized version of the AsyncChannel class, which in turn could be used to perform Asynchrnous call to the service in Synchronous-like way.

Internally it uses the TypeGenerator class to obtain an asynchronous version of the synchronous interface of the channel

### TypeGenerator

[![image](http://download.codeplex.com/download?ProjectName=syncwcf&DownloadId=271601 "image")](http://download.codeplex.com/download?ProjectName=syncwcf&DownloadId=271600)

This is an helper class used to dynamically generate asynchronous version that respects the Begin<Operation> End<Operation> pattern of a sync style interface.

it is important to note that this class doesn’t generates any proxy: it only emits a Type that represent an interface.

### Assigner

[![image](http://download.codeplex.com/download?ProjectName=syncwcf&DownloadId=271603 "image")](http://download.codeplex.com/download?ProjectName=syncwcf&DownloadId=271602)

This is an helper class used by the AsyncChannel class in case the invoked method has out or ref parameters, and it is used to assign the return values back to these parameters before the "responseAction” is invoked, so the user code in the responseAction will find these out and ref parameters already updated with the values provided by the service.

To perform the assignation this class emits code as DynamicMethod, so it could be garbage collected when they are no more necessary, no proxy are generated, and the IL is emitted trough the use of Expression for maximum safety and optimization, no direct IL opcode emit is performed.

Moreover, as code emit could be slow, this is performed only when it is absolutely necessary, if a webservice has no out nor ref parameters for an operation, zero code is emitted so absolutely no performance penalty happens.
