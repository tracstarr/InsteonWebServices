using System;
using System.Collections.Generic;
using ServiceStack.OrmLite;

namespace Insteon.Data
{
    public class InsteonDataManager
    {
        private readonly OrmLiteConnectionFactory dbFactory;

        public InsteonDataManager(bool inMemory)
        {
            if (inMemory)
            {
                //Use in-memory Sqlite DB
                dbFactory = new OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider);
            }
            else
            {
                var dir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                dbFactory = new OrmLiteConnectionFactory("Data Source =" + dir + @"\Insteon\InsteonDevices.s3db; Version=3;", SqliteDialect.Provider);
            }

            InitDb();
        }

        private void InitDb()
        {
            using (var db = dbFactory.OpenDbConnection())
            {
                if (!db.TableExists<InsteonDeviceModel>())
                {
                    db.CreateTable<InsteonDeviceModel>();
                }
            }
        }

        public IList<InsteonDeviceModel> GetAllDevices()
        {
            using (var db = dbFactory.OpenDbConnection())
            {
                return db.Select<InsteonDeviceModel>();
            }
        }

        public long Add(InsteonDeviceModel device)
        {
            using (var db = dbFactory.OpenDbConnection())
            {
                device.CreatedDate = DateTime.UtcNow;
                var id = db.Insert(device, selectIdentity: true);
                return id;
            }
        }

        public void Update(InsteonDeviceModel device)
        {
            using (var db = dbFactory.OpenDbConnection())
            {
                device.ModifiedDate = DateTime.UtcNow;
                db.Update(device);
            }
        }

        public InsteonDeviceModel GetByAddress(string address)
        {
            using (var db = dbFactory.OpenDbConnection())
            {
                return db.Single<InsteonDeviceModel>(new { Address = address });
            }
        }
    }
}