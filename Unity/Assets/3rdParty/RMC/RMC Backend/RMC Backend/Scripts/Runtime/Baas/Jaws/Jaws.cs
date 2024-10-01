using System.Threading.Tasks;
using RMC.Core.Exceptions;

namespace RMC.Backend.Baas.Aws
{
	/// <summary>
	/// The main entry point for this <see cref="IBackendSystem"/>
	///
	/// J.A.W.S. For Unity
	///		* Just (For Unity And)
	///		* Amazon
	///		* Web
	///		* Services
	/// 
	/// </summary>
	public class Jaws : IBackendSystem
	{
		//  Events ----------------------------------------
		public BackendSystemEvent OnInitialized { get; } = new BackendSystemEvent();

		
		//  Properties ------------------------------------
		public static Jaws Instance
		{
			get
			{
				if (_Instance == null)
				{
					_Instance = new Jaws();
				}
				return _Instance;
			}
		}

		public bool IsInitialized { get { return _isInitialized; }}
		public IAccounts Accounts { get { return _accounts; }}
		public ICloudCode CloudCode { get { return _cloudCode; }}
		public IDatabase Database { get { return _database; }}


		//  Fields ----------------------------------------
		private static Jaws _Instance = null;
		private bool _isInitialized = false;
		private readonly JawsAccounts _accounts;
		private readonly JawsCloudCode _cloudCode;
		private readonly JawsDatabase _database;
        
		
		//  Initialization --------------------------------
		private Jaws()
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

