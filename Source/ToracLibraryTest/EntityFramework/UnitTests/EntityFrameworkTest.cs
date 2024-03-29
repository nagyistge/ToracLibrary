﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToracLibrary.Core.DataProviders.EntityFrameworkDP;
using ToracLibrary.DIContainer;
using ToracLibraryTest.Framework;
using ToracLibraryTest.UnitsTest.EntityFramework.DataContext;

namespace ToracLibraryTest.UnitsTest.Core.DataProviders.EntityFrameworkDP
{

    /// <summary>
    /// Unit test for entity framework
    /// </summary>
    [TestClass]
    public class EntityFrameworkTest : IDependencyInject
    {

        #region IDependency Injection Methods

        /// <summary>
        /// Configure the DI container for this unit test. Get's called because the class has IDependencyInject - DIUnitTestContainer.ConfigureDIContainer
        /// </summary>
        /// <param name="DIContainer">container to modify</param>
        public void ConfigureDIContainer(ToracDIContainer DIContainer)
        {
            //let's register the di container for the readonly EF Data provider
            DIContainer.Register<EntityFrameworkDP<EntityFrameworkEntityDP>>()
                .WithFactoryName(ReadonlyDataProviderName)
                .WithConstructorImplementation((di) => new EntityFrameworkDP<EntityFrameworkEntityDP>(false, true, false));

            //let's register the di container for the editable EF data provider
            DIContainer.Register<EntityFrameworkDP<EntityFrameworkEntityDP>>()
                .WithFactoryName(WritableDataProviderName)
                .WithConstructorImplementation((di) => new EntityFrameworkDP<EntityFrameworkEntityDP>(true, true, false));
        }

        #endregion

        #region Framework

        /// <summary>
        /// Builds x amount of rows
        /// </summary>
        /// <param name="HowManyRowsToBuild">How many rows to build</param>
        /// <returns>list of ref_test</returns>
        private static IList<Ref_Test> BuildRows(int HowManyRowsToBuild)
        {
            List<Ref_Test> lst = new List<Ref_Test>();

            for (int i = 0; i < HowManyRowsToBuild; i++)
            {
                lst.Add(new Ref_Test { Id = i, Description = i.ToString() });
            }

            return lst;
        }

        #endregion

        #region Constants

        /// <summary>
        /// holds the di container name for the readonly data provider
        /// </summary>
        internal const string ReadonlyDataProviderName = "EFReadOnly";

        /// <summary>
        /// holds the di container name for the insert or update data provider
        /// </summary>
        internal const string WritableDataProviderName = "EFWritableOnly";

        #endregion

        #region Untyped Methods

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void BuildConnectionStringTest1()
        {
            //the value we want to check for
            const string ValueToCheckFor = "metadata=res://*/;provider=System.Data.SqlClient;provider connection string=\"Data Source=ServerName123;Initial Catalog=Db123;Integrated Security=True\"";

            //go run the method and check the results
            Assert.AreEqual(ValueToCheckFor, EFUnTypedDP.BuildConnectionString("ServerName123", "Db123"));
        }

        #endregion

        #region Data Provider Tests

        #region Can Connect

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void CanConnect()
        {
            //make sure we can connect
            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(ReadonlyDataProviderName))
            {
                Assert.IsTrue(DP.CanConnectToDatabase(false).Item1);
            }
        }

        #endregion

        #region Schema

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void SchemaTest1()
        {
            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(ReadonlyDataProviderName))
            {
                Assert.AreEqual("dbo", DP.SchemaOfTableSelect<Ref_Test>());
            }
        }

        #endregion

        #region Auto Detect Test

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void UpdateRecordWithAutoDetectFalseTest1()
        {
            //if you have auto detect false (constructor parameter)
            //then grab a record and update. If you dont have  thisContext.ChangeTracker.DetectChanges(); in the save changes it wont update it but wont raise an error
            DataProviderSetupTearDown.TruncateTable();

            //description to use
            const string DescriptionTest = "New Record";

            //update string to use
            const string DescriptionToUpdateWith = "Update Description";

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //flip the auto detect off
                DP.EFContext.Configuration.AutoDetectChangesEnabled = false;

                //add a new record
                DP.Add(new Ref_Test() { Description = DescriptionTest }, true);

                //go grab this record
                var RecordToUpdate = DP.Find<Ref_Test>(x => x.Description == DescriptionTest, true).First();

                //update the description
                RecordToUpdate.Description = DescriptionToUpdateWith;

                //save the changes now
                DP.SaveChanges();

                //go find the updated record
                var UpdatedRecord = DP.Find<Ref_Test>(x => x.Description == DescriptionToUpdateWith, true).FirstOrDefault();

                //check to make sure we have a record
                Assert.IsNotNull(UpdatedRecord);

                //check the updated description
                Assert.AreEqual(DescriptionToUpdateWith, UpdatedRecord.Description);
            }
        }

        #endregion

        #region Primary Key Is Auto Seed Lookup

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void PrimaryKeyIsAutoSeedLookupTest1()
        {
            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(ReadonlyDataProviderName))
            {
                //go check the Id and make sure it comes back as true
                Assert.IsTrue(DP.ColumnIsAutoSeedLookup(typeof(Ref_Test).Name, nameof(Ref_Test.Id)));

                //test a column that is not an auto seed
                Assert.IsFalse(DP.ColumnIsAutoSeedLookup(typeof(Ref_Test).Name, nameof(Ref_Test.Description)));
            }
        }

        #endregion

        #region Regular Entity Framework Data Provider Tests

        #region Get Or Add

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void GetOrAddTest1()
        {
            //go truncate the table to get ready for the test
            DataProviderSetupTearDown.TruncateTable();

            //go build the record to test
            var RecordToTest = BuildRows(1)[0];

            //store what the description is before we change it
            string OriginalDescriptionValue = RecordToTest.Description;

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //let's test the "add" part when there is no record
                var InsertedRecord = DP.GetOrAdd(RecordToTest, x => x.Id == RecordToTest.Id, false);

                //make sure we have a new row
                Assert.AreEqual(RecordToTest.Id, InsertedRecord.Id);

                //make sure we only have 1 row
                Assert.AreEqual(1, DP.EFContext.Ref_Test.Count());

                //we are going to test the get now...so we change the local record...and then we will test what the database has
                RecordToTest.Description = "New Description";

                //now go run a get or add
                var InsertedRecord2 = DP.GetOrAdd(RecordToTest, x => x.Id == RecordToTest.Id, false);

                //check the description on the 2nd record inserted
                Assert.AreEqual(OriginalDescriptionValue, InsertedRecord2.Description);

                //make sure we have 1 row
                Assert.AreEqual(1, DP.EFContext.Ref_Test.Count());
            }
        }

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public async Task GetOrAddAsyncTest1()
        {
            //go truncate the table to get ready for the test
            DataProviderSetupTearDown.TruncateTable();

            //go build the record to test
            var RecordToTest = BuildRows(1)[0];

            //store what the description is before we change it
            string OriginalDescriptionValue = RecordToTest.Description;

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //let's test the "add" part when there is no record
                var InsertedRecord = await DP.GetOrAddAsync(RecordToTest, x => x.Id == RecordToTest.Id, false);

                //check the inserted record
                Assert.AreEqual(RecordToTest.Id, InsertedRecord.Id);

                //make sure we only have 1 row
                Assert.AreEqual(1, DP.EFContext.Ref_Test.Count());

                //we are going to test the get now...so we change the local record...and then we will test what the database has
                RecordToTest.Description = "New Description";

                //now go run a get or add
                var insertedRecord2 = await DP.GetOrAddAsync(RecordToTest, x => x.Id == RecordToTest.Id, false);

                //check the record
                Assert.AreEqual(OriginalDescriptionValue, insertedRecord2.Description);

                //check how many rows we have
                Assert.AreEqual(1, DP.EFContext.Ref_Test.Count());
            }
        }

        #endregion

        #region Bulk Insert

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void BulkInsertTest1()
        {
            DataProviderSetupTearDown.TruncateTable();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //how many rows to add
                const int HowManyRows = 500;

                //rows to insert
                var RowsToInsert = BuildRows(HowManyRows);

                //go build 500 rows and insert them
                DP.BulkInsert("dbo", RowsToInsert, System.Data.SqlClient.SqlBulkCopyOptions.Default, 100);

                //check that we have 500 rows in the table
                Assert.AreEqual(HowManyRows, DP.Fetch<Ref_Test>(false).Count());

                //first row's id to use to fetch the record
                var FirstRowInsertedDescription = RowsToInsert[0].Description;

                //grab the first record
                var FirstRow = DP.EFContext.Ref_Test.First(x => x.Description == FirstRowInsertedDescription);

                //id is an identity seed, so whatever we put in id will start with 1, thats why description is 1 behind the id
                Assert.AreEqual(1, FirstRow.Id);
                Assert.AreEqual("0", FirstRow.Description);
            }
        }

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public async Task BulkInsertAsyncTest1()
        {
            DataProviderSetupTearDown.TruncateTable();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //how many rows to add
                const int HowManyRows = 500;

                //rows to insert
                var RowsToInsert = BuildRows(HowManyRows);

                //go build 500 rows and insert them
                await DP.BulkInsertAsync("dbo", RowsToInsert, System.Data.SqlClient.SqlBulkCopyOptions.Default, 100);

                //make sure we have the 500 rows to insert
                Assert.AreEqual(HowManyRows, DP.Fetch<Ref_Test>(false).Count());

                //first row's id to use to fetch the record
                var FirstRowInsertedDescription = RowsToInsert[0].Description;

                //grab the first row
                var FirstRow = DP.EFContext.Ref_Test.First(x => x.Description == FirstRowInsertedDescription);

                //id is an identity seed, so whatever we put in id will start with 1, thats why description is 1 behind the id
                Assert.AreEqual(1, FirstRow.Id);
                Assert.AreEqual("0", FirstRow.Description);
            }
        }

        #endregion

        #region ExecuteRawSql (With And Without Parameteres)

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void ExecuteRawSqlWAndWithoutParamTest1()
        {
            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //go truncate the table first (this test's the no parameter)
                DataProviderSetupTearDown.TruncateTable();

                //go insert the raw sql
                DP.ExecuteRawSql("Insert Into Ref_Test (Description) Values ({0});", TransactionalBehavior.DoNotEnsureTransaction, "Ref Test 1");

                //make sure there is 1 record
                Assert.AreEqual(1, DP.Fetch<Ref_Test>(false).Count());
            }
        }

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public async Task ExecuteRawSqlAsyncWAndWithoutParamTest1()
        {
            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //go truncate the table first (this test's the no parameter)
                DataProviderSetupTearDown.TruncateTable();

                //go insert the raw sql
                await DP.ExecuteRawSqlAsync(@"Insert Into Ref_Test (Description) Values ({0});", TransactionalBehavior.DoNotEnsureTransaction, "Ref Test 1");

                //make sure there is 1 record
                Assert.AreEqual(1, DP.Fetch<Ref_Test>(false).Count());
            }
        }

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void ExecuteRawSqlWithResultsNoParametersTest1()
        {
            DataProviderSetupTearDown.TearDownAndBuildUpDbEnvironment();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(ReadonlyDataProviderName))
            {
                //go grab the results from the table
                var Results = DP.ExecuteRawSqlWithResults<Ref_Test>("select * from ref_test;");

                //check that we have the correct number of rows
                Assert.AreEqual(DataProviderSetupTearDown.DefaultRecordsToInsert, Results.Count());
            }
        }

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void ExecuteRawSqlWithResultsWithParametersTest1()
        {
            DataProviderSetupTearDown.TearDownAndBuildUpDbEnvironment();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(ReadonlyDataProviderName))
            {
                //grab a random record's id
                var RandomRecord = DP.Fetch<Ref_Test>(false).OrderBy(x => x.Id).Skip(3).First();

                //go grab the results from the table
                var Result = DP.ExecuteRawSqlWithResults<Ref_Test>("select * from ref_test where Id={0};", RandomRecord.Id).Single();

                //let's verify it's the correct row
                Assert.AreEqual(RandomRecord.Id, Result.Id);
                Assert.AreEqual(RandomRecord.Description, Result.Description);
            }
        }

        #endregion

        #region Delete

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void DeleteWithSqlTest1()
        {
            DataProviderSetupTearDown.TearDownAndBuildUpDbEnvironment();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //grab all the rows
                var DeleteEverythingButThisId = DP.Fetch<Ref_Test>(true).OrderBy(x => x.Id).First().Id;

                //delete everything but this id
                DP.Delete<Ref_Test>(x => x.Id != DeleteEverythingButThisId, true);

                //grab the records so we can test
                var RecordsAfterDelete = DP.Fetch<Ref_Test>(false);

                //should only have 1 record
                Assert.AreEqual(1, RecordsAfterDelete.Count());
            }
        }

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public async Task DeleteWithSqlAsyncTest1()
        {
            DataProviderSetupTearDown.TearDownAndBuildUpDbEnvironment();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //grab all the rows
                var DeleteEverythingButThisId = DP.Fetch<Ref_Test>(true).OrderBy(x => x.Id).Skip(2).First().Id;

                //delete everything but this id
                await DP.DeleteAsync<Ref_Test>(x => x.Id != DeleteEverythingButThisId, true);

                //grab the records so we can test
                var RecordsAfterDelete = DP.Fetch<Ref_Test>(false);

                //should only have 1 record
                Assert.AreEqual(1, RecordsAfterDelete.Count());
            }
        }

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void DeleteMultipleTest1()
        {
            DataProviderSetupTearDown.TearDownAndBuildUpDbEnvironment();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //grab all the rows to delete
                var DeleteAllTheseRows = DP.Fetch<Ref_Test>(true).OrderBy(x => x.Id).Skip(2).Take(2).ToArray();

                //there should be 2 rows to delete
                Assert.AreEqual(2, DeleteAllTheseRows.Length);

                //delete the range and save it
                DP.DeleteRange(DeleteAllTheseRows, true);

                //make sure we have default rows - how many we deleted
                Assert.AreEqual(DataProviderSetupTearDown.DefaultRecordsToInsert - DeleteAllTheseRows.Length, DP.Fetch<Ref_Test>(false).Count());
            }
        }

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public async Task DeleteMultipleAsyncTest1()
        {
            DataProviderSetupTearDown.TearDownAndBuildUpDbEnvironment();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //grab all the rows to delete
                var DeleteAllTheseRows = DP.Fetch<Ref_Test>(true).OrderBy(x => x.Id).Skip(2).Take(2).ToArray();

                //there should be 2 rows to delete
                Assert.AreEqual(2, DeleteAllTheseRows.Length);

                //delete the range and save it
                await DP.DeleteRangeAsync(DeleteAllTheseRows, true);

                //make sure we have default rows - how many we deleted
                Assert.AreEqual(DataProviderSetupTearDown.DefaultRecordsToInsert - DeleteAllTheseRows.Length, DP.Fetch<Ref_Test>(false).Count());
            }
        }

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void DeleteByEntityTest1()
        {
            DataProviderSetupTearDown.TearDownAndBuildUpDbEnvironment();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //grab a random record to delete
                var RecordToDelete = DP.Fetch<Ref_Test>(true).OrderBy(x => x.Id).Skip(2).First();

                //delete that record and save it
                DP.Delete(RecordToDelete, true);

                //make sure we have x amount of records
                Assert.AreEqual(DataProviderSetupTearDown.DefaultRecordsToInsert - 1, DP.Fetch<Ref_Test>(false).Count());
            }
        }

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public async Task DeleteByEntityAsyncTest1()
        {
            DataProviderSetupTearDown.TearDownAndBuildUpDbEnvironment();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //grab a random record to delete
                var RecordToDelete = DP.Fetch<Ref_Test>(true).OrderBy(x => x.Id).Skip(2).First();

                //delete that record and save it
                await DP.DeleteAsync(RecordToDelete, true);

                //make sure we have x amount of records
                Assert.AreEqual(DataProviderSetupTearDown.DefaultRecordsToInsert - 1, DP.Fetch<Ref_Test>(false).Count());
            }
        }

        #endregion

        #region Add

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void AddTest1()
        {
            DataProviderSetupTearDown.TruncateTable();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //build the rows to build
                var RowsToUse = BuildRows(3);

                //3 records to add
                var NewRecord1 = RowsToUse[0];
                var NewRecord2 = RowsToUse[1];
                var NewRecord3 = RowsToUse[2];

                //add record 1 but dont save it
                DP.Add(NewRecord1, false);

                //make sure we have 0 rows in the table
                Assert.AreEqual(0, DP.Fetch<Ref_Test>(false).Count());

                //now save the changes, there should be 1 row after we have
                DP.SaveChanges();

                //make sure we have 1 row saved
                Assert.AreEqual(1, DP.Fetch<Ref_Test>(false).Count());

                //add the next 2 rows, but don't have it now
                DP.Add(NewRecord2, false);
                DP.Add(NewRecord3, false);

                //still should only be 1 row, until we save the changes
                Assert.AreEqual(1, DP.Fetch<Ref_Test>(false).Count());

                //save the changes now
                DP.SaveChanges();

                //make sure we have 3 rows now
                Assert.AreEqual(3, DP.Fetch<Ref_Test>(false).Count());
            }
        }

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void AddTest2()
        {
            DataProviderSetupTearDown.TruncateTable();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //build the rows to build
                var RowsToUse = BuildRows(3);

                //3 records to add
                var NewRecord1 = RowsToUse[0];
                var NewRecord2 = RowsToUse[1];
                var NewRecord3 = RowsToUse[2];

                //add record 1 and save the changes
                DP.Add(NewRecord1, true);

                //make sure we only have this 1 record
                Assert.AreEqual(1, DP.Fetch<Ref_Test>(false).Count());

                //add record 2 and save the changes
                DP.Add(NewRecord2, true);

                //should have 2 rows now
                Assert.AreEqual(2, DP.Fetch<Ref_Test>(false).Count());

                //add record 3 and save the changes
                DP.Add(NewRecord3, true);

                //should have 3 rows now
                Assert.AreEqual(3, DP.Fetch<Ref_Test>(false).Count());
            }
        }

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public async Task AddAsyncTest1()
        {
            DataProviderSetupTearDown.TruncateTable();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //build the rows to build
                var RowsToUse = BuildRows(3);

                //3 records to add
                var NewRecord1 = RowsToUse[0];
                var NewRecord2 = RowsToUse[1];
                var NewRecord3 = RowsToUse[2];

                //add record 1 but dont save it
                DP.Add(NewRecord1, false);

                //make sure we have 0 rows in the table
                Assert.AreEqual(0, DP.Fetch<Ref_Test>(false).Count());

                //now save the changes, there should be 1 row after we have
                await DP.SaveChangesAsync();

                //make sure we have 1 row saved
                Assert.AreEqual(1, DP.Fetch<Ref_Test>(false).Count());

                //add the next 2 rows, but don't have it now
                DP.Add(NewRecord2, false);
                DP.Add(NewRecord3, false);

                //still should only be 1 row, until we save the changes
                Assert.AreEqual(1, DP.Fetch<Ref_Test>(false).Count());

                //save the changes now
                await DP.SaveChangesAsync();

                //make sure we have 3 rows now
                Assert.AreEqual(3, DP.Fetch<Ref_Test>(false).Count());
            }
        }

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void AddRangeTest1()
        {
            DataProviderSetupTearDown.TruncateTable();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //build a list to add to the db
                var RecordsToAdd = BuildRows(3);

                //add the range but don't save it
                DP.AddRange(RecordsToAdd, false);

                //we didn't save yet, so we should have 0 rows
                Assert.AreEqual(0, DP.Fetch<Ref_Test>(false).Count());

                //save the changes now
                DP.SaveChanges();

                //make sure we have x number of rows in the collection
                Assert.AreEqual(RecordsToAdd.Count, DP.Fetch<Ref_Test>(false).Count());
            }
        }

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void AddRangeTest2()
        {
            DataProviderSetupTearDown.TruncateTable();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //records to be added
                var RecordsToBeAdded = BuildRows(3);

                //add the rows to the context to be add
                DP.AddRange(RecordsToBeAdded, true);

                //check how many records we have now
                Assert.AreEqual(RecordsToBeAdded.Count, DP.Fetch<Ref_Test>(false).Count());
            }
        }

        #endregion

        #region Add Or Update

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void UpsertTest1()
        {
            DataProviderSetupTearDown.TruncateTable();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //how many rows to add
                const int HowManyNewRows = 2;

                //build the rows to build
                var RowsToUse = BuildRows(HowManyNewRows);

                //2 records to add
                var NewRecord1 = RowsToUse[0];
                var NewRecord2 = RowsToUse[1];

                //upsert the record but dont save it
                DP.Upsert(NewRecord1, false);

                //check how many records we should have (none because we haven't saved anything yet)
                Assert.AreEqual(0, DP.Fetch<Ref_Test>(false).Count());

                //add record 2 and save it...both records should be saved now
                DP.Upsert(NewRecord2, true);

                //make sure we have the correct number of rows
                Assert.AreEqual(HowManyNewRows, DP.Fetch<Ref_Test>(false).Count());
            }
        }

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void UpsertRangeTest1()
        {
            DataProviderSetupTearDown.TruncateTable();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //create 2 random rows to upsert
                var NewRecord1 = new Ref_Test() { Id = -1, Description = "1" };
                var NewRecord2 = new Ref_Test() { Id = -1, Description = "2" };

                //rows to upsert
                var RowsToUpsert = new Ref_Test[] { NewRecord1, NewRecord2 };

                //go upsert the rows and save it
                DP.UpsertRange(RowsToUpsert, true);

                //how many rows + the rows in the collection
                Assert.AreEqual(RowsToUpsert.Length, DP.Fetch<Ref_Test>(false).Count());
            }
        }

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void UpsertRangeTest2()
        {
            DataProviderSetupTearDown.TearDownAndBuildUpDbEnvironment();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //string value to change
                const string ChangeStringValue = "UpdatedRecordDescription";

                //grab 2 records to change
                var NewRecord1 = DP.EFContext.Ref_Test.First();
                var NewRecord2 = DP.EFContext.Ref_Test.OrderBy(x => x.Id).Skip(1).First();

                //set the description on the first record
                NewRecord1.Description = ChangeStringValue;

                //create a list to upsert
                var RecordsToUpsert = new Ref_Test[] { NewRecord1, NewRecord2 };

                //upsert the records and save
                DP.UpsertRange(RecordsToUpsert, true);

                //make sure we have the change string on the record we wanted to change
                Assert.AreEqual(ChangeStringValue, DP.EFContext.Ref_Test.Single(x => x.Id == NewRecord1.Id).Description);

                //make sure we only have this record with this description
                Assert.AreEqual(1, DP.EFContext.Ref_Test.Count(x => x.Description == ChangeStringValue));
            }
        }

        #endregion

        #region Find

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public async Task FindTest1()
        {
            DataProviderSetupTearDown.TearDownAndBuildUpDbEnvironment();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(ReadonlyDataProviderName))
            {
                //grab the last id
                var LastRecordInTable = DP.EFContext.Ref_Test.OrderByDescending(x => x.Id).First();

                //find the records where the id is greater then the last record
                var RecordsToFind = await DP.Find<Ref_Test>(x => x.Id >= LastRecordInTable.Id, false).ToArrayAsync();

                //make sure we only have 1 record
                Assert.AreEqual(1, RecordsToFind.Length);
                Assert.AreEqual(LastRecordInTable.Id, RecordsToFind[0].Id);
                Assert.AreEqual(LastRecordInTable.Description, RecordsToFind[0].Description);
            }
        }

        #endregion

        #region Fetch

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public async Task FetchTest1()
        {
            DataProviderSetupTearDown.TearDownAndBuildUpDbEnvironment();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(ReadonlyDataProviderName))
            {
                //grab the last id
                var LastRecordInTable = DP.EFContext.Ref_Test.OrderByDescending(x => x.Id).First();

                //find the records where the id is greater then the last record
                var RecordsToFind = await DP.Fetch<Ref_Test>(false).Where(x => x.Id >= LastRecordInTable.Id).ToArrayAsync();

                //make sure we only have 1 record
                Assert.AreEqual(1, RecordsToFind.Length);
                Assert.AreEqual(LastRecordInTable.Id, RecordsToFind[0].Id);
                Assert.AreEqual(LastRecordInTable.Description, RecordsToFind[0].Description);
            }
        }

        #endregion

        #region Transactions

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void TransactionTest1()
        {
            DataProviderSetupTearDown.TruncateTable();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //Record to add
                var RecordToAdd = new Ref_Test() { Description = "New Record" };

                //start the transaction
                DP.StartTransaction();

                //add the record, don't save yet
                DP.Add(RecordToAdd, false);

                //make sure we still have 0 records
                Assert.AreEqual(0, DP.Fetch<Ref_Test>(false).Count());

                //save the changes now
                DP.SaveChanges();

                //should have 1 record
                Assert.AreEqual(1, DP.Fetch<Ref_Test>(false).Count());

                //roll back because we have commited
                DP.RollBackTransaction();

                //should have 0 rows
                Assert.AreEqual(0, DP.Fetch<Ref_Test>(false).Count());
            }
        }

        [TestCategory("Core.DataProviders.EntityFramework")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public void TransactionTest2()
        {
            DataProviderSetupTearDown.TruncateTable();

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //Record to add
                var RecordToAdd = new Ref_Test() { Description = "New Record" };

                //start the transaction
                DP.StartTransaction();

                //add the record, don't save yet
                DP.Add(RecordToAdd, false);

                //make sure we still have 0 records
                Assert.AreEqual(0, DP.Fetch<Ref_Test>(false).Count());

                //save the changes now
                DP.SaveChanges();

                //should have 1 record
                Assert.AreEqual(1, DP.Fetch<Ref_Test>(false).Count());

                //commit the transaction now
                DP.CommitTransaction();

                //shold have the 1 commited record in the table now
                Assert.AreEqual(1, DP.Fetch<Ref_Test>(false).Count());
            }
        }

        #endregion

        #endregion

        #endregion

        #region Data Inheritance Examples

        [TestCategory("Core.DataProviders.EntityFramework.Examples")]
        [TestCategory("EntityFramework")]
        [TestCategory("Core")]
        [TestMethod]
        public async Task DataInheritanceExample1()
        {
            //example of Table per Type(TPT) inheritance (which i think is the most useful out of all of them)

            //these are great examples of the different types
            //http://weblogs.asp.net/manavi/inheritance-mapping-strategies-with-entity-framework-code-first-ctp5-part-1-table-per-hierarchy-tph
            //http://weblogs.asp.net/manavi/inheritance-mapping-strategies-with-entity-framework-code-first-ctp5-part-2-table-per-type-tpt
            //http://weblogs.asp.net/manavi/inheritance-mapping-strategies-with-entity-framework-code-first-ctp5-part-3-table-per-concrete-type-tpc-and-choosing-strategy-guidelines

            using (var DP = DIUnitTestContainer.DIContainer.Resolve<EntityFrameworkDP<EntityFrameworkEntityDP>>(WritableDataProviderName))
            {
                //delete the records
                DP.Delete<Animal>(x => true, false);
                DP.Delete<Cat>(x => true, false);
                DP.Delete<Dog>(x => true, false);

                //go save the deletes
                await DP.SaveChangesAsync();

                //size of dog
                const string DogSize = "Size Of Dog";
                const string CatSize = "Size Of Dog";

                //bark
                const int Bark = 25;

                //meow
                const int Meow = 30;

                //declare an action to test the size
                Action<Dog> DogSizeTester = x => Assert.AreEqual(DogSize, x.Size);
                Action<Cat> CatSizeTester = x => Assert.AreEqual(CatSize, x.Size);

                //declare an action for the bark and meow
                Action<Dog> DogBarkTester = x => Assert.AreEqual(Bark, x.Bark);
                Action<Cat> CatMeowTester = x => Assert.AreEqual(Meow, x.Meow);

                //go insert a dog and a cat
                DP.Add(new Dog { Size = DogSize, Bark = Bark }, false);
                DP.Add(new Cat { Size = CatSize, Meow = Meow }, false);

                //save it now
                await DP.SaveChangesAsync();

                //grab all the dogs
                var DogsInTable = DP.Fetch<Animal>(false).OfType<Dog>().ToArray();

                //grab all the cats
                var CatsInTable = DP.Fetch<Animal>(false).OfType<Cat>().ToArray();

                //grab all the animals
                var AnimalsInTable = DP.Fetch<Animal>(false).ToArray();

                //run the tests now
                Assert.AreEqual(2, AnimalsInTable.Length);
                Assert.AreEqual(1, CatsInTable.Length);
                Assert.AreEqual(1, DogsInTable.Length);

                //go check the size for each cat, dog
                DogSizeTester.Invoke(DogsInTable[0]);
                CatSizeTester.Invoke(CatsInTable[0]);

                //check bark now
                DogBarkTester.Invoke(DogsInTable[0]);

                //check meow
                CatMeowTester.Invoke(CatsInTable[0]);

                //let's check the individual records in the animal collection (grab each record)
                var DogInAnimlCollection = AnimalsInTable.OfType<Dog>().ToArray();

                //grab the cat
                var CatInAnimalCollection = AnimalsInTable.OfType<Cat>().ToArray();

                //make sure we have 1 of each in animals
                Assert.AreEqual(1, DogInAnimlCollection.Length);
                Assert.AreEqual(1, CatInAnimalCollection.Length);

                //check the size in the animal collection
                //go check the size for each cat, dog
                DogSizeTester.Invoke(DogInAnimlCollection[0]);
                CatSizeTester.Invoke(CatInAnimalCollection[0]);

                //check bark now
                DogBarkTester.Invoke(DogInAnimlCollection[0]);

                //check meow
                CatMeowTester.Invoke(CatInAnimalCollection[0]);
            }
        }

        #endregion

    }

}
