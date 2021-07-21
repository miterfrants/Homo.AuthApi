using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Homo.Core.Helpers;

namespace Homo.AuthApi
{
    public class UserDataservice
    {
        public static User GetOne(DBContext dbContext, long id, bool asNoTracking = false)
        {
            DbSet<User> dbSet = dbContext.User;
            IQueryable<User> queryablUser = dbSet;
            if (asNoTracking)
            {
                queryablUser = queryablUser.AsNoTracking();
            }
            return queryablUser.Where(x => x.Id == id && x.DeletedAt == null).FirstOrDefault();
        }

        public static User GetOneByEmail(DBContext dbContext, string email, bool asNoTracking = false)
        {
            DbSet<User> dbSet = dbContext.User;
            IQueryable<User> queryablUser = dbSet;
            if (asNoTracking)
            {
                queryablUser = queryablUser.AsNoTracking();
            }
            return queryablUser.Where(x => x.Email == email && x.DeletedAt == null).FirstOrDefault();
        }

        public static User GetOneBySocialMediaSub(DBContext dbContext, SocialMediaProvider provider, string sub)
        {
            return dbContext.User.Where(
                x =>
                (
                    (provider == SocialMediaProvider.FACEBOOK && x.FbSub == sub)
                    || (provider == SocialMediaProvider.GOOGLE && x.GoogleSub == sub)
                    || (provider == SocialMediaProvider.LINE && x.LineSub == sub)
                )
                && x.DeletedAt == null
            ).FirstOrDefault();
        }

        public static User SignUpWithSocialMedia(DBContext dbContext, SocialMediaProvider provider, string sub, string email, string fullname, string picture, string firstName, string lastName, DateTime? birthday = null)
        {
            User newUser = new User()
            {
                Email = email,
                CreatedAt = DateTime.Now,
                Username = fullname,
                FirstName = firstName,
                LastName = lastName,
                Profile = picture,
                Birthday = birthday,
            };
            if (provider == SocialMediaProvider.FACEBOOK)
            {
                newUser.FbSub = sub;
            }
            else if (provider == SocialMediaProvider.GOOGLE)
            {
                newUser.GoogleSub = sub;
            }
            else if (provider == SocialMediaProvider.LINE)
            {
                newUser.LineSub = sub;
            }
            try
            {
                dbContext.User.Add(newUser);
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return newUser;
        }

        public static User SignUp(DBContext dbContext, string email, string password, string firstName, string lastName, string salt, string hash, DateTime? birthday = null)
        {
            User newUser = new User()
            {
                Email = email,
                Salt = salt,
                Hash = hash,
                CreatedAt = DateTime.Now,
                Status = true,
                FirstName = firstName,
                LastName = lastName,
                Birthday = birthday,
                DeletedAt = null
            };
            try
            {
                dbContext.User.Add(newUser);
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return newUser;
        }

        public static void SetUserToForgotPasswordState(DBContext dbContext, long userId)
        {
            var record = new User() { Id = userId };
            dbContext.User.Attach(record);
            record.ForgotPasswordAt = DateTime.Now;
            dbContext.SaveChanges();
        }

        public static void ResetPassword(DBContext dbContext, long userId, string salt, string hash)
        {
            var record = new User() { Id = userId };
            dbContext.User.Attach(record);
            record.Salt = salt;
            record.Hash = hash;
            dbContext.SaveChanges();
        }

        public static void RemoveFbSub(DBContext dbContext, string fbSub, string confirmCode)
        {
            User user = dbContext.User.Where(x => x.FbSub == fbSub && x.DeletedAt == null).FirstOrDefault();
            user.FbSub = null;
            user.FbSubDeletionConfirmCode = confirmCode;
            user.EditedBy = 0;
            user.UpdatedAt = DateTime.Now;
            dbContext.SaveChanges();
        }

        public static User GetOneByConfirmCode(string confirmCode, DBContext dbContext)
        {
            return dbContext.User.Where(x => x.FbSubDeletionConfirmCode == confirmCode && x.DeletedAt == null).FirstOrDefault();
        }

    }
}