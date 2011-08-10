﻿using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using PhotoAtomic.Communication.Wcf.Silverlight.Interface.Test;

namespace PhotoAtomic.Reflection.Silverlight.Test.Web
{
    [SilverlightFaultBehavior]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class TestServiceOut : ITestServiceOut
    {

        public int Method(ref int a)
        {
            a = 9;            
            return 7;
        }


        public void MethodVoid(ref int a)
        {
            a = 10;
        }
    }
}
