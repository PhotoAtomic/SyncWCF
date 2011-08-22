// -----------------------------------------------------------------------
// <copyright file="INonExistingTestService.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace PhotoAtomic.Communication.Wcf.Silverlight.Interface.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ServiceModel;

    [ServiceContract]
    public interface  INonExistingTestService
    {
        [OperationContract]
        int Operation(int value);
    }
}
