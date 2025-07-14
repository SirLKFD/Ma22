using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork) 
        {

        }

        public IQueryable<Account> GetUsers()
        {
            return this.GetDbSet<Account>();
        }

        public Account GetUserById(int id)
        {
            return this.GetDbSet<Account>().FirstOrDefault(x => x.Id == id);
        }

        public bool UserExists(string emailId)
        {
            return this.GetDbSet<Account>().Any(x => x.EmailId == emailId);
        }

        public bool UserExists(string emailId, int excludeId)
        {
            return this.GetDbSet<Account>().Any(x => x.EmailId == emailId && x.Id != excludeId);
        }

        public void AddUser(Account user)
        {
            this.GetDbSet<Account>().Add(user);
            UnitOfWork.SaveChanges();
        }

        public void UpdateUser(Account user)
        {
            var existingUser = this.GetDbSet<Account>().FirstOrDefault(x => x.Id == user.Id);
            if (existingUser != null)
            {
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.EmailId = user.EmailId;
                existingUser.Contact = user.Contact;
                existingUser.Birthdate = user.Birthdate;
                existingUser.Role = user.Role;
                existingUser.ProfilePicture = user.ProfilePicture;
                existingUser.UpdatedTime = DateTime.Now;
                existingUser.UpdatedBy = System.Environment.UserName;

                // Only update password if provided
                if (!string.IsNullOrEmpty(user.Password))
                {
                    existingUser.Password = user.Password;
                }

                UnitOfWork.SaveChanges();
            }
        }
        public void DeleteUser(Account user)
        {
            var existingUser = this.GetDbSet<Account>().FirstOrDefault(x => x.Id == user.Id);
            if (existingUser == null)
                return;

            // Use transaction for safety
            using (var transaction = Context.Database.BeginTransaction())
            {
                if (existingUser.Role == 2) // SuperAdmin
                {
                    // Delete all content created by this superadmin
                    var topics = Context.Topics.Where(t => t.AccountId == user.Id).ToList();
                    Context.Topics.RemoveRange(topics);

                    var trainings = Context.Trainings.Where(t => t.AccountId == user.Id).ToList();
                    Context.Trainings.RemoveRange(trainings);

                    var categories = Context.TrainingCategories.Where(c => c.AccountId == user.Id).ToList();
                    Context.TrainingCategories.RemoveRange(categories);

                    var topicMedia = Context.TopicMedia.Where(m => m.AccountId == user.Id).ToList();
                    Context.TopicMedia.RemoveRange(topicMedia);
                }
                else if (existingUser.Role == 1) 
                {
                    // Delete reviews and enrollments
                    var reviews = Context.Reviews.Where(r => r.AccountId == user.Id).ToList();
                    Context.Reviews.RemoveRange(reviews);

                    var enrollments = Context.Enrollments.Where(e => e.AccountId == user.Id).ToList();
                    Context.Enrollments.RemoveRange(enrollments);
                }

                // Finally, delete the user
                this.GetDbSet<Account>().Remove(existingUser);
                UnitOfWork.SaveChanges();
                transaction.Commit();
            }
        }

    }
}
