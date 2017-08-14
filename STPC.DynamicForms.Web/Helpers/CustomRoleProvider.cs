using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Configuration.Provider;
using STPC.DynamicForms.Web.Common.Services.Users;

namespace STPC.DynamicForms.Web.Helpers
{
    public sealed class CustomRoleProvider: RoleProvider
    {
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            foreach (string rolename in roleNames)
            {
                if (!RoleExists(rolename))
                {
                    throw new ProviderException("Role name not found.");
                }
            }

            foreach (string username in usernames)
            {
                if (username.Contains(","))
                {
                    throw new ArgumentException("User names cannot contain commas.");
                }

                foreach (string rolename in roleNames)
                {
                    if (IsUserInRole(username, rolename))
                    {
                        throw new ProviderException("User is already in role.");
                    }
                }
            }
            using (var srv = new UserServiceClient())
            {
                srv.AddUsersToRoles(usernames, roleNames);
            }
        }

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

        public override void CreateRole(string roleName)
        {
            if (roleName.Contains(","))
            {
                throw new ArgumentException("Role names cannot contain commas.");
            }

            if (RoleExists(roleName))
            {
                throw new ProviderException("Role name already exists.");
            }
            using(var srv=new UserServiceClient())
            {
                srv.CreateRole(roleName);
            }
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            if (!RoleExists(roleName))
            {
                throw new ProviderException("Role does not exist.");
            }

            if (throwOnPopulatedRole && GetUsersInRole(roleName).Length > 0)
            {
                throw new ProviderException("Cannot delete a populated role.");
            }
            using (var srv = new UserServiceClient())
            {
                srv.DeleteRole(roleName);
            }
            return true;
        }

        //TODO: Evaluar poner esto en memoria/cache
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            using (var srv = new UserServiceClient())
            {
                return srv.FindUsersInRole(roleName, usernameToMatch);
            }
        }

        //TODO: Evaluar poner esto en memoria/cache
        public override string[] GetAllRoles()
        {
            using (var srv = new UserServiceClient())
            {
                return srv.GetAllRoles();
            }
        }

        //TODO: Evaluar poner esto en memoria/cache
        public override string[] GetRolesForUser(string username)
        {
            using (var srv = new UserServiceClient())
            {
                return srv.GetRolesForUser(username);
            }
        }

        //TODO: Evaluar poner esto en memoria/cache
        public override string[] GetUsersInRole(string roleName)
        {
            using (var srv = new UserServiceClient())
            {
                return srv.GetUsersInRole(roleName);
            }
        }

        //TODO: Evaluar poner esto en memoria/cache
        public override bool IsUserInRole(string username, string roleName)
        {
            using (var srv = new UserServiceClient())
            {
                return srv.IsUserInRole(username,roleName);
            }
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            foreach (string rolename in roleNames)
            {
                if (!RoleExists(rolename))
                {
                    throw new ProviderException("Role name not found.");
                }
            }

            foreach (string username in usernames)
            {
                foreach (string rolename in roleNames)
                {
                    if (!IsUserInRole(username, rolename))
                    {
                        throw new ProviderException("User is not in role.");
                    }
                }
            }
            using (var srv = new UserServiceClient())
            {
                srv.RemoveUsersFromRoles(usernames, roleNames);
            }
        }

        //TODO: Evaluar poner esto en memoria/cache
        public override bool RoleExists(string roleName)
        {
            using (UserServiceClient srv = new UserServiceClient())
            {
                return srv.RoleExists(roleName);
            }
        }
    }
}