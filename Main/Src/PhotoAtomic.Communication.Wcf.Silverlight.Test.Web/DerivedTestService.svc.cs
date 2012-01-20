using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using PhotoAtomic.Communication.Wcf.Silverlight.Interface.Test;
using System.ServiceModel.Activation;

namespace PhotoAtomic.Reflection.Silverlight.Test.Web
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "DerivedTestService" in code, svc and config file together.
    [SilverlightFaultBehavior]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class DerivedTestService : IDerivedTestService
    {

        public void DerivedOperation()
        {
            int i = 0;            
        }

        public int Operation(int value)
        {
            return 0;
        }

        public int FaultingOperation(int value)
        {
            return 0;
        }

        public void VoidOperation(int value)
        {            
        }
    }
}
