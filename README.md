[![PhotoAtomic Logo color](http://download.codeplex.com/download?ProjectName=syncwcf&DownloadId=266882 "PhotoAtomic Research Lab.")](http://www.photoatomiclab.net "PhotoAtomic Research Lab.")

# SyncWcf overview

SyncWcf allows you to consume async WCF service easilly. it allows you to share the service interface in a portable library and use the synchronous style interface to perform async operations. It doesn't requires any magical code generation.  

SyncWcf is a library that allows Silverlight to call WCF service with ease.  
Usually you have two option:

1.  rely on the generated code in visual studio for service client ("add service reference and so on...")
2.  handwrite an Async version of the Sync interface you have used at the server side.

both of this have downsides:

*   generated code is usually a mess to maintain, and you (or your development team) have to remember to update the service reference each time the service interface change
*   on the other side if you handwrite the async version of the service interface you could potentially spend lot of time and again you have to remember to update it, plus you have to manage the invokation of the async pattern

there are scenarios where these points are a big problems, for examples in teams where there are developers with few experience with .NET, or where the development time is very critical or, again, your client side application must be very tight coupled to the server side, and you have to be absolutely sure that they share the exact same interface (even with additional attributes you have applied on it!)  

_this where SyncWcf came into place_  
by using the new Portable assebly you could:

*   share exactly the same interface and data types between server and client.
*   invoke the service using the synchrounous interface (no more Begin<Operation> End<Operation> but just <Operation>), and the asynchronicity is maintained!
*   by using lambdas your code could look like as it is synchrnous but in fact it executes asyncrhonously.

## Code samples

Below how the code changes if you use SyncWcf  

assume we have the following service interface on the server side

<div style="background-color: white; color: black;">

<pre>    [ServiceContract]
    <span style="color: blue;">public</span> <span style="color: blue;">interface</span> ITestService
    {
        [OperationContract]
        <span style="color: blue;">int</span> Operation(<span style="color: blue;">int</span> value);
    }</pre>

</div>

*   Client side service interface

_without SyncWcf_ (you have to write it by hand)

<div style="background-color: white; color: black;">

<pre>    [ServiceContract]
    <span style="color: blue;">public</span> <span style="color: blue;">interface</span> ITestService
    {
        [OperationContract(AsyncPattern = <span style="color: blue;">true</span>)]
        IAsyncResult BeginOperation(<span style="color: blue;">int</span> value, AsyncCallback callback, <span style="color: blue;">object</span> state);

        <span style="color: blue;">int</span> EndOperation(IAsyncResult result);
    }</pre>

</div>

_with SyncWcf_ (nothing to write and maintain)

<div style="background-color: white; color: black;">

<pre>     <span style="color: green;">//simply import the portable assembly that contains the service interface definition</span></pre>

</div>

*   Client side service call

_without SyncWcf_ (using the Async version of the Interface you have written by hand)

<div style="background-color: white; color: black;">

<pre>    ITestService testService = <span style="color: blue;">new</span> ChannelFactory<ITestService>(<span style="color: #a31515;">"*"</span>).CreateChannel();
    <span style="color: green;">// [...]</span>
    AsyncCallback asyncCallBack = <span style="color: blue;">delegate</span>(IAsyncResult result)
    {
        <span style="color: blue;">int</span> value = ((<span style="color: blue;">int</span>)result.AsyncState).EndOperation(result);
        <span style="color: blue;">this</span>.Dispatcher.BeginInvoke(<span style="color: blue;">delegate</span>
        {
            <span style="color: green;">// [...] Do something with the result</span>
        });
    };
    testService.BeginOperation(1, asyncCallBack, testService);</pre>

</div>

_with SyncWcf_ (using the Sync version of the Interface in the portable library)

<div style="background-color: white; color: black;">

<pre>    <span style="color: blue;">var</span> channel = <span style="color: blue;">new</span> AsyncChannelFactory<ITestService>().CreateChannel();

    channel.ExecuteAsync(
        ws => ws.Operation(1),
        result => 
        {
             <span style="color: green;">// [...] Do something with the result</span>
        });</pre>

</div>

*   Client side service configurationfile (ServiceReferences.ClientConfig)

_without SyncWcf_

<div style="background-color: white; color: black;">

<pre>    <span style="color: blue;"><</span><span style="color: #a31515;">client</span><span style="color: blue;">></span>
        <span style="color: blue;"><</span><span style="color: #a31515;">endpoint</span> 
            <span style="color: red;">address</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">../TestService.svc</span><span style="color: black;">"</span> 
            <span style="color: red;">binding</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">customBinding</span><span style="color: black;">"</span>
            <span style="color: red;">bindingConfiguration</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">CustomBinding_ITestService</span><span style="color: black;">"</span> 
            <span style="color: red;">contract</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">PhotoAtomic.Communication.Wcf.Silverlight.Interface.Test.ITestService</span><span style="color: black;">"</span>
            <span style="color: red;">name</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">CustomBinding_ITestService</span><span style="color: black;">"</span> 
        <span style="color: blue;">/></span>
    <span style="color: blue;"></</span><span style="color: #a31515;">client</span><span style="color: blue;">></span></pre>

</div>

_with SyncWcf_ (just ad ".Async" at the namespace that identifies where the service interface is)

<div style="background-color: white; color: black;">

<pre>    <span style="color: blue;"><</span><span style="color: #a31515;">client</span><span style="color: blue;">></span>
        <span style="color: blue;"><</span><span style="color: #a31515;">endpoint</span> 
            <span style="color: red;">address</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">../TestService.svc</span><span style="color: black;">"</span> 
            <span style="color: red;">binding</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">customBinding</span><span style="color: black;">"</span>
            <span style="color: red;">bindingConfiguration</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">CustomBinding_ITestService</span><span style="color: black;">"</span> 
            <span style="color: red;">contract</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">PhotoAtomic.Communication.Wcf.Silverlight.Interface.Test.Async.ITestService</span><span style="color: black;">"</span>
            <span style="color: red;">name</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">CustomBinding_ITestService</span><span style="color: black;">"</span> 
        <span style="color: blue;">/></span>
    <span style="color: blue;"></</span><span style="color: #a31515;">client</span><span style="color: blue;">></span></pre>

</div>

As you can see by using SyncWcf the code looks a way better and it is a lot simpler. moreover, by default the result callback is dispatched con the calling thread so there is no synch problem with the UI in Silverlight.

Last but not least: all the call are strongly typed, with a large use of type auto inferred: total intellisense support.

# Current Version

Current version is used in a real project, it resembles the WCF’s ChannelFactory – Channel pattern.  
Version [Version 2.0.1.0](http://syncwcf.codeplex.com/releases/view/71645 "Release 2.0.1.0") could be considered the first production grade release. Anyway please let me know any feedback you have, areas that requires improvements and ideas, I’ll be glad to know your point of view to make the library evolve in the right direction

**[Version 3.0.0.0](https://syncwcf.codeplex.com/releases/view/624762)** is a Silverlight 5 revamp of the project in Visual Studio 2015\. Five year later, it adds support for native Task and for generic services. The former is extensively tested, while the latter is derived from a community proposed patch, but it hasn't tested it carefully.

# License

The project is free for use

# About the author

[![Mosè Bottacini](http://download.codeplex.com/download?ProjectName=syncwcf&DownloadId=266888 "Mosè Bottacini")](http://download.codeplex.com/download?ProjectName=syncwcf&DownloadId=266887)

Mosè Bottacini

[PhotoAtomic Research Lab.](http://www.photoatomiclab.net)  
Technical Leader

Facebook: [http://www.facebook.com/mose.bottacini](http://www.facebook.com/mose.bottacini "http://www.facebook.com/mose.bottacini")

Twitter: [@PhotoAtomicLab](http://twitter.com/PhotoAtomicLab)
