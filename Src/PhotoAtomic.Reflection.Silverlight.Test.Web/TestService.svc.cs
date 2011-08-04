using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using PhotoAtomic.Communication.Wcf.Silverlight.Interface.Test;

namespace PhotoAtomic.Reflection.Silverlight.Test.Web
{    
    [SilverlightFaultBehavior]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class TestService : ITestService
    {
        public int Operation(int value)
        {
            return value + 1;
            
        }

        public int FaultingOperation(int value)
        {
            throw new NotImplementedException();
        }
    }
}
