using ThaiReference.Models;
using ThaiReference.Models.BaseModels;
using ThaiReference.Models.Interfaces;
using LiteDB;

namespace ThaiReference.DataAccess
{
    public class LiteDataService(string snapshotDbDir, Func<string, Task>? ensureSnapshotDb = null) : IDataService
    {
        private readonly string _snapshotDbDir = snapshotDbDir;
        private readonly Func<string, Task>? _ensureSnapshotDb = ensureSnapshotDb;
        private string? _dbName;
        private LiteDBManager? _liteDBManager;
        private bool _bsonMappingSet = false;

        public void SetBSONMapping()
        {
            var mapper = BsonMapper.Global;

            mapper.RegisterType<ITranscription>(
                serialize: obj => mapper.Serialize(typeof(Transcription), obj),
                deserialize: bson => mapper.Deserialize<Transcription>(bson)
            );

            mapper.RegisterType<ISound>(
                serialize: obj => mapper.Serialize(typeof(Sound), obj),
                deserialize: bson => mapper.Deserialize<Sound>(bson)
            );

            mapper.RegisterType<IWord>(
                serialize: obj => mapper.Serialize(typeof(Word), obj),
                deserialize: bson => mapper.Deserialize<Word>(bson)
            );

            mapper.RegisterType<IGridPositionCell>(
                serialize: obj => mapper.Serialize(typeof(GridPositionCell), obj),
                deserialize: bson => mapper.Deserialize<GridPositionCell>(bson)
            );

            mapper.RegisterType<ISentence>(
                serialize: obj => mapper.Serialize(typeof(Sentence), obj),
                deserialize: bson => mapper.Deserialize<Sentence>(bson)
            );

            mapper.RegisterType<IPosition>(
                serialize: obj => mapper.Serialize(typeof(Position), obj),
                deserialize: bson => mapper.Deserialize<Position>(bson)
            );

            mapper.RegisterType<IWeekday>(
                serialize: obj => mapper.Serialize(typeof(Weekday), obj),
                deserialize: bson => mapper.Deserialize<Weekday>(bson)
            );

            mapper.RegisterType<ICelestialBody>(
                serialize: obj => mapper.Serialize(typeof(CelestialBody), obj),
                deserialize: bson => mapper.Deserialize<CelestialBody>(bson)
            );

            mapper.RegisterType<ITimeOfDay>(
                serialize: obj => mapper.Serialize(typeof(TimeOfDay), obj),
                deserialize: bson => mapper.Deserialize<TimeOfDay>(bson)
            );

            mapper.RegisterType<IHour>(
                serialize: obj => mapper.Serialize(typeof(Hour), obj),
                deserialize: bson => mapper.Deserialize<Hour>(bson)
            );

            mapper.RegisterType<IZodiac>(
                serialize: obj => mapper.Serialize(typeof(Zodiac), obj),
                deserialize: bson => mapper.Deserialize<Zodiac>(bson)
            );

            mapper.RegisterType<IMonth>(
                serialize: obj => mapper.Serialize(typeof(Month), obj),
                deserialize: bson => mapper.Deserialize<Month>(bson)
            );

            mapper.RegisterType<IVowelGrapheme>(
                serialize: obj => mapper.Serialize(typeof(VowelGrapheme), obj),
                deserialize: bson => mapper.Deserialize<VowelGrapheme>(bson)
            );

            mapper.RegisterType<ICluster>(
                serialize: obj => mapper.Serialize(typeof(Cluster), obj),
                deserialize: bson => mapper.Deserialize<Cluster>(bson)
            );

            mapper.RegisterType<IHomophoneGroup>(
                serialize: obj => mapper.Serialize(typeof(HomophoneGroup), obj),
                deserialize: bson => mapper.Deserialize<HomophoneGroup>(bson)
            );

            mapper.RegisterType<IMinimalSet>(
                serialize: obj => mapper.Serialize(typeof(MinimalSet), obj),
                deserialize: bson => mapper.Deserialize<MinimalSet>(bson)
            );

            mapper.RegisterType<IConsonantClass>(
                serialize: obj => mapper.Serialize(typeof(ConsonantClass), obj),
                deserialize: bson => mapper.Deserialize<ConsonantClass>(bson)
            );

            mapper.RegisterType<IDirection>(
                serialize: obj => mapper.Serialize(typeof(Direction), obj),
                deserialize: bson => mapper.Deserialize<Direction>(bson)
            );

            mapper.RegisterType<IColor>(
                serialize: obj => mapper.Serialize(typeof(ThaiColor), obj),
                deserialize: bson => mapper.Deserialize<ThaiColor>(bson)
            );

            mapper.RegisterType<IHueFamily>(
                serialize: obj => mapper.Serialize(typeof(HueFamily), obj),
                deserialize: bson => mapper.Deserialize<HueFamily>(bson)
            );
        }

        public string? DbName
        {
            get { return $"{_dbName}_lite.db"; }
            set { _dbName = value; }
        }

        public bool IsInitialized => File.Exists(DbPath);

        public string DbPath
        {
            get
            {
                if (DbName == null)
                {
                    throw new InvalidOperationException("Database name is not set. Please initialize the property before use.");
                }
                return Path.Combine(_snapshotDbDir, DbName);
            }
        }

        public LiteDBManager DBManager
        {
            get
            {
                if (_liteDBManager == null || _liteDBManager.DbPath != DbPath)
                {
                    _liteDBManager = new LiteDBManager(DbPath);
                }
                return _liteDBManager;
            }
        }

        private async Task Init()
        {
            if (_ensureSnapshotDb is not null)
                await _ensureSnapshotDb(DbName!).ConfigureAwait(false);
        }

        public Task InitializeAsync() => Init();

        /// <summary>Drop any open LiteDB handle so <see cref="Init"/> can replace a stale AppData file.</summary>
        void ReleaseConnection()
        {
            if (_liteDBManager is null)
                return;
            try { _liteDBManager.Connection.Dispose(); }
            catch { /* already disposed */ }
            _liteDBManager = null;
        }

        public IEnumerable<T> GetData<T>(string collName)
        {
            try
            {
                if (!_bsonMappingSet)
                {
                    SetBSONMapping();
                    _bsonMappingSet = true;
                }

                // Never open LiteDB before ensure: opening creates an empty file that then
                // blocks re-copy forever (File.Exists == true, collections empty).
                if (File.Exists(DbPath) && !OfflineSnapshot.HasRequiredCollections(DbPath))
                    ReleaseConnection();

                Init().ConfigureAwait(false).GetAwaiter().GetResult();

                if (!OfflineSnapshot.HasRequiredCollections(DbPath))
                {
                    throw new InvalidOperationException(
                        $"Offline snapshot '{DbPath}' is missing or incomplete after ensure.");
                }

                return DBManager.Connection.GetCollection<T>(collName).FindAll();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lite request for collection \n{collName}\nfailed with error:\n{ex.Message}", ex);
            }
        }

        public IEnumerable<ISound> Sounds => GetData<ISound>("sounds");
        public IEnumerable<IConsonant> Consonants => GetData<IConsonant>("consonants");
        public IEnumerable<IVowel> Vowels => GetData<IVowel>("vowels");
        public IEnumerable<IMark> Marks => GetData<IMark>("marks");

        public IEnumerable<IConsonantClass> Classes
        {
            get
            {
                try
                {
                    return GetData<IConsonantClass>("classes");
                }
                catch
                {
                    return [];
                }
            }
        }

        public IEnumerable<INumeral> Numerals => GetData<INumeral>("numerals");
        public IEnumerable<IPronoun> Pronouns => GetData<IPronoun>("pronouns");
        public IEnumerable<IGridPositionCell> GridPositionCells
        {
            get
            {
                try { return GetData<IGridPositionCell>("gridpositioncells"); }
                catch { return []; }
            }
        }

        public IEnumerable<IPosition> Positions
        {
            get
            {
                try { return GetData<IPosition>("positions"); }
                catch { return []; }
            }
        }
        public IEnumerable<IWeekday> Weekdays => GetData<IWeekday>("weekdays");

        public IEnumerable<ICelestialBody> CelestialBodies
        {
            get
            {
                try
                {
                    return GetData<ICelestialBody>("celestialbodies");
                }
                catch
                {
                    return [];
                }
            }
        }

        public IEnumerable<IMonth> Months => GetData<IMonth>("months");
        public IEnumerable<ITimeOfDay> TimeOfDayItems => GetData<ITimeOfDay>("timeofdayitems");
        public IEnumerable<IHour> HourItems => GetData<IHour>("houritems");
        public IEnumerable<ICluster> Clusters => GetData<ICluster>("clusters");

        public IEnumerable<IHomophoneGroup> Homophones
        {
            get
            {
                try
                {
                    return GetData<IHomophoneGroup>("homophones");
                }
                catch
                {
                    return [];
                }
            }
        }

        public IEnumerable<IMinimalSet> MinimalSets
        {
            get
            {
                try
                {
                    return GetData<IMinimalSet>("minimalsets");
                }
                catch
                {
                    return [];
                }
            }
        }

        public IEnumerable<IDirection> Directions
        {
            get
            {
                try
                {
                    return GetData<IDirection>("directions");
                }
                catch
                {
                    return [];
                }
            }
        }

        public IEnumerable<IHueFamily> HueFamilies
        {
            get
            {
                try
                {
                    return GetData<IHueFamily>("huefamilies");
                }
                catch
                {
                    return [];
                }
            }
        }
    }
}
