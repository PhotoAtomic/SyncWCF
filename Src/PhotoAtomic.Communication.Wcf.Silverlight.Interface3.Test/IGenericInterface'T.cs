using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

//PhotoAtomic.Communication.Wcf.Silverlight.Interface3.Test.IGenericInterface
namespace PhotoAtomic.Communication.Wcf.Silverlight.Interface3.Test
{
    [ServiceContract]
    public interface IGenericInterface<T> where T : new()
    {
        [OperationContract]
        int AMethod(T paramA);
    }
}