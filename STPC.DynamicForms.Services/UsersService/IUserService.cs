using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using STPC.DynamicForms.Web.Models;
using System.ServiceModel.Activation;
using System.Web.Security;
using System.Runtime.Serialization;

[assembly: ContractNamespace("http://STPC.LiSim.Abc", ClrNamespace = "STPC.DynamicForms.Services.Users")]
namespace STPC.DynamicForms.Services.Users
{
	[ServiceContract]
	public interface IUserService
	{

		[OperationContract]
		MembershipCreateStatus CreateAccount(User user);

		[OperationContract]
		string GetUserNameByEmail(string email);

		[OperationContract]
		User GetUser(string username, bool userIsOnline);

		[OperationContract]
		List<User> GetAllUsers(int pageIndex, int pageSize, out int totalRecords);

		[OperationContract]
		List<User> GetAllUsersIndicatora();

		[OperationContract]
		List<User> GetAllUsersTotalPages(int pageIndex, int pageSize, out int totalRecords, out int totalPages);

		[OperationContract]
		ICollection<User> GetAllUsersCompleteName(int pageIndex, int pageSize, out int totalRecords);

		[OperationContract]
		bool RoleExists(string rolename);

		[OperationContract]
		bool IsUserInRole(string username, string rolename);

		[OperationContract]
		void UpdateUser(User user);

		[OperationContract]
		void UnlockUser(string username);

		[OperationContract]
		void LockUser(string username);

		[OperationContract]
		void ActivateUser(string username);

		[OperationContract]
		void ResetPassword(string username, string newpasswordencoded);

		[OperationContract]
		void UpdateFiedldIsResetPassword(string username, bool state);

		[OperationContract]
		void AddUsersToRoles(string[] usernames, string[] roleNames);

		[OperationContract]
		void UpdateUserLastLoginDate(string username);

		[OperationContract]
		void CreateRole(string roleName);

		
		[OperationContract]
		void DeleteRole(string roleName);

		[OperationContract]
		string[] FindUsersInRole(string roleName, string usernameToMatch);

		[OperationContract]
		string[] GetAllRoles();
		
		[OperationContract]
		string[] GetRolesForUser(string username);
		[OperationContract]
		string[] GetUsersInRole(string roleName);
		[OperationContract]
		void RemoveUsersFromRoles(string[] usernames, string[] roleNames);

		[OperationContract]
		string[] GetListForbiddenPassword();

		[OperationContract]
		void AddHistoryPassWord(string user, string passWord);

		[OperationContract]
		bool ValidateLastPassWordHistroy(string userName, string newPassWord);

		[OperationContract]
		int UpdatePassWordAttemptCount(string userName);

		[OperationContract]
		ICollection<UserResetQuestion> GetQuestionsUser(Guid UserId);

		[OperationContract]
		ICollection<ResetQuestion> GetQuestions();

		[OperationContract]
		ResetQuestion GetQuestionsById(int id);

		[OperationContract]
		void InsertQuestionUser(UserResetQuestion resetQuestion);

		[OperationContract]
		void DeleteAllAnswerByUser(Guid id);

		[OperationContract]
		void UpdateUserIsResetPassword(string username);

		[OperationContract]
		User GetBasicInfoUser(string username);

		[OperationContract]
		void UpdateUserLastLoginDateAndIp(User username, string ipAdrress);

		[OperationContract]
		List<User> GetAllUsersFilter(int pageIndex, int pageSize, string searchText, out int totalRecords, out int totalPages);

		[OperationContract]
		ICollection<User> GetAllUsersByAplicationName(int pageIndex, int pageSize, int aplicationNameId, out int totalRecords);

	}
}
