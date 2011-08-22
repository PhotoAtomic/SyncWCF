using System.ServiceModel;
namespace PhotoAtomic.Communication.Wcf.Silverlight.Interface.Test
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
