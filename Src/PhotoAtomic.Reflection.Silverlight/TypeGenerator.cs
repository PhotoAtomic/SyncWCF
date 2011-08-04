using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using System.ServiceModel;
using System.Collections.Generic;

namespace PhotoAtomic.Reflection.Silverlight
{
    public class TypeGenerator
    {
      

        public Type GenerateAsyncInterfaceFor<TSync>() where TSync : class
        {
            Type syncType = typeof(TSync);
            if (!syncType.IsInterface) throw new InvalidOperationException("Only interface type could be transformed");

            var asynchAssemblyName = string.Format("{0}.Async", syncType.Namespace);

            TypeBuilder typeBuilder =
                GetModuleBuilder(asynchAssemblyName)
                .DefineType(
                    string.Format("{0}.Async.{1}",syncType.Namespace,syncType.Name),
                    ((syncType.IsPublic)? TypeAttributes.Public : 0) |
                    TypeAttributes.Abstract | 
                    TypeAttributes.Interface);

            foreach (var method in syncType.GetMethods())
            {
                AddBeginAsynchVersionForMethod(typeBuilder, method);
                AddEndAsynchVersionForMethod(typeBuilder, method);
            }


            var serviceContractConstructor = typeof(ServiceContractAttribute).GetConstructor(new Type[0]);
            var attribuiteBuilder =
                new CustomAttributeBuilder(
                    serviceContractConstructor,
                    new object[0]);

            typeBuilder.SetCustomAttribute(attribuiteBuilder);

            Type asyncType = typeBuilder.CreateType();
            return asyncType;
        }

        private void AddEndAsynchVersionForMethod(TypeBuilder typeBuilder, MethodInfo method)
        {
            string endMethodName = string.Format("End{0}", method.Name);

            var parametersTypeList = new []{typeof(IAsyncResult)};
            var parametersAttributeList = new []{ParameterAttributes.None};
            var parametersNameList = new[]{"asyncResult"};

            var methodBuilder =
                typeBuilder
                .DefineMethod(
                    endMethodName,
                    method.Attributes,
                    method.CallingConvention,
                    method.ReturnType,
                    parametersTypeList.ToArray());

            for (int i = 0; i < parametersTypeList.Count(); i++)
            {
                methodBuilder.DefineParameter(i + 1, parametersAttributeList[i], parametersNameList[i]);
            }
        }

        private void AddBeginAsynchVersionForMethod(TypeBuilder typeBuilder, MethodInfo method)
        {
            string beginMethodName = string.Format("Begin{0}",method.Name);

            var parametersTypeList = method.GetParameters().Select(x=>x.ParameterType).ToList();
            var parametersNameList = method.GetParameters().Select(x=>x.Name).ToList();
            var parametersAttributeList = method.GetParameters().Select(x=>x.Attributes).ToList();


            parametersTypeList.Add(typeof(AsyncCallback));
            parametersAttributeList.Add(ParameterAttributes.None);
            parametersNameList.Add("callBack");

            parametersTypeList.Add(typeof(object));
            parametersAttributeList.Add(ParameterAttributes.None);
            parametersNameList.Add("statusObject");

            var methodBuilder = 
                typeBuilder
                .DefineMethod(
                    beginMethodName,
                    method.Attributes,
                    method.CallingConvention,
                    typeof(IAsyncResult),
                    parametersTypeList.ToArray());

            for(int i = 0; i<parametersTypeList.Count(); i++){
                methodBuilder.DefineParameter(i + 1, parametersAttributeList[i], parametersNameList[i]);
            }

            var operationContractConstructor = typeof(OperationContractAttribute).GetConstructor(new Type[0]);
            var asynchPatternProperty = typeof(OperationContractAttribute).GetProperty("AsyncPattern");
            var attribuiteBuilder = 
                new CustomAttributeBuilder(
                    operationContractConstructor, 
                    new object[0], 
                    new[] { asynchPatternProperty }, 
                    new object[] { true });

            methodBuilder.SetCustomAttribute(attribuiteBuilder);

        }

        private ModuleBuilder GetModuleBuilder(string requiredAssemblyName)
        {
            AssemblyName assemblyName = new AssemblyName(requiredAssemblyName);
            AssemblyBuilder assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    assemblyName,
                    AssemblyBuilderAccess.Run);
            
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            return moduleBuilder;
                

        }
    }
}
