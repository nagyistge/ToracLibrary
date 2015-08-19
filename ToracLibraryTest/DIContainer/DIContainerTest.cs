﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToracLibrary.DIContainer;
using ToracLibrary.DIContainer.Exceptions;
using ToracLibrary.DIContainer.Parameters.ConstructorParameters;
using ToracLibrary.DIContainer.RegisteredObjects;
using static ToracLibrary.DIContainer.ToracDIContainer;

namespace ToracLibraryTest.UnitsTest.DIContainer
{

    /// <summary>
    /// Unit test to test the Torac DI Container
    /// </summary>
    [TestClass]
    public class DIContainerTest
    {

        #region Framework For Tests

        #region Constants

        /// <summary>
        /// what to write to the log to test
        /// </summary>
        private const string WriteToLog = "Test123";

        /// <summary>
        /// Holds the connection string to use when you pass it in to the SqlDIProvider
        /// </summary>
        private const string ConnectionStringToUse = "TestConn";

        #endregion

        private interface ILogger
        {
            StringBuilder LogFile { get; }
            void Log(string TextToLog);
        }

        private class Logger : ILogger
        {
            public StringBuilder LogFile { get; } = new StringBuilder();

            public void Log(string TextToLog)
            {
                LogFile.Append(TextToLog);
            }
        }

        private class SectionFactoryWithConstParam
        {
            public SectionFactoryWithConstParam(ILogger logger)
            {
                Log = logger;
            }

            public readonly ILogger Log;
        }

        private class SqlDIProvider
        {
            public SqlDIProvider(string ConnectionStringToSet, ILogger LoggerToSet)
            {
                ConnectionString = ConnectionStringToSet;
                LoggerToUse = LoggerToSet;
            }

            internal string ConnectionString { get; }
            internal ILogger LoggerToUse { get; }
        }

        private class SqlDIWithTypeProvider<T>
        {
            public Type GetTypeOfT()
            {
                return typeof(T);
            }
        }

        #endregion

        #region Unit Tests

        #region Exception Tests

        /// <summary>
        /// Let's make sure if we don't register an item, then when we go to resolve it, it will fail
        /// </summary>        
        [TestCategory("ToracLibrary.DIContainer")]
        [TestCategory("DIContainer")]
        [ExpectedException(typeof(TypeNotRegisteredException))]
        [TestMethod]
        public void NoRegistrationErrorTest1()
        {
            //declare my container
            var DIContainer = new ToracDIContainer();

            //let's grab an instance now, but we never registered it...so it should raise an error
            var LoggerToUse = DIContainer.Resolve<ILogger>();
        }

        /// <summary>
        /// Test multiple types with no factory name.This should blow up with a MultipleTypesFoundException error
        /// </summary>
        [TestCategory("ToracLibrary.DIContainer")]
        [TestCategory("DIContainer")]
        [ExpectedException(typeof(MultipleTypesFoundException))]
        [TestMethod]
        public void MultipleTypeFoundErrorTest1()
        {
            //this example would be for factories
            //PolicyFactory implements SectionFactory
            //ClaimFactory implements SectionFactory

            //declare my container
            var DIContainer = new ToracDIContainer();

            //register my item now with no overloads
            DIContainer.Register<ILogger, Logger>(DIContainerScope.Singleton);

            //register a second instance
            DIContainer.Register<ILogger, Logger>(DIContainerScope.Singleton);

            //now try to resolve it would should lead to an error
            DIContainer.Resolve<ILogger>();
        }

        #endregion

        #region Resolve

        /// <summary>
        /// Test the interface base transient for the DI container works
        /// </summary>
        [TestCategory("ToracLibrary.DIContainer")]
        [TestCategory("DIContainer")]
        [TestMethod]
        public void InterfaceBaseTransientTest1()
        {
            //declare my container
            var DIContainer = new ToracDIContainer();

            //register my item now with no overloads
            DIContainer.Register<ILogger, Logger>();

            //let's grab an instance now
            var LoggerToUse = DIContainer.Resolve<ILogger>();

            //make sure the logger is not null
            Assert.IsNotNull(LoggerToUse);

            //write test to the log
            LoggerToUse.Log(WriteToLog);

            //now let's check the log
            Assert.AreEqual(WriteToLog, LoggerToUse.LogFile.ToString());

            //let's ensure it's not using a singleton. Grab a new instance of ILogger, and make sure the result is empty
            Assert.AreEqual(string.Empty, DIContainer.Resolve<ILogger>().LogFile.ToString());
        }

        /// <summary>
        /// Test the interface base singleton for the DI container works
        /// </summary>
        [TestCategory("ToracLibrary.DIContainer")]
        [TestCategory("DIContainer")]
        [TestMethod]
        public void InterfaceBaseSingletonTest1()
        {
            //declare my container
            var DIContainer = new ToracDIContainer();

            //register my item now with no overloads
            DIContainer.Register<ILogger, Logger>(DIContainerScope.Singleton);

            //let's grab an instance now
            var LoggerToUse = DIContainer.Resolve<ILogger>();

            //make sure the logger is not null
            Assert.IsNotNull(LoggerToUse);

            //write test to the log
            LoggerToUse.Log(WriteToLog);

            //now let's check the log
            Assert.AreEqual(WriteToLog, LoggerToUse.LogFile.ToString());

            //its a singleton, so it should return the same instance which already has the test we wrote into it
            Assert.AreEqual(WriteToLog, DIContainer.Resolve<ILogger>().LogFile.ToString());
        }

        /// <summary>
        /// Test a concrete class to concrete class
        /// </summary>    
        [TestCategory("ToracLibrary.DIContainer")]
        [TestCategory("DIContainer")]
        [TestMethod]
        public void ConcreteToConcreteWithConstructorParameterLambdaTest1()
        {
            //declare my container
            var DIContainer = new ToracDIContainer();

            //register my item now with no overloads
            DIContainer.Register<ILogger, Logger>(DIContainerScope.Singleton);

            //let's register the data provider (since a string get's passed in, we need to specify how to create this guy)
            DIContainer.Register<SqlDIProvider>(DIContainerScope.Singleton)
                .WithConstructorImplementation(() => new SqlDIProvider(ConnectionStringToUse, DIContainer.Resolve<ILogger>()));

            //let's grab an the data provide rnow
            var DataProviderToUse = DIContainer.Resolve<SqlDIProvider>();

            //make sure the logger is not null
            Assert.IsNotNull(DataProviderToUse);

            //make sure the logger is not null
            Assert.IsNotNull(DataProviderToUse.LoggerToUse);

            //make sure the connection string is not null
            Assert.IsFalse(string.IsNullOrEmpty(DataProviderToUse.ConnectionString));

            //make sure the connection string is correct
            Assert.AreEqual(ConnectionStringToUse, DataProviderToUse.ConnectionString);

            //write test to the log
            DataProviderToUse.LoggerToUse.Log(WriteToLog);

            //now let's check the log
            Assert.AreEqual(WriteToLog, DataProviderToUse.LoggerToUse.LogFile.ToString());

            //its a singleton, so it should return the same instance which already has the test we wrote into it
            Assert.AreEqual(WriteToLog, DIContainer.Resolve<SqlDIProvider>().LoggerToUse.LogFile.ToString());
        }

        /// <summary>
        /// Test a concrete class to concrete class
        /// </summary>    
        [TestCategory("ToracLibrary.DIContainer")]
        [TestCategory("DIContainer")]
        [TestMethod]
        public void ConcreteToConcreteWithConstructorParameterTransientTest1()
        {
            //basically want to test a transient with a dependency in the constructor passed in

            //declare my container
            var DIContainer = new ToracDIContainer();

            //register my item now with no overloads
            DIContainer.Register<ILogger, Logger>(DIContainerScope.Transient);

            //let's register the data provider (since a string get's passed in, we need to specify how to create this guy)
            DIContainer.Register<SectionFactoryWithConstParam>(DIContainerScope.Transient);

            //let's grab an the data provide rnow
            var DataProviderToUse = DIContainer.Resolve<SectionFactoryWithConstParam>();
        }

        /// <summary>
        /// Test multiple types with a factory name
        /// </summary>
        [TestCategory("ToracLibrary.DIContainer")]
        [TestCategory("DIContainer")]
        [TestMethod]
        public void MultipleFactoriesWithFactoryNameTest1()
        {
            //this example would be for factories
            //PolicyFactory implements SectionFactory
            //ClaimFactory implements SectionFactory

            //declare my container
            var DIContainer = new ToracDIContainer();

            //store the factory names
            const string FactoryName1 = "FactoryName1";
            const string FactoryName2 = "FactoryName2";

            //what the 2nd factory will write
            const string Factory2LoggerTestString = "WriteToFactory2";

            //register my item now with no overloads
            DIContainer.Register<ILogger, Logger>().WithFactoryName(FactoryName1);

            //register a second instance
            DIContainer.Register<ILogger, Logger>().WithFactoryName(FactoryName2);

            //let's try to resolve the first guy
            var FactoryLogger1 = DIContainer.Resolve<ILogger>(FactoryName1);

            //grab the 2nd ILogger
            var FactoryLogger2 = DIContainer.Resolve<ILogger>(FactoryName2);

            //make sure the logger is not null
            Assert.IsNotNull(FactoryLogger1);
            Assert.IsNotNull(FactoryLogger2);

            //let's test both loggers now
            //write test to the log
            FactoryLogger1.Log(WriteToLog);

            //now let's check the log
            Assert.AreEqual(WriteToLog, FactoryLogger1.LogFile.ToString());

            //let's write to the 2nd logger
            FactoryLogger2.Log(Factory2LoggerTestString);

            //check the log
            Assert.AreEqual(Factory2LoggerTestString, FactoryLogger2.LogFile.ToString());
        }

        /// <summary>
        /// Let's make sure generic types work. Where the method has a generic parameter
        /// </summary>
        [TestCategory("ToracLibrary.DIContainer")]
        [TestCategory("DIContainer")]
        [TestMethod]
        public void InterfaceBaseGenericTypeTransientTest1()
        {
            //declare my container
            var DIContainer = new ToracDIContainer();

            //register my item now with no overloads
            DIContainer.Register<SqlDIWithTypeProvider<string>>();

            //let's grab an instance now
            var GenericTypeToUse = DIContainer.Resolve<SqlDIWithTypeProvider<string>>();

            //make sure the logger is not null
            Assert.IsNotNull(GenericTypeToUse);

            //grab the type of T
            Assert.AreEqual(typeof(string), GenericTypeToUse.GetTypeOfT());
        }

        /// <summary>
        /// Test a concrete class to concrete class. When passing in a set of parameters
        /// </summary>    
        [TestCategory("ToracLibrary.DIContainer")]
        [TestCategory("DIContainer")]
        [TestMethod]
        public void ConcreteToConcreteWithConstructorParameterPassedInSingletonTest1()
        {
            //declare my container
            var DIContainer = new ToracDIContainer();

            //register my item now with no overloads
            DIContainer.Register<ILogger, Logger>(DIContainerScope.Singleton);

            //let's register the data provider (since a string get's passed in, we need to specify how to create this guy)
            DIContainer.Register<SqlDIProvider>(DIContainerScope.Singleton)
                .WithConstructorParameters(new PrimitiveCtorParameter(ConnectionStringToUse), new ResolveCtorParameter(x => x.Resolve<ILogger>()));

            //let's grab an the data provide rnow
            var DataProviderToUse = DIContainer.Resolve<SqlDIProvider>();

            //make sure the logger is not null
            Assert.IsNotNull(DataProviderToUse);

            //make sure the logger is not null
            Assert.IsNotNull(DataProviderToUse.LoggerToUse);

            //make sure the connection string is not null
            Assert.IsFalse(string.IsNullOrEmpty(DataProviderToUse.ConnectionString));

            //make sure the connection string is correct
            Assert.AreEqual(ConnectionStringToUse, DataProviderToUse.ConnectionString);

            //write test to the log
            DataProviderToUse.LoggerToUse.Log(WriteToLog);

            //now let's check the log
            Assert.AreEqual(WriteToLog, DataProviderToUse.LoggerToUse.LogFile.ToString());

            //its a singleton, so it should return the same instance which already has the test we wrote into it
            Assert.AreEqual(WriteToLog, DIContainer.Resolve<SqlDIProvider>().LoggerToUse.LogFile.ToString());
        }

        /// <summary>
        /// Test a concrete class to concrete class. When passing in a set of parameters
        /// </summary>    
        [TestCategory("ToracLibrary.DIContainer")]
        [TestCategory("DIContainer")]
        [TestMethod]
        public void ConcreteToConcreteWithConstructorParameterPassedInTransientTest1()
        {
            //declare my container
            var DIContainer = new ToracDIContainer();

            //register my item now with no overloads
            DIContainer.Register<ILogger, Logger>(DIContainerScope.Transient);

            //let's register the data provider (since a string get's passed in, we need to specify how to create this guy)
            DIContainer.Register<SqlDIProvider>(DIContainerScope.Transient)
                .WithConstructorParameters(new PrimitiveCtorParameter(ConnectionStringToUse), new ResolveCtorParameter(x => x.Resolve<ILogger>()));

            //let's grab an the data provide rnow
            var DataProviderToUse = DIContainer.Resolve<SqlDIProvider>();

            //make sure the logger is not null
            Assert.IsNotNull(DataProviderToUse);

            //make sure the logger is not null
            Assert.IsNotNull(DataProviderToUse.LoggerToUse);

            //make sure the connection string is not null
            Assert.IsFalse(string.IsNullOrEmpty(DataProviderToUse.ConnectionString));

            //make sure the connection string is correct
            Assert.AreEqual(ConnectionStringToUse, DataProviderToUse.ConnectionString);

            //write test to the log
            DataProviderToUse.LoggerToUse.Log(WriteToLog);

            //now let's check the log
            Assert.AreEqual(WriteToLog, DataProviderToUse.LoggerToUse.LogFile.ToString());

            //its a transient, so it should return a new instance of the logger which will be empty
            Assert.AreEqual(string.Empty, DIContainer.Resolve<SqlDIProvider>().LoggerToUse.LogFile.ToString());
        }

        /// <summary>
        /// Test a concrete class to concrete class. When passing in a set of parameters
        /// </summary>    
        [TestCategory("ToracLibrary.DIContainer")]
        [TestCategory("DIContainer")]
        [TestMethod]
        public void ConcreteToConcreteWithConstructorParameterPassedInMixAndMatchTest1()
        {
            //declare my container
            var DIContainer = new ToracDIContainer();

            //register my item now with no overloads
            DIContainer.Register<ILogger, Logger>(DIContainerScope.Singleton);

            //let's register the data provider (since a string get's passed in, we need to specify how to create this guy)
            DIContainer.Register<SqlDIProvider>(DIContainerScope.Transient)
                .WithConstructorParameters(new PrimitiveCtorParameter(ConnectionStringToUse), new ResolveCtorParameter(x => x.Resolve<ILogger>()));

            //let's grab an the data provide rnow
            var DataProviderToUse = DIContainer.Resolve<SqlDIProvider>();

            //make sure the logger is not null
            Assert.IsNotNull(DataProviderToUse);

            //make sure the logger is not null
            Assert.IsNotNull(DataProviderToUse.LoggerToUse);

            //make sure the connection string is not null
            Assert.IsFalse(string.IsNullOrEmpty(DataProviderToUse.ConnectionString));

            //make sure the connection string is correct
            Assert.AreEqual(ConnectionStringToUse, DataProviderToUse.ConnectionString);

            //write test to the log
            DataProviderToUse.LoggerToUse.Log(WriteToLog);

            //now let's check the log
            Assert.AreEqual(WriteToLog, DataProviderToUse.LoggerToUse.LogFile.ToString());

            //the logger is a singleton, so it should return a new instance of the logger which will be empty
            Assert.AreEqual(WriteToLog, DIContainer.Resolve<SqlDIProvider>().LoggerToUse.LogFile.ToString());
        }

        /// <summary>
        /// Test a concrete class to concrete class. When passing in a set of parameters
        /// </summary>    
        [TestCategory("ToracLibrary.DIContainer")]
        [TestCategory("DIContainer")]
        [TestMethod]
        public void ConcreteToConcreteWithConstructorParameterPassedInWithResolveTypeSingletonTest1()
        {
            //declare my container
            var DIContainer = new ToracDIContainer();

            //register my item now with no overloads
            DIContainer.Register<ILogger, Logger>(DIContainerScope.Singleton);

            //let's register the data provider (since a string get's passed in, we need to specify how to create this guy)
            DIContainer.Register<SqlDIProvider>(DIContainerScope.Singleton)
                .WithConstructorParameters(new PrimitiveCtorParameter(ConnectionStringToUse), new ResolveTypeCtorParameter<ILogger>());

            //let's grab an the data provide rnow
            var DataProviderToUse = DIContainer.Resolve<SqlDIProvider>();

            //make sure the logger is not null
            Assert.IsNotNull(DataProviderToUse);

            //make sure the logger is not null
            Assert.IsNotNull(DataProviderToUse.LoggerToUse);

            //make sure the connection string is not null
            Assert.IsFalse(string.IsNullOrEmpty(DataProviderToUse.ConnectionString));

            //make sure the connection string is correct
            Assert.AreEqual(ConnectionStringToUse, DataProviderToUse.ConnectionString);

            //write test to the log
            DataProviderToUse.LoggerToUse.Log(WriteToLog);

            //now let's check the log
            Assert.AreEqual(WriteToLog, DataProviderToUse.LoggerToUse.LogFile.ToString());

            //its a singleton, so it should return the same instance which already has the test we wrote into it
            Assert.AreEqual(WriteToLog, DIContainer.Resolve<SqlDIProvider>().LoggerToUse.LogFile.ToString());
        }

        /// <summary>
        /// Test a concrete class to concrete class. When passing in a set of parameters
        /// </summary>    
        [TestCategory("ToracLibrary.DIContainer")]
        [TestCategory("DIContainer")]
        [TestMethod]
        public void ConcreteToConcreteWithConstructorParameterPassedInWithResolveTypeTransientTest1()
        {
            //declare my container
            var DIContainer = new ToracDIContainer();

            //declare the factory names so we don't have to have 2 methods with the same functionality
            const string FactoryWithGenericParameters = "FactoryWithGenericParameters";
            const string FactoryWithNonGenericParameters = "FactoryWithNonGenericParameters";

            //register ILogger
            DIContainer.Register<ILogger, Logger>(DIContainerScope.Transient);

            //register my item now with no overloads
            DIContainer.Register<SqlDIProvider>(DIContainerScope.Transient)
                .WithFactoryName(FactoryWithNonGenericParameters)
                .WithConstructorParameters(new PrimitiveCtorParameter(ConnectionStringToUse), new ResolveTypeNonGenericCtorParameter(typeof(ILogger)));

            //let's add the generic resolve ctor 
            DIContainer.Register<SqlDIProvider>(DIContainerScope.Transient)
                .WithFactoryName(FactoryWithGenericParameters)
                .WithConstructorParameters(new PrimitiveCtorParameter(ConnectionStringToUse), new ResolveTypeCtorParameter<ILogger>());

            //let's add the non generic version

            //let's grab an the data provide rnow
            var DataProviderToUseGeneric = DIContainer.Resolve<SqlDIProvider>(FactoryWithGenericParameters);
            var DataProviderToUseNonGeneric = DIContainer.Resolve<SqlDIProvider>(FactoryWithNonGenericParameters);

            //make sure the logger is not null
            Assert.IsNotNull(DataProviderToUseGeneric);
            Assert.IsNotNull(DataProviderToUseNonGeneric);

            //make sure the logger is not null
            Assert.IsNotNull(DataProviderToUseGeneric.LoggerToUse);
            Assert.IsNotNull(DataProviderToUseNonGeneric.LoggerToUse);

            //make sure the connection string is not null
            Assert.IsFalse(string.IsNullOrEmpty(DataProviderToUseGeneric.ConnectionString));
            Assert.IsFalse(string.IsNullOrEmpty(DataProviderToUseNonGeneric.ConnectionString));

            //make sure the connection string is correct
            Assert.AreEqual(ConnectionStringToUse, DataProviderToUseGeneric.ConnectionString);
            Assert.AreEqual(ConnectionStringToUse, DataProviderToUseNonGeneric.ConnectionString);

            //write test to the log
            DataProviderToUseGeneric.LoggerToUse.Log(WriteToLog);
            DataProviderToUseNonGeneric.LoggerToUse.Log(WriteToLog);

            //now let's check the log
            Assert.AreEqual(WriteToLog, DataProviderToUseGeneric.LoggerToUse.LogFile.ToString());
            Assert.AreEqual(WriteToLog, DataProviderToUseNonGeneric.LoggerToUse.LogFile.ToString());

            //its a transient, so it should return a new instance of the logger which will be empty
            Assert.AreEqual(string.Empty, DIContainer.Resolve<SqlDIProvider>(FactoryWithGenericParameters).LoggerToUse.LogFile.ToString());
            Assert.AreEqual(string.Empty, DIContainer.Resolve<SqlDIProvider>(FactoryWithNonGenericParameters).LoggerToUse.LogFile.ToString());
        }

        #endregion

        #region Resolve All

        /// <summary>
        /// Test resolve all when you have multiple factories
        /// </summary>
        [TestCategory("ToracLibrary.DIContainer")]
        [TestCategory("DIContainer")]
        [TestMethod]
        public void ResolveAllTest1()
        {
            //this example would be for factories
            //PolicyFactory implements SectionFactory
            //ClaimFactory implements SectionFactory

            //declare my container
            var DIContainer = new ToracDIContainer();

            //prefix for factory
            const string FactoryNamePrefix = "Factory";

            //store the factory names
            const string FactoryName1 = FactoryNamePrefix + "0";
            const string FactoryName2 = FactoryNamePrefix + "1";

            //register my item now with no overloads
            DIContainer.Register<ILogger, Logger>().WithFactoryName(FactoryName1);

            //register a second instance
            DIContainer.Register<ILogger, Logger>().WithFactoryName(FactoryName2);

            //count how many resolve all items we have
            int ResolveAllResultCount = 0;

            //lets loop through the resolve all
            foreach (var FactoryToResolve in DIContainer.ResolveAllLazy<ILogger>().OrderBy(x => x.Key))
            {
                //let's resolve based on the index (so index 0 is the first factory)
                Assert.AreEqual(string.Format($"{FactoryNamePrefix}{ResolveAllResultCount}"), FactoryToResolve.Key);

                //and just make sure the actual instance is not null
                Assert.IsNotNull(FactoryToResolve.Value);

                //increase the count
                ResolveAllResultCount++;
            }

            //make sure we have 2 factories that resolved
            Assert.AreEqual(2, ResolveAllResultCount);
        }

        #endregion

        #region Clear All

        /// <summary>
        /// Test clearing of all the registrations for a specific type
        /// </summary>
        [TestCategory("ToracLibrary.DIContainer")]
        [TestCategory("DIContainer")]
        [TestMethod]
        public void ClearAllRegistrationsForSpecificTypeTest1()
        {
            //declare my container
            var DIContainer = new ToracDIContainer();

            //we are mixing up the order below to make sure the removal works correctly

            //register my item now with no overloads
            DIContainer.Register<ILogger, Logger>().WithFactoryName(Guid.NewGuid().ToString());

            //let's register the data provider (since a string get's passed in, we need to specify how to create this guy)
            DIContainer.Register<SqlDIProvider>(DIContainerScope.Singleton)
                 .WithConstructorImplementation(() => new SqlDIProvider(ConnectionStringToUse, DIContainer.Resolve<ILogger>()));

            //register a second instance
            DIContainer.Register<ILogger, Logger>().WithFactoryName(Guid.NewGuid().ToString());

            //clear all the registrations
            DIContainer.ClearAllRegistrationsForSpecificType<ILogger>();

            //make sure we have 1 item left in the container, we can't resolve sql di provider because ilogger is a dependency!!!!!
            Assert.AreEqual(1, DIContainer.AllRegistrationSelectLazy().Count(x => x.ConcreteType == typeof(SqlDIProvider)));
        }

        /// <summary>
        /// Test clearing of all the registrations
        /// </summary>
        [TestCategory("ToracLibrary.DIContainer")]
        [TestCategory("DIContainer")]
        [TestMethod]
        public void ClearAllRegistrationsTest1()
        {
            //declare my container
            var DIContainer = new ToracDIContainer();

            //register my item now with no overloads
            DIContainer.Register<ILogger, Logger>().WithFactoryName(Guid.NewGuid().ToString());

            //register a second instance
            DIContainer.Register<ILogger, Logger>().WithFactoryName(Guid.NewGuid().ToString());

            //let's register the data provider (since a string get's passed in, we need to specify how to create this guy)
            DIContainer.Register<SqlDIProvider>(DIContainerScope.Singleton)
                .WithConstructorImplementation(() => new SqlDIProvider(ConnectionStringToUse, DIContainer.Resolve<ILogger>()));

            //clear all the registrations
            DIContainer.ClearAllRegistrations();

            //make sure we have 0 items in the container
            Assert.AreEqual(0, DIContainer.AllRegistrationSelectLazy().Count());
        }

        #endregion

        #region Get All Items In The Container

        /// <summary>
        /// Get all the registered items in the container
        /// </summary>
        [TestCategory("ToracLibrary.DIContainer")]
        [TestCategory("DIContainer")]
        [TestMethod]
        public void AllRegistrationsInContainerTest1()
        {
            //declare my container
            var DIContainer = new ToracDIContainer();

            //store the factory names
            const string FactoryName1 = "FactoryName1";
            const string FactoryName2 = "FactoryName2";

            //register my item now with no overloads
            DIContainer.Register<ILogger, Logger>().WithFactoryName(FactoryName1);

            //register a second instance
            DIContainer.Register<ILogger, Logger>().WithFactoryName(FactoryName2);

            //let's register the data provider (since a string get's passed in, we need to specify how to create this guy)
            DIContainer.Register<SqlDIProvider>(DIContainerScope.Singleton)
                .WithConstructorImplementation(() => new SqlDIProvider(ConnectionStringToUse, DIContainer.Resolve<ILogger>()));

            //now let's check what items we have registered
            var ItemsRegistered = DIContainer.AllRegistrationSelectLazy().ToArray();

            //make sure we have 3 items
            Assert.AreEqual(3, ItemsRegistered.Length);

            //make sure we have factory 1
            Assert.IsTrue(ItemsRegistered.Any(x => x.FactoryName == FactoryName1));

            //make sure the logger is a transient
            Assert.AreEqual(DIContainerScope.Transient, ItemsRegistered.First(x => x.FactoryName == FactoryName1).ObjectScope);

            //make sure the second factory is a transient
            Assert.AreEqual(DIContainerScope.Transient, ItemsRegistered.First(x => x.FactoryName == FactoryName2).ObjectScope);

            //make sure the sql di provider is a singleton
            Assert.AreEqual(DIContainerScope.Singleton, ItemsRegistered.First(x => x.TypeToResolve == typeof(SqlDIProvider)).ObjectScope);

            //make sure we have factory 2
            Assert.IsTrue(ItemsRegistered.Any(x => x.FactoryName == FactoryName2));

            //make sure we have the sql di provider now
            Assert.IsTrue(ItemsRegistered.Any(x => x.ConcreteType == typeof(SqlDIProvider)));
        }

        #endregion

        #endregion

    }

}