// <copyright file="BaseTest.cs" company="ARITEC">
// Copyright (c) ARITEC. All rights reserved.
// </copyright>

namespace Integra.Space.UnitTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Base class for tests.
    /// </summary>
    public abstract class BaseTest
    {
        /// <summary>
        /// Test context.
        /// </summary>
        private TestContext testContextInstance;

        /// <summary>
        /// Gets the connection string name.
        /// </summary>
        public string ConnectionStringName { get; private set; }

        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext
        {
            get
            {
                return this.testContextInstance;
            }

            set
            {
                this.testContextInstance = value;
            }
        }

        /// <summary>
        /// Contains code to be used before all test in the assembly.
        /// </summary>
        [ClassInitialize]
        public void InitializeAssembly()
        {
            this.ConnectionStringName = this.TestContext.Properties["ConnectionStringName"].ToString();
        }
    }
}
