using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace PhotoAtomic.Communication.Wcf.Silverlight.Interface2.Test
{
    [ServiceContract]
    public interface ITestServiceOut
    {
        [OperationContract]
        int Method(ref int a);

        [OperationContract]
        void MethodVoid(ref int a);

        [OperationContract]
        ComplexDataType ComplexMethod(ref ComplexDataType param);
    }
}
