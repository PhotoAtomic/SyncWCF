using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using PhotoAtomic.Communication.Wcf.Silverlight.Interface2.Test;

namespace PhotoAtomic.Reflection.Silverlight.Test.Web
{
    [SilverlightFaultBehavior]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class TestServiceOut2 : ITestServiceOut
    {

        public int Method(ref int a)
        {
            a = 29;
            return 27;
        }


        public void MethodVoid(ref int a)
        {
            a = 210;
        }


        public ComplexDataType ComplexMethod(ref ComplexDataType param)
        {

            param = new ComplexDataType
            {
                Description = "2ref",
                Name = "2ref name",
                Id = 21
            };
            return new ComplexDataType
            {
                Description = "2out",
                Name = "2out name",
                Id = 22
            };

        }
    }
}
