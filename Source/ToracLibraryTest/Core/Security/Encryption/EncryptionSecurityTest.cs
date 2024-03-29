﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToracLibrary.Core.Security.Encryption;
using ToracLibraryTest.Framework;
using ToracLibrary.DIContainer;
using ToracLibrary.DIContainer.Parameters.ConstructorParameters;

namespace ToracLibraryTest.UnitsTest.Core
{

    /// <summary>
    /// Unit test to test the Encryption
    /// </summary>
    [TestClass]
    public class EncryptionSecurityTest : IDependencyInject
    {

        #region IDependency Injection Methods

        /// <summary>
        /// Configure the DI container for this unit test. Get's called because the class has IDependencyInject - DIUnitTestContainer.ConfigureDIContainer
        /// </summary>
        /// <param name="DIContainer">container to modify</param>
        public void ConfigureDIContainer(ToracDIContainer DIContainer)
        {
            //let's register the di container now (md5)
            DIContainer.Register<ISecurityEncryption, MD5HashSecurityEncryption>(ToracDIContainer.DIContainerScope.Singleton)
                .WithFactoryName(MD5DIContainerName)
                .WithConstructorParameters(new PrimitiveCtorParameter("Test"));

            //let's register the rijndael container now
            DIContainer.Register<ISecurityEncryption, RijndaelSecurityEncryption>(ToracDIContainer.DIContainerScope.Singleton)
                .WithFactoryName(RijndaelDIContainerName)
                .WithConstructorParameters(new PrimitiveCtorParameter("1234567891123456"), new PrimitiveCtorParameter("1234567891123456"));

            //let's register the 1 way data binding
            DIContainer.Register<IOneWaySecurityEncryption, SHA256SecurityEncryption>(ToracDIContainer.DIContainerScope.Singleton)
                .WithFactoryName(SHA256ContainerName);
        }

        #endregion

        #region Constants

        /// <summary>
        /// Holds the md5 Di container name
        /// </summary>
        private const string MD5DIContainerName = "MD5";

        /// <summary>
        /// Rijndael container name for the di container
        /// </summary>
        private const string RijndaelDIContainerName = "Rijndael";

        /// <summary>
        /// Sha 256 container name
        /// </summary>
        private const string SHA256ContainerName = "SHA256";

        /// <summary>
        /// Value to test
        /// </summary>
        private const string ValueToTest = "test123";

        #endregion

        #region Unit Tests

        /// <summary>
        /// Test the MD5 Hash Encryption
        /// </summary>
        [TestCategory("Core.Security.Encryption")]
        [TestCategory("Core.Security")]
        [TestCategory("Core")]
        [TestMethod]
        public void EncryptionMD5HashTest1()
        {
            //create the implementation of the interface
            var EncryptImplementation = DIUnitTestContainer.DIContainer.Resolve<ISecurityEncryption>(MD5DIContainerName);

            //go encrypt the value
            var EncryptedValue = EncryptImplementation.Encrypt(ValueToTest);

            //is it what we are expecting
            Assert.AreEqual("6Ktjr0b7Wj0=", EncryptedValue);

            //go decrypt it
            var DecryptedValue = EncryptImplementation.Decrypt(EncryptedValue);

            //check the decrypted value
            Assert.AreEqual(ValueToTest, DecryptedValue);
        }

        /// <summary>
        /// Test the Rijndael Encrytion
        /// </summary>
        [TestCategory("Core.Security.Encryption")]
        [TestCategory("Core.Security")]
        [TestCategory("Core")]
        [TestMethod]
        public void EncryptionRijndaelSecurityTest1()
        {
            //create the implementation of the interface
            var EncryptImplementation = DIUnitTestContainer.DIContainer.Resolve<ISecurityEncryption>(RijndaelDIContainerName);

            //go encrypt the value
            var EncryptedValue = EncryptImplementation.Encrypt(ValueToTest);

            //is it what we are expecting
            Assert.AreEqual("bo1JgQZZcRDRqmjNK47h2Q==", EncryptedValue);

            //go decrypt it
            var DecryptedValue = EncryptImplementation.Decrypt(EncryptedValue);

            //check the decrypted value
            Assert.AreEqual(ValueToTest, DecryptedValue);
        }

        /// <summary>
        /// Test the SHA256 Encrytion
        /// </summary>
        [TestCategory("Core.Security.Encryption")]
        [TestCategory("Core.Security")]
        [TestCategory("Core")]
        [TestMethod]
        public void EncryptionSHA256SecurityTest1()
        {
            //create the implementation of the interface
            var EncryptImplementation = DIUnitTestContainer.DIContainer.Resolve<IOneWaySecurityEncryption>(SHA256ContainerName);

            //go encrypt the value
            var EncryptedValue = EncryptImplementation.Encrypt(ValueToTest);

            //is it what we are expecting
            Assert.AreEqual("ECD71870D1963316A97E3AC3408C9835AD8CF0F3C1BC703527C30265534F75AE", EncryptedValue);
        }

        #endregion

    }

}