using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace PhotoAtomic.Communication.Wcf.Silverlight.Interface.Test
{
    [ServiceContract]
    public interface ITestService
    {
        [OperationContract]
        int Operation(int value);

        [OperationContract]
        int FaultingOperation(int value);
    }
}
