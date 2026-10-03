using LiteDB;

namespace ThaiReference.DataAccess
{
    public partial class LiteDB(string dbPath) : LiteDatabase(dbPath)
    {
        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }

    public class LiteDBManager(string dbPath)
    {
        public string DbPath { get; } = dbPath;

        private LiteDB? _connection = null;

        public LiteDB Connection
        {
            get
            {
                if (_connection == null || _connection.IsDisposed)
                {
                    _connection = new LiteDB(DbPath);
                }
                return _connection;
            }
        }

        public ILiteCollection<T> GetCollection<T>(string collectionName) where T : class
        {
            return Connection.GetCollection<T>(collectionName);
        }

        public T InsertAndReturn<T>(ILiteCollection<T> coll, T record) where T : class
        {
            return coll.FindById(coll.Insert(record));
        }

        public T InsertAndReturn<T>(string collectionName, T record) where T : class
        {
            var coll = Connection.GetCollection<T>(collectionName);
            return coll.FindById(coll.Insert(record));
        }
    }
}
