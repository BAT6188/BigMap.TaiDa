using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jovian.BigMap.classes
{
    using System.IO;
    using System.Threading.Tasks;
    using System.Runtime.Serialization.Json;
    using System.Windows;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows.Media.Imaging;
    using System.Data.SQLite;
    using System.Net;
    using System.Threading;
    using System.Collections.Concurrent;
    using System.Net.NetworkInformation;
    using System.Drawing;

    using ESRI.ArcGIS.Client;
    using ESRI.ArcGIS.Client.Geometry;    
    /// <summary>
    /// 2015-9-1 添加 离线地图帮助类
    /// 实现在线地图缓存
    /// 加载地图时，先加载离线地图，离线地图不存在，再加载在线地图
    /// </summary>
    class TiledMapHelper:TiledMapServiceLayer
    {
        struct struOneTiledData
        {
            //public SQLiteCommand cmd;
            //public string Key;
            public byte[] tile;
            public string TableName;
            public int level;
            public int row;
            public int column;
        }
        private void SaveQueue()
        {
            LogHelper.WriteLog("离线地图路径：" + DBName);
            CheckPath(PublicParams.strDBPath);
            SQLiteConnection db = new SQLiteConnection("Data Source=" + DBName + ";PRAGMA synchronous = OFF;PRAGMA page_size = 8192;PRAGMA cache_size = 4096");
            db.Open();

            SQLiteCommand cmd = db.CreateCommand();
            object odata;
            struOneTiledData oneTiledDataTemp = new struOneTiledData() ;
            while (true)
            {
                if (Buffer.Count > 0)
                {
                    SQLiteTransaction Transaction = db.BeginTransaction();
                    while (Buffer.Count > 0)
                    {
                        Buffer.TryDequeue(out odata);
                        oneTiledDataTemp = (struOneTiledData)odata;
                        cmd.CommandText = "INSERT OR REPLACE INTO '" + oneTiledDataTemp.TableName + "' (level, row, column,tile) VALUES (@Level,@Row,@Column,@Bytes)"; //,level,row,column);
                        cmd.Parameters.Add(new SQLiteParameter("@Level", oneTiledDataTemp.level));
                        cmd.Parameters.Add(new SQLiteParameter("@Row", oneTiledDataTemp.row));
                        cmd.Parameters.Add(new SQLiteParameter("@Column", oneTiledDataTemp.column));
                        cmd.Parameters.Add(new SQLiteParameter("@Bytes", oneTiledDataTemp.tile));
                        cmd.ExecuteNonQuery();
                    }
                    Transaction.Commit(); 
                }
                else
                    Thread.Sleep(100);
            }
        }
        static ConcurrentQueue<object> Buffer = new ConcurrentQueue<object>();
        public TiledMapHelper()
        {
            Thread LoopSave = new Thread(SaveQueue);
            LoopSave.IsBackground = true;
            LoopSave.Start();
        }

        #region Property & Fields
        public static readonly DependencyProperty EnableOfflineProperty = DependencyProperty.Register("EnableOffline",typeof(bool), typeof(TiledMapHelper), new PropertyMetadata(true));
        /// <summary>
        /// Whether to allow this layer works in offline mode. If false, it is an original ArcGISTiledMapServiceLayer(DeleteOfflineCache property will also take no effect).
        /// Default is true.
        /// </summary>
        public bool EnableOffline
        {
            get { return (bool)GetValue(EnableOfflineProperty); }
            set { SetValue(EnableOfflineProperty, value); }
        }
        public static readonly DependencyProperty SaveOfflineTilesProperty =
            DependencyProperty.Register("SaveOfflineTiles", typeof(bool), typeof(TiledMapHelper), new PropertyMetadata(true));
        /// <summary>
        /// Whether to switch on the auto saving downloaded tiles to local sqlite db ability.
        /// Default is true.
        /// Only take effect when LoadOfflineTileFirst==false
        /// </summary>
        public bool SaveOfflineTiles
        {
            get { return (bool)GetValue(SaveOfflineTilesProperty); }
            set { SetValue(SaveOfflineTilesProperty, value); }
        }

        public static readonly DependencyProperty LoadOfflineTileFirstProperty =
            DependencyProperty.Register("LoadOfflineTileFirst", typeof(bool), typeof(TiledMapHelper), new PropertyMetadata(false));
        /// <summary>
        /// If true, this layer will load offline db tile first.
        /// If false, this layer will load online map service tile first.
        /// Default is false.
        /// </summary>
        public bool LoadOfflineTileFirst
        {
            get { return (bool)GetValue(LoadOfflineTileFirstProperty); }
            set { SetValue(LoadOfflineTileFirstProperty, value); }
        }

        public static readonly DependencyProperty DeleteSavedOfflineTilesProperty =
            DependencyProperty.Register("DeleteSavedOfflineTiles", typeof(bool), typeof(TiledMapHelper), new PropertyMetadata(false));
        /// <summary>
        /// Whether to delete all offline cache(if has) before initialize. Set this to True only if you want to redownload all cache tiles.
        /// Default is false.
        /// </summary>
        public bool DeleteSavedOfflineTiles
        {
            get { return (bool)GetValue(DeleteSavedOfflineTilesProperty); }
            set { SetValue(DeleteSavedOfflineTilesProperty, value); }
        }

        public enum Mode
        {
            /// <summary>
            /// only save tiles which not exist.
            /// </summary>
            SaveOnly,
            /// <summary>
            /// save tiles which not exist and update tiles which already exist.
            /// </summary>
            SaveOrUpdate
        }
        [TypeConverter(typeof(Mode))]
        public static readonly DependencyProperty SaveTilesModeProperty = DependencyProperty.Register("SaveTilesMode", typeof(Mode), typeof(TiledMapHelper), new PropertyMetadata(Mode.SaveOnly));
        /// <summary>
        /// Save offline tiles mode.
        /// Only take effect when LoadOfflineTileFirst==false && SaveOfflineTiles==true
        /// </summary>
        public Mode SaveTilesMode
        {
            get { return (Mode)GetValue(SaveTilesModeProperty); }
            set { SetValue(SaveTilesModeProperty, value); }
        }

        //private const string DBName = "OfflineTiles.db";
        private static string DBName = PublicParams.strDBPath + @"OfflineTiles.db";//System.Configuration.ConfigurationManager.AppSettings["DBPath"].Trim();
        //private const string TableServicesName = "MapServices";
        private bool _isConnected;
        private SQLiteConnection _conn;
        private List<string> _tilesNeedToSave = new List<string>();
        private List<string> _tilesAlreadySaved = new List<string>();

        #endregion

        private string _url = "";
        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
            }
        }

        public override void Initialize()
        {
            //PGIS 
            this.FullExtent = new ESRI.ArcGIS.Client.Geometry.Envelope(111.925137105232, 9.44664648125718, 141.597475975149, 46.5533847659212)//    this.FullExtent = new ESRI.ArcGIS.Client.Geometry.Envelope(-180, -90, 180, 90)            
            {
                SpatialReference = new ESRI.ArcGIS.Client.Geometry.SpatialReference(4326)
            };
            this.SpatialReference = new ESRI.ArcGIS.Client.Geometry.SpatialReference(4326);
            this.TileInfo = new TileInfo()
            {
                Height = 256,
                Width = 256,

                //PGIS
                Origin = new ESRI.ArcGIS.Client.Geometry.MapPoint(0, 90)                                                                      //Origin = new ESRI.ArcGIS.Client.Geometry.MapPoint(-180, 90)                
                {
                    SpatialReference = new ESRI.ArcGIS.Client.Geometry.SpatialReference(4326)
                },
                Lods = new Lod[22]
            };


            //PGIS
            double resolution = 2;                                                                                                            //double resolution = 0.703125 * 2;            

            for (int i = 0; i < TileInfo.Lods.Length; i++)
            {

                TileInfo.Lods[i] = new Lod() { Resolution = resolution };
                resolution = resolution / 2;
            }

            base.Initialize();
            base.TileLoaded += new EventHandler<TileLoadEventArgs>(OfflineableTileLayer_TileLoaded);
            InitDB();
            InitTileTable();
        }

        public override string GetTileUrl(int level, int row, int col)
        {
            // PGIS
            double bound = Math.Pow(2, level);
            double row2 = (bound * 0.17578125 - row - 1);
            string url = _url + "Zoom=" + level + "&Row=" + row2 + "&Col=" + col + "";
            // 天地图
            //string url = _url + "&x=" + col.ToString() + "&y=" + row.ToString() + "&l=" + level.ToString();
            //System.Net.HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url);
            return url;
        }

        protected override void GetTileSource(int level, int row, int col, Action<System.Windows.Media.ImageSource> onComplete)
        {
            //base.GetTileSource(level, row, col, onComplete);
            //return;

            string key = string.Format("{0}/{1}/{2}", level, row, col);
            byte[] tilebytes = null;
            _isConnected = true;
            // _isConnected = NetworkInterface.GetIsNetworkAvailable();  // 如果每次判断，会严重影响性能
            if (_isConnected)
            {
                if (LoadOfflineTileFirst && ((tilebytes = LoadTile(level, row, col)) != null))
                {
                    if (tilebytes != null)
                    {
                        BitmapImage image = new BitmapImage()
                        {
                            CreateOptions = BitmapCreateOptions.DelayCreation
                        };
                        image.BeginInit();
                        image.StreamSource = new MemoryStream(tilebytes);
                        image.EndInit();
                        onComplete(image);
                        //mark this tile need to download, so when tile_loaded, avoid to check if tile exist again.
                        if (!_tilesAlreadySaved.Contains(key))
                            _tilesAlreadySaved.Add(key);
                    }
                }
                else
                {
                    base.GetTileSource(level, row, col, onComplete);
                    //mark this tile need to download, so when tile_loaded, avoid to check if tile exist again.
                    if (!_tilesAlreadySaved.Contains(key))
                        _tilesNeedToSave.Add(key);
                }
            }
            else
            {
                tilebytes = LoadTile(level, row, col);
                if (tilebytes != null)
                {
                    BitmapImage image = new BitmapImage()
                    {
                        CreateOptions = BitmapCreateOptions.DelayCreation
                    };
                    image.BeginInit();
                    image.StreamSource = new MemoryStream(tilebytes);
                    image.EndInit();
                    onComplete(image);
                }
                else
                    onComplete(null);
            }
        }

        private void OfflineableTileLayer_TileLoaded(object sender, TiledLayer.TileLoadEventArgs e)
        {
            //This event is only fired if _isConnected==true
            if (EnableOffline && SaveOfflineTiles)
            {
                string key = string.Format("{0}/{1}/{2}", e.Level, e.Row, e.Column);
                if (SaveTilesMode == Mode.SaveOnly && _tilesNeedToSave.Contains(key) && !_tilesAlreadySaved.Contains(key) && e.ImageStream != null)
                {
                    byte[] bytes = LoadTile(e.Level, e.Row, e.Column);
                    if (bytes == null)
                    {
                        //OneTile oneTile = new OneTile(e.Level, e.Row, e.Column,e.ImageStream);
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(WriteSQLite), oneTile);
                        SaveTile(e.Level, e.Row, e.Column, StreamToBytes(e.ImageStream));
                        //mark this tile already saved, avoid to save it repeatly when LoadOfflineTileFirst=false. 
                        _tilesAlreadySaved.Add(key);
                        //   Debug.WriteLine(key + " tile saved.");
                        _tilesNeedToSave.Remove(key);
                    }
                }
                else if (SaveTilesMode == Mode.SaveOrUpdate && e.ImageStream != null)
                {
                    SaveOrReplaceTile(e.Level, e.Row, e.Column, StreamToBytes(e.ImageStream));
                    //  Debug.WriteLine(key + " tile saved or replaced.");
                }
            }
        }


        private void InitDB()
        {
            //IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            //must implement on UI thread to ensure _conn initialized.
            LogHelper.WriteLog("离线地图路径：" + DBName);
            CheckPath(PublicParams.strDBPath);
            _conn = new SQLiteConnection("Data Source=" + DBName);
            _conn.Open();//will create one in IsolatedStorage if not exist.//            
        }

        /// <summary>
        /// check if the database table to store tiles of this service exists.
        /// if not, create one.
        /// 检查数据库中存储瓦片图的表是否存在，不存在：创建一个
        /// </summary>
        private void InitTileTable()
        {
            //check if the tile table exist
            if (!IsTableExists(this.Url))
            {
                SQLiteTransaction Transaction = _conn.BeginTransaction();
                SQLiteCommand cmd = _conn.CreateCommand();
                cmd.CommandText = @"CREATE TABLE """ + this.Url + @""" (""level"" INTEGER NOT NULL , ""row"" INTEGER NOT NULL , ""column"" INTEGER NOT NULL , ""tile"" BLOB NOT NULL )";
                cmd.ExecuteNonQuery();
                cmd = _conn.CreateCommand();
                cmd.CommandText = "CREATE UNIQUE INDEX 'idx_" + this.Url + "' ON '" + this.Url + "' ('level' ASC, 'row' ASC, 'column' ASC)";
                cmd.ExecuteNonQuery();
                Transaction.Commit(); // CommitTransaction();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="tile"></param>
        private void SaveTile(int level, int row, int column, byte[] tile)
        {
            struOneTiledData oneTile = new struOneTiledData();
            oneTile.tile = tile;
            oneTile.level = level;
            oneTile.row = row;
            oneTile.column = column;
            oneTile.TableName = this.Url;
            Buffer.Enqueue(oneTile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="tile"></param>
        private void SaveOrReplaceTile(int level, int row, int column, byte[] tile)
        {
            struOneTiledData oneTile = new struOneTiledData();
            oneTile.tile = tile;
            oneTile.level = level;
            oneTile.row = row;
            oneTile.column = column;
            oneTile.TableName = this.Url;
            Buffer.Enqueue(oneTile);
        }

        /// <summary>
        /// 
        /// </summary>
        private byte[] LoadTile(int level, int row, int column)
        {
            SQLiteCommand cmd = _conn.CreateCommand();
            cmd.CommandText = "SELECT tile FROM '" + this.Url + "' WHERE level='" + level + "' AND row='" + row + "' AND column='" + column + "'";
            object result = cmd.ExecuteScalar();
            return result != null ? (byte[])result : null;
        }

        private bool IsTableExists(string name)
        {
            SQLiteCommand cmd = _conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='" + name + "'";
            long i = (long)cmd.ExecuteScalar();
            return i == 0 ? false : true;
        }

        private byte[] StreamToBytes(Stream input)
        {
            byte[] bytes = new byte[input.Length];
            input.Read(bytes, 0, bytes.Length);
            return bytes;
        }

        private T ConvertJsonStringToObject<T>(string stringToDeserialize)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] bytes = UnicodeEncoding.Unicode.GetBytes(stringToDeserialize);
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                return (T)serializer.ReadObject(ms);
            }
        }

        private void CheckPath(string path)
        {
            if (path=="")
                return;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
