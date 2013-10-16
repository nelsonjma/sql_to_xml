// 
//  ____  _     __  __      _        _ 
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from DbLog on 2013-10-16 22:21:19Z.
// Please visit http://code.google.com/p/dblinq2007/ for more information.
//
namespace DbLoging
{
	using System;
	using System.ComponentModel;
	using System.Data;
#if MONO_STRICT
	using System.Data.Linq;
#else   // MONO_STRICT
	using DbLinq.Data.Linq;
	using DbLinq.Vendor;
#endif  // MONO_STRICT
	using System.Data.Linq.Mapping;
	using System.Diagnostics;
	
	
	public partial class DbLog : DataContext
	{
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		#endregion
		
		
		public DbLog(string connectionString) : 
				base(connectionString)
		{
			this.OnCreated();
		}
		
		public DbLog(string connection, MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			this.OnCreated();
		}
		
		public DbLog(IDbConnection connection, MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			this.OnCreated();
		}
		
		public Table<Log> Log
		{
			get
			{
				return this.GetTable<Log>();
			}
		}
	}
	
	#region Start MONO_STRICT
#if MONO_STRICT

	public partial class DbLog
	{
		
		public DbLog(IDbConnection connection) : 
				base(connection)
		{
			this.OnCreated();
		}
	}
	#region End MONO_STRICT
	#endregion
#else     // MONO_STRICT
	
	public partial class DbLog
	{
		
		public DbLog(IDbConnection connection) : 
				base(connection, new DbLinq.Sqlite.SqliteVendor())
		{
			this.OnCreated();
		}
		
		public DbLog(IDbConnection connection, IVendor sqlDialect) : 
				base(connection, sqlDialect)
		{
			this.OnCreated();
		}
		
		public DbLog(IDbConnection connection, MappingSource mappingSource, IVendor sqlDialect) : 
				base(connection, mappingSource, sqlDialect)
		{
			this.OnCreated();
		}
	}
	#region End Not MONO_STRICT
	#endregion
#endif     // MONO_STRICT
	#endregion
	
	[Table(Name="main.log")]
	public partial class Log : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private string _conn;
		
		private string _created;
		
		private string _execTime;
		
		private string _frameTitle;
		
		private System.Nullable<int> _id;
		
		private string _pageTitle;
		
		private string _siteName;
		
		private string _sql;
		
		private string _status;
		
		private string _xmlfIle;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnConnChanged();
		
		partial void OnConnChanging(string value);
		
		partial void OnCreatedChanged();
		
		partial void OnCreatedChanging(string value);
		
		partial void OnExecTimeChanged();
		
		partial void OnExecTimeChanging(string value);
		
		partial void OnFrameTitleChanged();
		
		partial void OnFrameTitleChanging(string value);
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(System.Nullable<int> value);
		
		partial void OnPageTitleChanged();
		
		partial void OnPageTitleChanging(string value);
		
		partial void OnSiteNameChanged();
		
		partial void OnSiteNameChanging(string value);
		
		partial void OnSQLChanged();
		
		partial void OnSQLChanging(string value);
		
		partial void OnStatusChanged();
		
		partial void OnStatusChanging(string value);
		
		partial void OnXMLFileChanged();
		
		partial void OnXMLFileChanging(string value);
		#endregion
		
		
		public Log()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_conn", Name="conn", DbType="VARCHAR", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Conn
		{
			get
			{
				return this._conn;
			}
			set
			{
				if (((_conn == value) 
							== false))
				{
					this.OnConnChanging(value);
					this.SendPropertyChanging();
					this._conn = value;
					this.SendPropertyChanged("Conn");
					this.OnConnChanged();
				}
			}
		}
		
		[Column(Storage="_created", Name="created", DbType="VARCHAR", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Created
		{
			get
			{
				return this._created;
			}
			set
			{
				if (((_created == value) 
							== false))
				{
					this.OnCreatedChanging(value);
					this.SendPropertyChanging();
					this._created = value;
					this.SendPropertyChanged("Created");
					this.OnCreatedChanged();
				}
			}
		}
		
		[Column(Storage="_execTime", Name="exec_time", DbType="VARCHAR", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string ExecTime
		{
			get
			{
				return this._execTime;
			}
			set
			{
				if (((_execTime == value) 
							== false))
				{
					this.OnExecTimeChanging(value);
					this.SendPropertyChanging();
					this._execTime = value;
					this.SendPropertyChanged("ExecTime");
					this.OnExecTimeChanged();
				}
			}
		}
		
		[Column(Storage="_frameTitle", Name="frame_title", DbType="VARCHAR", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string FrameTitle
		{
			get
			{
				return this._frameTitle;
			}
			set
			{
				if (((_frameTitle == value) 
							== false))
				{
					this.OnFrameTitleChanging(value);
					this.SendPropertyChanging();
					this._frameTitle = value;
					this.SendPropertyChanged("FrameTitle");
					this.OnFrameTitleChanged();
				}
			}
		}
		
		[Column(Storage="_id", Name="id", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_pageTitle", Name="page_title", DbType="VARCHAR", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string PageTitle
		{
			get
			{
				return this._pageTitle;
			}
			set
			{
				if (((_pageTitle == value) 
							== false))
				{
					this.OnPageTitleChanging(value);
					this.SendPropertyChanging();
					this._pageTitle = value;
					this.SendPropertyChanged("PageTitle");
					this.OnPageTitleChanged();
				}
			}
		}
		
		[Column(Storage="_siteName", Name="site_name", DbType="VARCHAR", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string SiteName
		{
			get
			{
				return this._siteName;
			}
			set
			{
				if (((_siteName == value) 
							== false))
				{
					this.OnSiteNameChanging(value);
					this.SendPropertyChanging();
					this._siteName = value;
					this.SendPropertyChanged("SiteName");
					this.OnSiteNameChanged();
				}
			}
		}
		
		[Column(Storage="_sql", Name="sql", DbType="TEXT", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string SQL
		{
			get
			{
				return this._sql;
			}
			set
			{
				if (((_sql == value) 
							== false))
				{
					this.OnSQLChanging(value);
					this.SendPropertyChanging();
					this._sql = value;
					this.SendPropertyChanged("SQL");
					this.OnSQLChanged();
				}
			}
		}
		
		[Column(Storage="_status", Name="status", DbType="VARCHAR", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Status
		{
			get
			{
				return this._status;
			}
			set
			{
				if (((_status == value) 
							== false))
				{
					this.OnStatusChanging(value);
					this.SendPropertyChanging();
					this._status = value;
					this.SendPropertyChanged("Status");
					this.OnStatusChanged();
				}
			}
		}
		
		[Column(Storage="_xmlfIle", Name="xml_file", DbType="VARCHAR", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string XMLFile
		{
			get
			{
				return this._xmlfIle;
			}
			set
			{
				if (((_xmlfIle == value) 
							== false))
				{
					this.OnXMLFileChanging(value);
					this.SendPropertyChanging();
					this._xmlfIle = value;
					this.SendPropertyChanged("XMLFile");
					this.OnXMLFileChanged();
				}
			}
		}
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
