namespace PhotoAtomic.Communication.Wcf.Silverlight
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.ServiceModel;

    /// <summary>
    /// Can Generate Types
    /// It also haveinternal cache to not generate multiple times the same type
    /// </summary>
    public class TypeGenerator
    {
        /// <summary>
        /// internal cache for already generated types
        /// </summary>
        private static Dictionary<Type, Type> asyncTypeCache = new Dictionary<Type, Type>();      

        /// <summary>
        /// Generates the Async version of the TSync type.
        /// the generate type repects the AsyncPattern and it is already decorated with attributes for WCF operations
        /// </summary>
        /// <typeparam name="TSync">The Sync version of type</typeparam>
        /// <returns>A type that is the Async version of the TSync type, that implements the AsyncPattern for WCF</returns>
        public Type GenerateAsyncInterfaceFor<TSync>() where TSync : class
        {
            Type syncType = typeof(TSync);

            if (asyncTypeCache.ContainsKey(syncType)) return asyncTypeCache[syncType];

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

            asyncTypeCache.Add(syncType, asyncType);
            return asyncType;
        }

        /// <summary>
        /// Creates a End verison of a sync method, that implements the AsyncPattern
        /// </summary>
        /// <param name="typeBuilder">the tipebuilder where the type is being building</param>
        /// <param name="method">information about the sync version of the method</param>
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

        /// <summary>
        /// Creates a Begin verison of a sync method, that implements the AsyncPattern
        /// </summary>
        /// <param name="typeBuilder">the tipebuilder where the type is being building</param>
        /// <param name="method">information about the sync version of the method</param>
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

        /// <summary>
        /// provides a ModelBuilder with the required assembly name
        /// </summary>
        /// <param name="requiredAssemblyName">the assembly name for where the type will be generated in</param>
        /// <returns>a model builder</returns>
        /// <remarks>in this version the model builder is not cached, it could be interesting to generate all the types in the same assembly by caching the model builder</remarks>
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
