using System.Threading.Tasks;
using RMC.Core.Exceptions;

namespace RMC.Backend.Baas.Aws
{
	/// <summary>
	/// The main entry point for this <see cref="IBackendSystem"/>
	///
	/// B.U.F.F. For Unity
	///		* Backend
	///		* Unity
	///		* Framework (For)
	///		* Firebase
	/// 
	/// </summary>
	public class Buff : IBackendSystem
	{
		//  Events ----------------------------------------
		public BackendSystemEvent OnInitialized { get; } = new BackendSystemEvent();

		
		//  Properties ------------------------------------
		public static Buff Instance
		{
			get
			{
				if (_Instance == null)
				{
					_Instance = new Buff();
				}
				return _Instance;
			}
		}

		public bool IsInitialized { get { return _isInitialized; }}
		public IAccounts Accounts { get { return _accounts; }}
		public ICloudCode CloudCode { get { return _cloudCode; }}
		public IDatabase Database { get { return _database; }}


		//  Fields ----------------------------------------
		private static Buff _Instance = null;
		private bool _isInitialized = false;
		
		private readonly JawsAccounts _accounts;    //TODO: Replace with new buff-type here...
		private readonly JawsCloudCode _cloudCode;  //TODO: Replace with new buff-type here...
		private readonly JawsDatabase _database;    //TODO: Replace with new buff-type here...
        
		
		//  Initialization --------------------------------
		private Buff()
		{
			// Create all subsystems in constructor
			// To allow for any early subscriptions
			_accounts = new JawsAccounts();
			_cloudCode = new JawsCloudCode();
			_database = new JawsDatabase();
		}
        
		
		/// <summary>
		/// Initialize
		/// </summary>
		public async Task InitializeAsync()
		{
			if (_isInitialized)
			{
				return;
			}
			_isInitialized = true;
	        
			// Initialize Subsystems
			await _accounts.InitializeAsync();
			await _cloudCode.InitializeAsync();
			await _database.InitializeAsync();
	        
			//
			OnInitialized.Invoke(this);
		}
        
		
		/// <summary>
		/// Require initialization. Usage is optional
		/// </summary>
		/// <exception cref="NotInitializedException"></exception>
		public void RequireIsInitialized()
		{
			if (!IsInitialized)
			{
				throw new NotInitializedException(this);
			}
		}

		//  Methods ---------------------------------------

	}
}

