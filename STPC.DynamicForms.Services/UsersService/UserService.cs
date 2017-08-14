using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Web.Security;
using STPC.DynamicForms.Web.Models;
using System.Runtime.Serialization;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.Configuration;

namespace STPC.DynamicForms.Services.Users
{
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
	public class UserService : IUserService
	{
		const string applicationName = "LiSimAbc";

		STPC_FormsFormEntities ctx = new STPC_FormsFormEntities();

		public MembershipCreateStatus CreateAccount(User user)
		{
			if (user == null) return MembershipCreateStatus.InvalidUserName;
			//user.ApplicationName = applicationName;
			user.Id = Guid.NewGuid();
			user.Hierarchy = ctx.Hierarchies.Find(user.Hierarchy.Id);
			user.CreationDate = user.LastActivityDate = user.LastLockedOutDate = user.LastLoginDate = user.FailedPasswordAttemptWindowStart = user.FailedPasswordAnswerAttemptWindowStart = user.LastPasswordChangedDate = DateTime.Now;
			user.Token = user.Token;
			ctx.Users.Add(user);
			ctx.SaveChanges();
			return MembershipCreateStatus.Success;
		}

		public string GetUserNameByEmail(string email)
		{
			//var user = ctx.Users.Where(a => a.AplicationNameId == applicationName).Where(m => m.Email == email).FirstOrDefault();
			var user = ctx.Users.Where(m => m.Email == email).FirstOrDefault();
			if (user == null) return null;
			return user.Username;
		}

		public User GetUser(string username, bool userIsOnline)
		{

			//var user = ctx.Users.Include("Roles").Include("Hierarchy").Where(a => a.AplicationNameId == applicationName).Where(u => u.Username == username).FirstOrDefault();
			 var user = ctx.Users.Include("Roles").Include("Hierarchy").Where(u => u.Username == username).FirstOrDefault();
			if (user != null)
				if (userIsOnline)
				{
					user.LastActivityDate = DateTime.Now;
					ctx.Entry(user).State = System.Data.EntityState.Modified;
					ctx.SaveChanges();
				}
			return user;

		}

		public User GetBasicInfoUser(string username)
		{
			var user = ctx.Users.Where(u => u.Username == username).FirstOrDefault();
			if (user != null)
				return user;
			else
				return null;
		}

		public void UpdateUserLastLoginDate(string username)
		{
			var user = GetUser(username, true);
			if (user != null)
			{
				user.LastLoginDate = DateTime.Now;
				user.FailedPasswordAttemptCount = 0;
				ctx.Entry(user).State = System.Data.EntityState.Modified;
				ctx.SaveChanges();
			}
		}

		public void UpdateUserLastLoginDateAndIp(User users, string ipAdrress)
		{
			var user = GetUser(users.Username, true);
			if (user != null)
			{
				user.LastLoginDate = DateTime.Now;
				user.FailedPasswordAttemptCount = 0;
				user.WorkStation = ipAdrress;
				user.IsOnLine = true;
				ctx.Entry(user).State = System.Data.EntityState.Modified;
				ctx.SaveChanges();
			}
		}

		public bool RoleExists(string roleName)
		{
			var therole = GetRole(roleName);
			return (therole != null);
		}


		public bool IsUserInRole(string username, string rolename)
		{
			var theUser = GetUser(username);
			if (theUser == null) return false;
			var theRoles = theUser.Roles;
			if (theRoles == null) return false;
			return (theRoles.Where(n => n.Rolename == rolename).Count() > 0);
		}

		public void AddUsersToRoles(string[] usernames, string[] roleNames)
		{
			foreach (var roleName in roleNames)
			{
				var therole = GetRole(roleName);

				foreach (var username in usernames)
				{
					var theuser = GetUser(username);
					if (therole.UsersInRole == null) therole.UsersInRole = new List<User>();
					therole.UsersInRole.Add(theuser);
				}
				ctx.Entry(therole).State = System.Data.EntityState.Modified;
			}
			ctx.SaveChanges();
		}

		public void CreateRole(string roleName)
		{
			//ctx.Roles.Add(new Role { Rolename = roleName, ApplicationName = applicationName });
			ctx.SaveChanges();
		}

		public void DeleteRole(string roleName)
		{
			var therole = GetRole(roleName);
			ctx.Entry(therole).State = System.Data.EntityState.Deleted;
			ctx.SaveChanges();
		}

		public string[] FindUsersInRole(string roleName, string usernameToMatch)
		{
			var therole = GetRole(roleName);
			return therole.UsersInRole.Where(un => un.Username.Contains(usernameToMatch)).Select(n => n.Username).ToArray();
		}

		public string[] GetAllRoles()
		{
			//return ctx.Roles.Where(a => a.ApplicationName == applicationName).Select(n => n.Rolename).ToArray();
			return ctx.Roles.Select(n => n.Rolename).ToArray();
		}

		public string[] GetRolesForUser(string username)
		{
			var theuser = GetUser(username);
			if (theuser.Roles == null) return new string[0];
			var theroles = theuser.Roles;
			if (theroles == null) return new string[0];
			return theuser.Roles.Select(n => n.Rolename).ToArray();
		}

		public string[] GetUsersInRole(string roleName)
		{
			var therole = GetRole(roleName);
			return therole.UsersInRole.Select(n => n.Username).ToArray();
		}

		public void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
		{
			foreach (var roleName in roleNames)
			{
				var therole = GetRole(roleName);
				foreach (var username in usernames)
				{
					var theuser = GetUser(username);
					therole.UsersInRole.Remove(theuser);
				}
				ctx.Entry(therole).State = System.Data.EntityState.Modified;
			}
			ctx.SaveChanges();
		}

		public User GetUser(string username)
		{

			return GetUser(username, false);
		}

		public Role GetRole(string roleName)
		{
			//return ctx.Roles.Where(a => a.ApplicationName == applicationName).Where(r => r.Rolename == roleName).SingleOrDefault();
			return ctx.Roles.Where(r => r.Rolename == roleName).SingleOrDefault();
		}

		public List<User> GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
		{
			List<User> listUser = new List<User>();
			totalRecords = ctx.Users.Count();
			if (pageIndex == 0) pageIndex = 1;
			int totalPages = (totalRecords / pageSize) + 1;
			if (pageIndex > totalPages) pageIndex = totalPages;
			listUser = ctx.Users.Include("Hierarchy").Include("Roles").OrderBy(n => n.Username).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

			return listUser;
		}

		public List<User> GetAllUsersIndicatora()
		{
			List<User> listUser = new List<User>();

			listUser = ctx.Users.OrderBy(n => n.Username).ToList();

			return listUser;
		}

		public List<User> GetAllUsersTotalPages(int pageIndex, int pageSize, out int totalRecords, out int totalPages)
		{
			List<User> listUser = new List<User>();
			totalRecords = ctx.Users.Count();
			if (pageIndex == 0) pageIndex = 1;
			totalPages = (totalRecords / pageSize) + 1;
			if (pageIndex > totalPages) pageIndex = totalPages;
			listUser = ctx.Users.Include("Hierarchy").Include("Roles").OrderBy(n => n.Username).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

			return listUser;
		}

		public List<User> GetAllUsersFilter(int pageIndex, int pageSize, string searchText, out int totalRecords, out int totalPages)
		{      

			List<User> listUser = new List<User>();
			totalRecords = ctx.Users.Count();
			if (pageIndex == 0) pageIndex = 1;
			totalPages = (totalRecords / pageSize) + 1;
			if (pageIndex > totalPages) pageIndex = totalPages;

      if (!String.IsNullOrEmpty(searchText))
      {
        listUser = ctx.Users.Include("Hierarchy").Include("Roles").Where(u => u.Username.Contains(searchText)
          || u.GivenName.Contains(searchText)
          || u.LastName.Contains(searchText)
          || u.Email.Contains(searchText)
          || u.Hierarchy.Name.Contains(searchText)
          || u.Roles.Any(r => r.Rolename == searchText)).OrderBy(n => n.Username).ToList();
        totalRecords = listUser.Count;
        totalPages = (totalRecords / pageSize) + 1;
        var listUserResult = listUser.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        return listUserResult;
      }
      else
      {
        listUser = ctx.Users.Include("Hierarchy").Include("Roles").OrderBy(n => n.Username).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        totalRecords = listUser.Count;
        totalPages = (totalRecords / pageSize) + 1;
        return listUser;
      }


		}


		public ICollection<User> GetAllUsersCompleteName(int pageIndex, int pageSize, out int totalRecords)
		{
			List<User> listUser = new List<User>();
			totalRecords = ctx.Users.Count();
			if (pageIndex == 0) pageIndex = 1;
			int totalPages = (totalRecords / pageSize) + 1;
			if (pageIndex > totalPages) pageIndex = totalPages;
			listUser = ctx.Users.Include("Hierarchy").OrderBy(n => n.GivenName).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

			foreach (var item in listUser)
			{
				item.GivenName = item.GivenName + " " + item.LastName;
			}
			return listUser;
		}
		public ICollection<User> GetAllUsersByAplicationName(int pageIndex, int pageSize, int aplicationNameId, out int totalRecords)
		{
			List<User> listUser = new List<User>();
			totalRecords = ctx.Users.Count();
			if (pageIndex == 0) pageIndex = 1;
			int totalPages = (totalRecords / pageSize) + 1;
			if (pageIndex > totalPages) pageIndex = totalPages;
			listUser = ctx.Users.Include("Hierarchy").Where(e => e.AplicationNameId == aplicationNameId).OrderBy(n => n.GivenName).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

			foreach (var item in listUser)
			{
				item.GivenName = item.GivenName + " " + item.LastName;
			}
			return listUser;
		}


		public void UpdateUser(User user)
		{
			//TODO: Si hay auditoria debe guardarse valor anterior, valor nuevo, fecha y usuario de cambio
			var theuser = ctx.Users.Find(user.Id);
			if (theuser != null)
			{
				var thehierarchy = ctx.Hierarchies.Find(user.Hierarchy.Id);
				theuser.AplicationNameId = user.AplicationNameId;
				theuser.Username = user.Username;
				theuser.GivenName = user.GivenName;
				theuser.Email = user.Email;
				theuser.Hierarchy = thehierarchy;
				theuser.LastName = user.LastName;
				theuser.Phone_LandLine = user.Phone_LandLine;
				theuser.Phone_Mobile = user.Phone_Mobile;
				theuser.Vacations_Start = user.Vacations_Start;
				theuser.Vacations_End = user.Vacations_End;
				theuser.IsApproved = user.IsApproved;
				theuser.Address = user.Address;
				theuser.IsOnLine = user.IsOnLine;
				ctx.Entry(theuser).State = System.Data.EntityState.Modified;

				ctx.SaveChanges();
			}
		}

		public void UnlockUser(string username)
		{
			//TODO: Si hay auditoria debe guardarse valor anterior, valor nuevo, fecha y usuario de cambio
			var theuser = GetUser(username);
			theuser.IsLockedOut = false;
			theuser.LastLockedOutDate = DateTime.Now;
			ctx.Entry(theuser).State = System.Data.EntityState.Modified;
			ctx.SaveChanges();
		}

		public void LockUser(string username)
		{             //TODO: Si hay auditoria debe guardarse valor anterior, valor nuevo, fecha y usuario de cambio
			var theuser = GetUser(username);
			theuser.IsLockedOut = true;
			ctx.Entry(theuser).State = System.Data.EntityState.Modified;
			ctx.SaveChanges();
		}

		public void ActivateUser(string username)
		{             //TODO: Si hay auditoria debe guardarse valor anterior, valor nuevo, fecha y usuario de cambio
			var theuser = GetUser(username);
			theuser.IsApproved = true;
			ctx.Entry(theuser).State = System.Data.EntityState.Modified;
			ctx.SaveChanges();
		}

		public void ResetPassword(string username, string newpasswordencoded)
		{
			//TODO: Si hay auditoria debe guardarse valor anterior, valor nuevo, fecha y usuario de cambio
			var theuser = GetUser(username);
			theuser.Password = newpasswordencoded;
			theuser.IsResetPassword = false;
			theuser.IsApproved = true;
      theuser.FailedPasswordAttemptCount = 0;
			theuser.LastPasswordChangedDate = DateTime.Now;
			ctx.Entry(theuser).State = System.Data.EntityState.Modified;
			ctx.SaveChanges();
		}

		public void UpdateFiedldIsResetPassword(string username, bool state)
		{
			//TODO: Si hay auditoria debe guardarse valor anterior, valor nuevo, fecha y usuario de cambio
			var theuser = GetUser(username);
			theuser.IsResetPassword = state;
			ctx.Entry(theuser).State = System.Data.EntityState.Modified;
			ctx.SaveChanges();
		}

		//[AspNetCacheProfile("CacheFor6000Seconds")]
		public string[] GetListForbiddenPassword()
		{

			var list = (from db in ctx.ForbiddenPasswords select db);

			return list.Select(e => e.ForbiddenText).ToArray();
		}

		public void AddHistoryPassWord(string user, string passWord)
		{
			PasswordHistory _passwordHistory = new PasswordHistory();
			_passwordHistory.LastModified = DateTime.Now;
			_passwordHistory.LastModifiedBy = user;
			_passwordHistory.Password = passWord;

			ctx.PasswordHistory.Add(_passwordHistory);
			ctx.SaveChanges();
		}

		public bool ValidateLastPassWordHistroy(string userName, string newPassWord)
		{
			string LastPassWord = System.Configuration.ConfigurationManager.AppSettings["NumLastPassword"];

			var listHistory = ctx.PasswordHistory.Where(e => e.LastModifiedBy == userName).
				OrderByDescending(e => e.LastModified).Select(e => e.Password).Take(Convert.ToInt32(LastPassWord)).ToArray();

			foreach (var item in listHistory)
			{
				if (item == newPassWord)
					return false;
			}

			return true;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="userName">Nombre del usuario</param>
		/// <param name="isCorrectLogIn">Indica si el login fue correcto, para dejar el contador en 0</param>
		public int UpdatePassWordAttemptCount(string userName)
		{
			var theuser = GetUser(userName);

			int TimeDelayErrorLogin = Convert.ToInt32(ConfigurationManager.AppSettings["LoginErrorSlicingTime"]);

			theuser.FailedPasswordAttemptCount += 1;


			ctx.Entry(theuser).State = System.Data.EntityState.Modified;
			ctx.SaveChanges();
			return theuser.FailedPasswordAttemptCount;
		}


		/// <summary>
		/// Retorna las preguntas configuradas por el usuario
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>
		public ICollection<UserResetQuestion> GetQuestionsUser(Guid UserId)
		{
			return ctx.UserResetQuestions.Include("Question").Where(rq => rq.User.Id == UserId).ToList();
		}

		public ICollection<ResetQuestion> GetQuestions()
		{
			return ctx.ResetQuestions.Where(state=>state.IsActive).ToList();
		}

		public ResetQuestion GetQuestionsById(int id)
		{
			return ctx.ResetQuestions.Where(e => e.Id == id).FirstOrDefault();
		}


		public void InsertQuestionUser(UserResetQuestion userResetQuestion)
		{

			//UserResetQuestion _UserResetQuestion = ctx.UserResetQuestions.Where(c => c.User.Id == userResetQuestion.User.Id).Single();
			//ctx.UserResetQuestions.Remove(_UserResetQuestion);
			//ctx.SaveChanges();



			ctx.Entry(userResetQuestion.User).State = System.Data.EntityState.Unchanged;
			ctx.Entry(userResetQuestion.Question).State = System.Data.EntityState.Unchanged;
			ctx.UserResetQuestions.Add(userResetQuestion);
			ctx.SaveChanges();
		}


		public void DeleteAllAnswerByUser(Guid id)
		{
			List<UserResetQuestion> _UserResetQuestion = ctx.UserResetQuestions.Where(c => c.User.Id == id).ToList();

			foreach (var item in _UserResetQuestion)
			{
				ctx.UserResetQuestions.Remove(item);
			}
			ctx.SaveChanges();
		}


		public void UpdateUserIsResetPassword(string username)
		{

			User user = this.GetUser(username);

			var theuser = ctx.Users.Find(user.Id);

			if (theuser != null)
			{

				theuser.IsResetPassword = false;
				ctx.Entry(theuser).State = System.Data.EntityState.Modified;

				ctx.SaveChanges();
			}
		}


		//public bool ValidatePermissionByObject(string tableName, string objectName, string Permissions, string role)
		//{
		//	ObjectPermissions _ObjectPermissions = ctx.ObjectPermissions.Where(op => op.TableName == tableName && op.ObjectName == objectName
		//		&& op.Role.Rolename.ToString() == role).FirstOrDefault();

		//	return true;
		//}
	}
}
