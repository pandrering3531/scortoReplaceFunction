using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Configuration;
using System.Web.Configuration;
using System.Security.Cryptography;
using STPC.DynamicForms.Web.Common.Services.Users;
using System.Threading;
using STPC.DynamicForms.Core;
using System.Resources;
using STPC.DynamicForms.Web.Common.Messages;
using System.Web;
using System.Net;

namespace STPC.DynamicForms.Web.Common
{
	public sealed class CustomMembershipProvider : MembershipProvider
	{
		public override string ApplicationName
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public override bool ChangePassword(string username, string oldPassword, string newPassword)
		{
			ValidationStringPassWord(newPassword);
			//Valida usuario
			if (ValidateUser(username, oldPassword))
			{
				//Busca en el diccionario que el pws no tenga palabras no permitidas.
				if (ValidateForbiddenPassword(newPassword))
				{

					string newpasswordencoded = EncodePassword(newPassword);

					using (UserServiceClient srv = new UserServiceClient())
					{
						//Valida que la contraseña nueva no haya sido utilizada anteriormente. 
						if (srv.ValidateLastPassWordHistroy(username, newpasswordencoded))
						{
							srv.ResetPassword(username, newpasswordencoded);

							//Crea historico de la modificación de la contraseña
							srv.AddHistoryPassWord(username, EncodePassword(oldPassword));
							return true;
						}
						else
							throw new Exception("La contraseña proporcionada ya fue utilizada anteriormente");
					}
				}
				else
					throw new Exception("La contraseña contiene palabras no permitidas");
			}
			return false;
		}

		private void ValidationStringPassWord(string newPassword)
		{
			if (this.PasswordStrengthRegularExpression != string.Empty)
			{
				if (!ValidateRegularExpresion(newPassword))
					throw new Exception(string.Format(Constants.ERROR_PASSWORD_FORMAT));
			}
			else
			{
				//Valida que la cadena tenga un mínimo de caracteres
				if (this.MinRequiredPasswordLength > newPassword.Length)
					throw new Exception(string.Format(Constants.ERROR_MIN_REQUIRED_PASSWORD_LENGTH));

				//Valida que la cadena tenga caracteres no alfanuméricos
				if (ValidateminRequiredNonAlphanumericCharacters(newPassword) < MinRequiredNonAlphanumericCharacters)
					throw new Exception(string.Format(Constants.ERROR_REQUIRED_NON_ALPHANUMERIC_CHAR, MinRequiredNonAlphanumericCharacters));

				//Valida que la cadena tenga caracteres numéricos
				if (ValidateminRequiredNumericCharacters(newPassword) < MinRequiredNumericCharacters)
					throw new Exception(string.Format(Constants.ERROR_REQUIRED_NUMERIC_CHAR, MinRequiredNumericCharacters));

				if (ValidateMinAlphaCharacters(newPassword) < pMinRequiredAlphaCharacters)
					throw new Exception(string.Format(Constants.ERROR_REQUIRED_ALPHA_CHAR, pMinRequiredAlphaCharacters));

				if (ValidateMinUpperCharacters(newPassword) < pMinRequiredUpperCharacters)
					throw new Exception(string.Format(Constants.ERROR_REQUIRED_UPPER_CHAR, pMinRequiredUpperCharacters));

				if (ValidateMinLowerCharacters(newPassword) < pMinRequiredLowerCharacters)
					throw new Exception(string.Format(Constants.ERROR_REQUIRED_LOWER_CHAR, pMinRequiredLowerCharacters));
			}
		}

		/// <summary>
		/// Valida que la contraseña tengo o no caracteres NO alfanumericos
		/// </summary>
		/// <param name="newPassword"></param>
		/// <returns></returns>
		private int ValidateminRequiredNonAlphanumericCharacters(string newPassword)
		{

			int NumNonAlphaNumericChars = 0;

			for (int i = 0; i < newPassword.Length; i++)
			{
				char a = newPassword[i];
				if ((a < 48) | (a > 57 & a < 65) | (a > 90 & a < 97) | (a > 122))
				{
					NumNonAlphaNumericChars++;
				}
			}

			return NumNonAlphaNumericChars;

		}

		/// <summary>
		/// Valida la cantidad de caracteres numéricios de la nueva contraseña
		/// </summary>
		/// <param name="newPassword"></param>
		/// <returns></returns>
		private int ValidateminRequiredNumericCharacters(string newPassword)
		{

			int NumNumericChars = 0;


			for (int i = 0; i < newPassword.Length; i++)
			{
				char a = newPassword[i];
				if ((a >= 48) && (a <= 57))
				{
					NumNumericChars++;
				}
			}

			return NumNumericChars;

		}

		/// <summary>
		/// Valida la cantidad de caracteres alfabéticos de la nueva contraseña
		/// </summary>
		/// <param name="newPassword"></param>
		/// <returns></returns>
		private int ValidateMinAlphaCharacters(string newPassword)
		{

			int NumAlphacChars = 0;
			string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ";

			for (int i = 0; i < newPassword.Length; i++)
			{
				char a = newPassword[i];
				if (allowedChars.IndexOf(a) >= 0)
				{
					NumAlphacChars++;
				}
			}

			return NumAlphacChars;

		}

		/// <summary>
		/// Valida la cantidad de caracteres Mayusculas de la nueva contraseña
		/// </summary>
		/// <param name="newPassword"></param>
		/// <returns></returns>
		private int ValidateMinUpperCharacters(string newPassword)
		{

			int NumAlphacChars = 0;
			string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZ";

			for (int i = 0; i < newPassword.Length; i++)
			{
				char a = newPassword[i];
				if (allowedChars.IndexOf(a) >= 0)
				{
					NumAlphacChars++;
				}
			}

			return NumAlphacChars;

		}

		/// <summary>
		/// Valida la cantidad de caracteres minusculas de la nueva contraseña
		/// </summary>
		/// <param name="newPassword"></param>
		/// <returns></returns>
		private int ValidateMinLowerCharacters(string newPassword)
		{

			int NumAlphacChars = 0;
			string allowedChars = "abcdefghijklmnopqrstuvwxyz";

			for (int i = 0; i < newPassword.Length; i++)
			{
				char a = newPassword[i];
				if (allowedChars.IndexOf(a) >= 0)
				{
					NumAlphacChars++;
				}
			}

			return NumAlphacChars;

		}


		private bool ValidateRegularExpresion(string newPassWord)
		{
			//string sPattern = "^\\d{3}-\\d{3}-\\d{4}$";
			string sPattern = PasswordStrengthRegularExpression;
			string sPatternFormat = @sPattern;
			if (System.Text.RegularExpressions.Regex.IsMatch(newPassWord, sPatternFormat))
			{
				return true;
			}
			else
				return false;

		}

		public bool ChangePasswordByChangePassword(string username, string newPassword)
		{
			//Valida usuario
			ValidationStringPassWord(newPassword);
			string oldPassword = string.Empty;

			if (ValidateUserByChangePassword(username, ref oldPassword, false))
			{
				//Busca en el diccionario que el pws no tenga palabras no permitidas.
				if (ValidateForbiddenPassword(newPassword))
				{

					string newpasswordencoded = EncodePassword(newPassword);

					using (UserServiceClient srv = new UserServiceClient())
					{
						//Valida que la contraseña nueva no haya sido utilizada anteriormente. 
						if (srv.ValidateLastPassWordHistroy(username, newpasswordencoded))
						{
							srv.ResetPassword(username, newpasswordencoded);

							//Crea historico de la modificación de la contraseña
							srv.AddHistoryPassWord(username, oldPassword);
							return true;
						}
						else
							throw new Exception("La contraseña proporcionada ya fue utilizada anteriormente");
					}
				}
				else
					throw new Exception("La contraseña contiene palabras no permitidas");
			}
			return false;
		}

		public void UpdateFiedldIsResetPassword(string username, bool state)
		{
			using (UserServiceClient srv = new UserServiceClient())
			{

        var theUser = srv.GetUser(username, false);
				srv.UpdateFiedldIsResetPassword(username, state);
        srv.UpdateUserLastLoginDateAndIp(theUser, GetVisitorIPAddress());

			}
		}
		private bool ValidateForbiddenPassword(string password)
		{

			//Carga diccionario de palabras no permitidas
			using (UserServiceClient srv = new UserServiceClient())
			{
				var listForbiddenPassword = srv.GetListForbiddenPassword();
				//Busca en la lista las coincidencias
				foreach (var item in listForbiddenPassword)
				{
					int firstCharacter = password.ToUpper().IndexOf(item.ToUpper());

					if (firstCharacter >= 0)
						return false;
				}

			}
			return true;

		}
		public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
		{
			throw new NotImplementedException();
		}

		public override MembershipUser CreateUser(
			string username,
			string password,
			string email,
			string passwordQuestion,
			string passwordAnswer,
			bool isApproved,
			object providerUserKey,
			out MembershipCreateStatus status)
		{
			var user = new User
			{
				Username = username,
				PasswordQuestion = passwordQuestion,
				Email = email,
				IsApproved = true,
				IsLockedOut = true,
				CreationDate = DateTime.Now,
				LastLoginDate = DateTime.Now,
				LastActivityDate = DateTime.Now,
				LastPasswordChangedDate = DateTime.Now,
				LastLockedOutDate = DateTime.Now,
				GivenName = null,
				LastName = null,
				Phone_LandLine = null,
				Hierarchy = null
			};

			return this.CreateUser(user, out status);
		}

		public MembershipUser CreateUser(User user,
		 out MembershipCreateStatus status)
		{
			ValidatePasswordEventArgs args =
			  new ValidatePasswordEventArgs(user.Username, user.Password, true);

			ValidationStringPassWord(user.Password);

			OnValidatingPassword(args);

			if (args.Cancel)
			{
				status = MembershipCreateStatus.InvalidPassword;
				return null;
			}

			if (RequiresUniqueEmail && !String.IsNullOrEmpty(GetUserNameByEmail(user.Email)))
			{
				status = MembershipCreateStatus.DuplicateEmail;
				return null;
			}
			MembershipUser u = GetUser(user.Username, false);
			if (u == null)
			{
				user.Password = EncodePassword(user.Password);

				if (user.PasswordAnswer != null)
					user.PasswordAnswer = EncodePassword(user.PasswordAnswer);
				try
				{
					using (UserServiceClient srv = new UserServiceClient())
					{
						status = srv.CreateAccount(user);
					}
				}
				catch
				{
					status = MembershipCreateStatus.ProviderError;
				}
				return GetUser(user.Username, false);
			}
			else
			{
				status = MembershipCreateStatus.DuplicateUserName;
			}
			return null;
		}



		public override bool DeleteUser(string username, bool deleteAllRelatedData)
		{
			throw new NotImplementedException();
		}

		public override bool EnablePasswordReset
		{
			get { throw new NotImplementedException(); }
		}

		public override bool EnablePasswordRetrieval
		{
			get { throw new NotImplementedException(); }
		}

		public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
		{
			using (UserServiceClient srv = new UserServiceClient())
			{
				var users = srv.GetAllUsers(out totalRecords, pageIndex, pageSize);
				MembershipUserCollection membershipusers = new MembershipUserCollection();
				foreach (var user in users)
					membershipusers.Add(GetMembershipUserFromUser(user));
				return membershipusers;
			}
		}

		public ICollection<User> GetAllUsers()
		{
			using (UserServiceClient srv = new UserServiceClient())
			{
				int totalRecords;
				var users = srv.GetAllUsers(out totalRecords, 0, int.MaxValue);


				return users;
			}
		}
		public ICollection<User> GetAllUsersIndicatora()
		{
			using (UserServiceClient srv = new UserServiceClient())
			{
				int totalRecords;
				var users = srv.GetAllUsersIndicatora();


				return users;
			}
		}
		public ICollection<User> GetAllUsersPaged(int pageIndex, int pageSize,  out int totalRecords)
		{
			using (UserServiceClient srv = new UserServiceClient())
			{

				var users = srv.GetAllUsers(out totalRecords, pageIndex, pageSize);


				return users;
			}
		}
		public ICollection<User> GetAllUsersPaged(int pageIndex, int pageSize, int totalRecords, int totalPages)
		{
			using (UserServiceClient srv = new UserServiceClient())
			{

				var users = srv.GetAllUsers(out totalRecords,pageIndex, pageSize);


				return users;
			}
		}


		public ICollection<User> GetAllUsersPaged(int pageIndex, int pageSize, out int totalRecords, out int totalPages, string searchText)
		{
			using (UserServiceClient srv = new UserServiceClient())
			{

				var users = srv.GetAllUsersFilter(out totalRecords, out totalPages, pageIndex, pageSize, searchText);


				return users;
			}
		}


		public ICollection<User> GetAllUsersCompleteName()
		{
			using (UserServiceClient srv = new UserServiceClient())
			{
				int totalRecords;
				var users = srv.GetAllUsersCompleteName(out totalRecords, 0, int.MaxValue);


				return users;
			}
		}
		public ICollection<User> GetAllUsersByAplicationName(int aplicationNameId)
		{
			using (UserServiceClient srv = new UserServiceClient())
			{
				int totalRecords;
				var users = srv.GetAllUsersByAplicationName(out totalRecords, 0, int.MaxValue, aplicationNameId);


				return users;
			}
		}

		//public ICollection<User> GetListForbiddenPassword()
		//{
		//  using (UserServiceClient srv = new UserServiceClient())
		//  {
		//    int totalRecords;
		//    var users = srv.GetListForbiddenPassword();
		//    return users;
		//  }
		//}


		public override int GetNumberOfUsersOnline()
		{
			throw new NotImplementedException();
		}

		public override string GetPassword(string username, string answer)
		{
			throw new NotImplementedException();
		}

		public override MembershipUser GetUser(string username, bool userIsOnline)
		{
			using (UserServiceClient srv = new UserServiceClient())
			{
				var user = srv.GetUser(username, userIsOnline);
				if (user == null) return null;
				return GetMembershipUserFromUser(user);
			}
		}

		public User GetUser(string username)
		{
			using (UserServiceClient srv = new UserServiceClient())
			{
				var user = srv.GetUser(username, false);
				if (user == null) return null;
				return user;
			}
		}

		private MembershipUser GetMembershipUserFromUser(User user)
		{
			return new MembershipUser(this.Name,
				user.Username,
				user.Id,
				user.Email,
				user.PasswordQuestion,
				string.Empty,
				user.IsApproved,
				user.IsLockedOut,
				user.CreationDate,
				user.LastLoginDate,
				user.LastActivityDate,
				user.LastPasswordChangedDate,
				user.LastLockedOutDate);
		}

		public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
		{
			throw new NotImplementedException();
		}

		public override string GetUserNameByEmail(string email)
		{
			using (UserServiceClient srv = new UserServiceClient())
			{
				return srv.GetUserNameByEmail(email);
			}
		}

		public override int MaxInvalidPasswordAttempts
		{
			get { return pMaxInvalidPasswordAttempts; }
		}

		private int pMinRequiredNonAlphanumericCharacters;

		public override int MinRequiredNonAlphanumericCharacters
		{
			get { return pMinRequiredNonAlphanumericCharacters; }
		}


		private int pMinRequiredNumericCharacters;

		public int MinRequiredNumericCharacters
		{
			get { return pMinRequiredNumericCharacters; }
		}

		private int pMinRequiredAlphaCharacters;

		public int MinRequiredAlphaCharacters
		{
			get { return pMinRequiredAlphaCharacters; }
		}

		private int pMinRequiredUpperCharacters;

		public int MinRequiredUpperCharacters
		{
			get { return pMinRequiredUpperCharacters; }
		}

		private int pMinRequiredLowerCharacters;

		public int MinRequiredLowerCharacters
		{
			get { return pMinRequiredLowerCharacters; }
		}

		private int pMinRequiredPasswordLength;

		public override int MinRequiredPasswordLength
		{
			get { return pMinRequiredPasswordLength; }
		}

		private int pPasswordMaxAge;

		private int pPasswordMaxAgeAlert;

		private int pResetQuestionsPoolNumber;

		private int loginErrorTimeDelay;

		private int pCaptchaInvalidPasswordAttempts;

		public int CaptchaInvalidPasswordAttempts
		{
			get { return pCaptchaInvalidPasswordAttempts; }
		}


		public int LoginErrorTimeDelay
		{
			get { return loginErrorTimeDelay; }
		}


		public int PasswordMaxAge
		{
			get { return pPasswordMaxAge; }
		}

		public int PasswordMaxAgeAlert
		{
			get { return pPasswordMaxAgeAlert; }
		}

		public int ResetQuestionsPoolNumber
		{
			get { return pResetQuestionsPoolNumber; }
		}


		private string pPasswordStrengthRegularExpression;

		public override string PasswordStrengthRegularExpression
		{
			get { return pPasswordStrengthRegularExpression; }
		}

		public override int PasswordAttemptWindow
		{
			get { return pPasswordAttemptWindow; }
		}

		public int ResetQuestionsNumber
		{
			get { return pResetQuestionsNumber; }
		}

		public override MembershipPasswordFormat PasswordFormat
		{
			get { return pPasswordFormat; }
		}

		public override bool RequiresQuestionAndAnswer
		{
			get { return pRequiresQuestionAndAnswer; }
		}

		public override bool RequiresUniqueEmail
		{
			get { return pRequiresUniqueEmail; }
		}

		public override string ResetPassword(string username, string answer)
		{
			throw new NotImplementedException();
		}

		public override bool UnlockUser(string userName)
		{
			using (UserServiceClient srv = new UserServiceClient())
			{
				srv.UnlockUser(userName);
			}
			return true;
		}

		public bool LockUser(string userName)
		{
			using (UserServiceClient srv = new UserServiceClient())
			{
				srv.LockUser(userName);
			}
			return true;
		}

		public bool ApproveUser(string userName)
		{
			using (UserServiceClient srv = new UserServiceClient())
			{
				srv.ActivateUser(userName);
			}
			return true;
		}
		public override void UpdateUser(MembershipUser user)
		{
			throw new NotImplementedException();
		}

		public void UpdateUser(User user)
		{

			using (UserServiceClient srv = new UserServiceClient())
			{
				srv.UpdateUser(user);
			}

		}

		public void CloseSesion(string userName)
		{
			User theUser;
			
			using (UserServiceClient srv = new UserServiceClient())
			{
				theUser = srv.GetUser(userName, false);

				if (theUser != null)
				{
					theUser.IsOnLine = false;
					UpdateUser(theUser);

					
				}
			}
		}

		public string ResetPassword(string username)
		{
			string newpassword = GenerateHumanFriendlyPassword();
			string newpasswordencoded = EncodePassword(newpassword);
			using (UserServiceClient srv = new UserServiceClient())
			{
				srv.ResetPassword(username, newpasswordencoded);
			}
			return newpassword;
		}

		private string GenerateHumanFriendlyPassword()
		{
			string[] passwordRoots = { "Medellin", "Bogota", "Barranquilla", "Popayan", "Cartagena",
				"Girardot", "Pereira", "Villavicencio", "Bucaramanga", "Cucuta" };

			Random r = new Random(DateTime.Now.Millisecond);
			int random = r.Next(10, 1000);
			int root = random % 10;
			return passwordRoots[root] + random.ToString();
		}

		public override bool ValidateUser(string username, string password)
		{
			bool isValid = false;
			User theUser;
			using (UserServiceClient srv = new UserServiceClient())
			{
				theUser = srv.GetUser(username, false);

				if (DateTime.Now >= theUser.LastPasswordChangedDate.AddDays(pPasswordMaxAge))
					throw new Exception("La contraseña ha expirado");


				if (theUser == null)
					throw new Exception("Nombre de usuario no válido");

				if (CheckPassword(password, theUser.Password))
				{
					if (theUser.IsApproved)
					{
						isValid = true;
						srv.UpdateUserLastLoginDate(theUser.Username);

					}
				}
				else
				{
					//Ingrementa el numero de intentos fallidos de la contraseña
					int attemptCount = 0;
					attemptCount = srv.UpdatePassWordAttemptCount(username);
					Thread.Sleep(((attemptCount * loginErrorTimeDelay) * 1000));
					throw new Exception("Nombre de usuario o contraseña no válido");
				}
			}
			return isValid;
		}

		public bool ValidateUser(string username, string password, bool viewCaptcha, ref bool isResetPassword
			, ref bool showCaptcha, ref int attemptCount)
		{
			bool isValid = false;
			User theUser;
			using (UserServiceClient srv = new UserServiceClient())
			{
				theUser = srv.GetUser(username, false);
				//theUser = srv.GetBasicInfoUser(username);

				if (theUser == null)
				{

					showCaptcha = (attemptCount) >= this.pCaptchaInvalidPasswordAttempts ? true : false;
					Thread.Sleep(((attemptCount * (loginErrorTimeDelay)) * 1000));
					attemptCount++;
					return false;

				}


				if (DateTime.Now >= theUser.LastPasswordChangedDate.AddDays(pPasswordMaxAge) && !theUser.IsResetPassword)
					throw new Exception("La contraseña ha expirado");

				string resetPasswordLifetime = System.Configuration.ConfigurationManager.AppSettings["resetPasswordLifetime"];

				DateTime expirateDate = theUser.LastPasswordChangedDate.AddHours(Convert.ToInt32(resetPasswordLifetime));


				if (expirateDate <= DateTime.Now && theUser.IsResetPassword == true)
          throw new Exception("La primera contraseña ha expirado");
				if (expirateDate >= DateTime.Now && theUser.IsResetPassword == true)
					theUser.IsResetPassword = true;

				if (CheckPassword(password, theUser.Password))
				{
					if (theUser.IsApproved)
					{
						isValid = true;

						if (!ValidateSessionTimeOut(theUser.LastActivityDate, theUser.IsOnLine))
							throw new Exception(Constants.ANOTHER_SESSION_ACTIVE);

                        if (theUser.IsLockedOut)
                            throw new Exception(Constants.ERROR_USER_LOCKED_OUT);

						if (!viewCaptcha)
							showCaptcha = theUser.FailedPasswordAttemptCount >= this.pCaptchaInvalidPasswordAttempts ? true : false;


						if (!showCaptcha)
						{

							if (!theUser.IsResetPassword)
								srv.UpdateUserLastLoginDateAndIp(theUser, GetVisitorIPAddress());


						}

						isResetPassword = theUser.IsResetPassword;

					}
				}
				else
				{
					//Ingrementa el numero de intentos fallidos de la contraseña
					//attemptCount = 0;
					int attemptCountExisting = 0;
					attemptCountExisting = srv.UpdatePassWordAttemptCount(username);

					showCaptcha = (attemptCountExisting + attemptCount) >= this.pCaptchaInvalidPasswordAttempts ? true : false;

					Thread.Sleep((((attemptCountExisting + attemptCount) * loginErrorTimeDelay) * 1000));


					return false;
				}
			}

			//Valida si la sesión expiró


			return isValid;
		}

		public bool ValidateExternalUser(string username)
		{
			bool isValid = false;

			User theUser;
			using (UserServiceClient srv = new UserServiceClient())
			{
				theUser = srv.GetUser(username, false);
				srv.UpdateUserLastLoginDateAndIp(theUser, GetVisitorIPAddress());
				isValid = theUser.IsApproved;
			}

			return isValid;
		}

		public double GetTimeOut()
		{

			double timeOut = 0;
			var configuration = WebConfigurationManager.OpenWebConfiguration("/");
			var authenticationSection = (AuthenticationSection)configuration.GetSection("system.web/authentication");

			if (authenticationSection.Mode == AuthenticationMode.Forms)
			{
				timeOut = authenticationSection.Forms.Timeout.TotalMinutes;
			}
			return timeOut;
		}

		/// <summary>
		/// Valida si el usuarios está en linea o ya expiro la session
		/// </summary>
		/// <returns></returns>
		private bool ValidateSessionTimeOut(DateTime LastActivityDate, bool isOnLine)
		{
			double timeOutConfig = GetTimeOut();
			DateTime expiredSesionDate = LastActivityDate.AddMinutes(timeOutConfig);

			if (DateTime.Now < expiredSesionDate && isOnLine)
				return false;

			if (!isOnLine)
				return true;

			if (DateTime.Now > expiredSesionDate && isOnLine)
				return true;

			return true;

		}

		public bool ValidateUserByChangePassword(string username, ref string oldPassword, bool isValidateExpiredPassword = true)
		{

			User theUser;

			using (UserServiceClient srv = new UserServiceClient())
			{
				theUser = srv.GetUser(username, false);

				if (isValidateExpiredPassword)
					if (DateTime.Now >= theUser.LastPasswordChangedDate.AddDays(pPasswordMaxAge))
						throw new Exception("La contraseña ha expirado");


				if (theUser == null)
					return false;

				oldPassword = theUser.Password;
				return true;
			}

		}

		public bool ValidatePasswordAnswer(string username, string passwordAnswer)
		{
			bool isValid = false;
			User theUser;
			using (UserServiceClient srv = new UserServiceClient())
			{
				theUser = srv.GetUser(username, false);
				if (theUser == null) return false;
				if (CheckPassword(passwordAnswer, theUser.PasswordAnswer))
				{
					isValid = true;
				}
			}
			return isValid;
		}

		/// <summary>
		/// Initialize the provider based on the configuration file
		/// </summary>
		/// <param name="name"></param>
		/// <param name="config"></param>
		public override void Initialize(string name, NameValueCollection config)
		{
			if (config == null)
				throw new ArgumentNullException("config");

			if (name == null || name.Length == 0)
				name = "AbcMembershipProvider";

			if (String.IsNullOrEmpty(config["description"]))
			{
				config.Remove("description");
				config.Add("description", "ABC Membership provider");
			}

			// Initialize the abstract base class.
			base.Initialize(name, config);

			pApplicationName = GetConfigValue(config["applicationName"],
										 System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
			pMaxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
			pPasswordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
			pMinRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "0"));

			pMinRequiredNumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNumericCharacters"], "0"));
			pMinRequiredAlphaCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredAlphaCharacters"], "0"));
			pMinRequiredUpperCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredUpperCharacters"], "0"));
			pMinRequiredLowerCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredLowerCharacters"], "0"));


			pMinRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"));

			pPasswordStrengthRegularExpression = Convert.ToString(GetConfigValue(config["passwordStrengthReqularExpression"], ""));

			pEnablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"));
			pEnablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], "true"));
			pRequiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
			pRequiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));
			pPasswordMaxAge = Convert.ToInt32(GetConfigValue(config["passwordMaxAge"], "true"));
			pPasswordMaxAgeAlert = Convert.ToInt32(GetConfigValue(config["passwordMaxAgeAlert"], "true"));
			//pWriteExceptionsToEventLog = Convert.ToBoolean(GetConfigValue(config["writeExceptionsToEventLog"], "true"));
			pResetQuestionsNumber = Convert.ToInt32(GetConfigValue(config["resetQuestionsNumber"], "10"));
			pResetQuestionsPoolNumber = Convert.ToInt32(GetConfigValue(config["resetQuestionsPoolNumber"], "10"));
			loginErrorTimeDelay = Convert.ToInt32(GetConfigValue(config["loginErrorTimeDelay"], "10"));
			pCaptchaInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["CaptchaInvalidPasswordAttempts"], "10"));
			string temp_format = config["passwordFormat"];
			if (temp_format == null)
			{
				temp_format = "Hashed";
			}

			switch (temp_format)
			{
				case "Hashed":
					pPasswordFormat = MembershipPasswordFormat.Hashed;
					break;
				case "Encrypted":
					pPasswordFormat = MembershipPasswordFormat.Encrypted;
					break;
				case "Clear":
					pPasswordFormat = MembershipPasswordFormat.Clear;
					break;
				default:
					throw new ProviderException("Password format not supported.");
			}
			// Get encryption and decryption key information from the configuration.
			Configuration cfg =
			  WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
			machineKey = (MachineKeySection)cfg.GetSection("system.web/machineKey");

			if (machineKey.ValidationKey.Contains("AutoGenerate"))
				if (PasswordFormat != MembershipPasswordFormat.Clear)
					throw new ProviderException("Hashed or Encrypted passwords " +
											 "are not supported with auto-generated keys.");
		}


		//
		// A helper function to retrieve config values from the configuration file.
		//

		private string GetConfigValue(string configValue, string defaultValue)
		{
			if (String.IsNullOrEmpty(configValue))
				return defaultValue;

			return configValue;
		}

		//
		// Used when determining encryption key values.
		//
		private MachineKeySection machineKey;
		//
		// System.Web.Security.MembershipProvider properties.
		//

		private string pApplicationName;
		private bool pEnablePasswordReset;
		private bool pEnablePasswordRetrieval;
		private bool pRequiresQuestionAndAnswer;
		private bool pRequiresUniqueEmail;
		private int pMaxInvalidPasswordAttempts;
		private int pPasswordAttemptWindow;
		private int pResetQuestionsNumber;
		private MembershipPasswordFormat pPasswordFormat;


		/// <summary>
		/// Encrypts, Hashes, or leaves the password clear based on the PasswordFormat.
		/// </summary>
		/// <param name="password"></param>
		/// <returns></returns>
		private string EncodePassword(string password)
		{
			string encodedPassword = password;

			switch (PasswordFormat)
			{
				case MembershipPasswordFormat.Clear:
					break;
				case MembershipPasswordFormat.Encrypted:
					encodedPassword =
					  Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)));
					break;
				case MembershipPasswordFormat.Hashed:
					HMACSHA1 hash = new HMACSHA1();
					hash.Key = HexToByte(machineKey.ValidationKey);
					encodedPassword =
					  Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
					break;
				default:
					throw new ProviderException("Unsupported password format.");
			}

			return encodedPassword;
		}


		/// <summary>
		/// Decrypts or leaves the password clear based on the PasswordFormat.
		/// </summary>
		/// <param name="encodedPassword"></param>
		/// <returns></returns>
		private string UnEncodePassword(string encodedPassword)
		{
			string password = encodedPassword;

			switch (PasswordFormat)
			{
				case MembershipPasswordFormat.Clear:
					break;
				case MembershipPasswordFormat.Encrypted:
					password =
					  Encoding.Unicode.GetString(DecryptPassword(Convert.FromBase64String(password)));
					break;
				case MembershipPasswordFormat.Hashed:
					throw new ProviderException("Cannot unencode a hashed password.");
				default:
					throw new ProviderException("Unsupported password format.");
			}

			return password;
		}

		public string[] UnEncodeAnswer(string[] anwers)
		{
			string[] encodedAnswer = new string[anwers.Length];

			for (int i = 0; i < anwers.Length; i++)
			{
				encodedAnswer[i] = EncodePassword(anwers[i]);
			}

			return encodedAnswer;
		}

		/// <summary>
		/// Calcula si es necesario alertar al usuario que está pronta a vencer la contraseña
		/// </summary>
		/// <param name="userName">Nombre de usuario</param>
		/// <returns>Número de dias a vences. Si el valor es -1 quiere decir que no es necesario alertar al usuario. </returns>
		public int EsxpirarDaysPassword(User theUser)
		{
	
			using (UserServiceClient srv = new UserServiceClient())
			{
				

				//Calcular la fecha en que vence
				DateTime EsxpirarDate = theUser.LastPasswordChangedDate.AddDays(pPasswordMaxAge);

				//calcula la fecha desde la cual debe alertar al usuario
				TimeSpan ts = EsxpirarDate - DateTime.Now;

				//Saca la diferencia de las fechas en dias
				int differenceInDays = ts.Days;

				//Compara si la diferencia en dias es mayor o igual al parametro que indica antes de n dias alertar al usuario
				if (differenceInDays <= pPasswordMaxAgeAlert)
				{
					return differenceInDays;
				}
				else
					//Retorna -1 en caso que no sea necesario alertar al usuario
					return -1;
			}
		}

		/// <summary>
		/// Converts a hexadecimal string to a byte array. Used to convert encryption key values from the configuration.
		/// </summary>
		/// <param name="hexString"></param>
		/// <returns></returns>
		private byte[] HexToByte(string hexString)
		{
			byte[] returnBytes = new byte[hexString.Length / 2];
			for (int i = 0; i < returnBytes.Length; i++)
				returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
			return returnBytes;
		}

		private bool CheckPassword(string password, string dbpassword)
		{
			string pass1 = password;
			string pass2 = dbpassword;

			switch (PasswordFormat)
			{
				case MembershipPasswordFormat.Encrypted:
					pass2 = UnEncodePassword(dbpassword);
					break;
				case MembershipPasswordFormat.Hashed:
					pass1 = EncodePassword(password);
					break;
				default:
					break;
			}

			if (pass1 == pass2)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Carga la lista de preguntas para recuperar la contraseña, por usuario
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>
		public ICollection<UserResetQuestion> GetUserResetQuestion(string userName)
		{
			List<UserResetQuestion> listUserResetQuestion = null;
			List<UserResetQuestion> listUserResetQuestionTake = new List<UserResetQuestion>();
			using (UserServiceClient srv = new UserServiceClient())
			{
				var user = srv.GetUser(userName, false);


				if (user != null)
				{
					listUserResetQuestion = srv.GetQuestionsUser(user.Id).ToList();
					int[] numeros = new int[listUserResetQuestion.Count];

					Random r = new Random();

					for (int i = 0; i < listUserResetQuestion.Count; i++)
					{
						numeros[i] = r.Next(0, listUserResetQuestion.Count);

						if (i > 0)    // a partir del segundo numero que genera empezara a comparar que no se repita
						{
							for (int x = 0; x < 50; x++)  //comprobara que no se repita por 50 veces
							{

								for (int j = 0; j < i; j++)
								{
									if (numeros[i] == numeros[j])
									{
										numeros[i] = r.Next(0, listUserResetQuestion.Count);
									}
								}
							}
						}

					}

					if (listUserResetQuestion.Count > 0)
					{
						if (pResetQuestionsNumber <= listUserResetQuestion.Count)
						{
							if (listUserResetQuestion.Count > 0)
								for (int i = 0; i < pResetQuestionsNumber; i++)
								{
									listUserResetQuestionTake.Add(listUserResetQuestion[numeros[i]]);
								}


							if (listUserResetQuestionTake.Count() > 0)
								return listUserResetQuestionTake;
							else
								throw new Exception(Constants.ERROR_QUESTION_NOT_CONFIGURED);
						}
						else
						{
							throw new Exception(Constants.ERROR_CONFIG_RESET_QUESTION_NUMBRER);
						}
					}
					else
					{
						throw new Exception(Constants.ERROR_QUESTION_NOT_CONFIGURED);
					}
				}
				else
					throw new Exception(Constants.ERROR_THERE_IS_NOT_USER);
			}

		}

		public ICollection<ResetQuestion> GetQuestions()
		{
			List<ResetQuestion> listUserResetQuestion = null;
			List<ResetQuestion> listUserResetQuestionTake = new List<ResetQuestion>();

			using (UserServiceClient srv = new UserServiceClient())
			{
				listUserResetQuestion = srv.GetQuestions().ToList();

				int[] numeros = new int[listUserResetQuestion.Count];

				Random r = new Random();

				for (int i = 0; i < listUserResetQuestion.Count; i++)
				{
					numeros[i] = r.Next(0, listUserResetQuestion.Count);

					if (i > 0)    // a partir del segundo numero que genera empezara a comparar que no se repita
					{
						for (int x = 0; x < 50; x++)  //comprobara que no se repita por 50 veces
						{

							for (int j = 0; j < i; j++)
							{
								if (numeros[i] == numeros[j])
								{
									numeros[i] = r.Next(0, listUserResetQuestion.Count);
								}
							}
						}
					}

				}
				if (listUserResetQuestion.Count > 0)
				{
					if (pResetQuestionsPoolNumber <= listUserResetQuestion.Count)
					{
						if (listUserResetQuestion.Count > 0)
							for (int i = 0; i < pResetQuestionsPoolNumber; i++)
							{
								listUserResetQuestionTake.Add(listUserResetQuestion[numeros[i]]);
							}


						if (listUserResetQuestionTake.Count() > 0)
							return listUserResetQuestionTake;
						else
							throw new Exception(Constants.ERROR_QUESTION_NOT_CONFIGURED_BD);
					}
					else
					{
						throw new Exception(Constants.ERROR_CONFIG_RESET_QUESTION_NUMBRER);
					}
				}
				else
				{
					throw new Exception(Constants.ERROR_QUESTION_NOT_CONFIGURED_BD);
				}
			}

		}

		public void InsertQuestionUser(string Answer, int QuestionId, string userName)
		{
			UserResetQuestion _resetQuestion = new UserResetQuestion();

			using (UserServiceClient srv = new UserServiceClient())
			{

				User _user = srv.GetUser(userName, true);
				_resetQuestion.User = srv.GetUser(userName, true);
				_resetQuestion.Answer = EncodePassword(Answer);
				_resetQuestion.Question = srv.GetQuestionsById(QuestionId);
				srv.InsertQuestionUser(_resetQuestion);
			}
		}

		public void DeleteAllAnswerByUser(string userName)
		{
			using (UserServiceClient srv = new UserServiceClient())
			{
				User _user = srv.GetUser(userName, true);
				srv.DeleteAllAnswerByUser(_user.Id);
			}
		}

		public void UpdateUserIsResetPassword(string username)
		{
			using (UserServiceClient srv = new UserServiceClient())
			{
				srv.UpdateUserIsResetPassword(username);
			}
		}

		public string[] GetRolesUser(string userName)
		{
			string[] userRoles;
			int iCountRoles = 0;
			User theUser;

			using (UserServiceClient srv = new UserServiceClient())
			{
				theUser = srv.GetUser(userName, false);
				userRoles = new string[theUser.Roles.Length];

				foreach (var item in theUser.Roles)
				{
					userRoles[iCountRoles] = item.Rolename;
				}

			}
			return userRoles;

		}

		public bool isRoleAllowed(string userName)
		{
			string[] allowedRoles = System.Configuration.ConfigurationManager.AppSettings["allowedRolesDT"].Split(new char[] { ',' });
			bool isAllowedAccess = true;
			string[] userRoles = GetRolesUser(userName);


			for (int i = 0; i < allowedRoles.Length; i++)
			{
				if (userRoles.Contains(allowedRoles[i]))
				{
					isAllowedAccess = true;
					break;

				}
				else
					isAllowedAccess = false;
			}

			return isAllowedAccess;
		}

		public static string GetVisitorIPAddress(bool GetLan = false)
		{
			string visitorIPAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

			if (String.IsNullOrEmpty(visitorIPAddress))
				visitorIPAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

			if (string.IsNullOrEmpty(visitorIPAddress))
				visitorIPAddress = HttpContext.Current.Request.UserHostAddress;

			if (string.IsNullOrEmpty(visitorIPAddress) || visitorIPAddress.Trim() == "::1")
			{
				GetLan = true;
				visitorIPAddress = string.Empty;
			}

			if (GetLan)
			{
				if (string.IsNullOrEmpty(visitorIPAddress))
				{
					//This is for Local(LAN) Connected ID Address
					string stringHostName = Dns.GetHostName();
					//Get Ip Host Entry
					IPHostEntry ipHostEntries = Dns.GetHostEntry(stringHostName);
					//Get Ip Address From The Ip Host Entry Address List
					IPAddress[] arrIpAddress = ipHostEntries.AddressList;

					try
					{
						visitorIPAddress = arrIpAddress[arrIpAddress.Length - 2].ToString();
					}
					catch
					{
						try
						{
							visitorIPAddress = arrIpAddress[0].ToString();
						}
						catch
						{
							try
							{
								arrIpAddress = Dns.GetHostAddresses(stringHostName);
								visitorIPAddress = arrIpAddress[0].ToString();
							}
							catch
							{
								visitorIPAddress = "127.0.0.1";
							}
						}
					}
				}
			}


			return visitorIPAddress;
		}

	}
}