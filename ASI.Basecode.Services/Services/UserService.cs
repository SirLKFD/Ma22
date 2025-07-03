using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public LoginResult AuthenticateUser(string emailId, string password, ref Account user)
        {
            user = new Account();
            var passwordKey = PasswordManager.EncryptPassword(password);
            user = _repository.GetUsers().Where(x => x.EmailId == emailId &&
                                                     x.Password == passwordKey).FirstOrDefault();

            return user != null ? LoginResult.Success : LoginResult.Failed;
        }

        public Account GetUserByEmail(string emailId)
        {
            return _repository.GetUsers().FirstOrDefault(x => x.EmailId == emailId);
        }

        public UserProfileViewModel GetUserByEmailId(string emailId){
            var user = _repository.GetUsers().FirstOrDefault(x => x.EmailId == emailId);
            if (user == null) return null;

            return new UserProfileViewModel
            {
                Id = user.Id,
                EmailId = user.EmailId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Contact = user.Contact,
                Birthdate = user.Birthdate,
                ProfilePicture = user.ProfilePicture,
                CreatedTime = user.CreatedTime
            };
        }



        public List<UserListViewModel> GetAllUsers()
        {
            var users = _repository.GetUsers()
                .Select(u => new UserListViewModel
                {
                    Id = u.Id,
                    EmailId = u.EmailId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Contact = u.Contact,
                    Birthdate = u.Birthdate,
                    Role = u.Role,
                    ProfilePicture = u.ProfilePicture,
                    CreatedTime = u.CreatedTime
                }).ToList();

            return users;
        }

        public UserEditViewModel GetUserById(int id)
        {
            var user = _repository.GetUserById(id);
            if (user == null)
                return null;

            return new UserEditViewModel
            {
                Id = user.Id,
                EmailId = user.EmailId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Contact = user.Contact,
                Birthdate = user.Birthdate,
                Role = user.Role,
                ProfilePicture = user.ProfilePicture,
                CreatedTime = user.CreatedTime,
                CreatedBy = user.CreatedBy
            };
        }

        public void AddUser(UserViewModel model)
        {
            var user = new Account();
            if (!_repository.UserExists(model.EmailId))
            {
                _mapper.Map(model, user);
                user.Password = PasswordManager.EncryptPassword(model.Password);
                user.CreatedTime = DateTime.Now;
                user.UpdatedTime = DateTime.Now;
                user.CreatedBy = System.Environment.UserName;
                user.UpdatedBy = System.Environment.UserName;

                _repository.AddUser(user);
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserExists);
            }
        }

         public void AddUser(AdminCreateUserViewModel model)
        {
            var user = new Account();
            if (!_repository.UserExists(model.EmailId))
            {
                _mapper.Map(model, user);
                user.Password = PasswordManager.EncryptPassword(model.Password);
                user.CreatedTime = DateTime.Now;
                user.UpdatedTime = DateTime.Now;
                user.CreatedBy = System.Environment.UserName;
                user.UpdatedBy = System.Environment.UserName;

                _repository.AddUser(user);
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserExists);
            }
        }

        public void UpdateUser(UserEditViewModel model)
        {
            var user = new Account();
            
            // Check if email exists for other users (excluding current user)
            if (_repository.UserExists(model.EmailId, model.Id))
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserExists);
            }

            _mapper.Map(model, user);
            
            // Only encrypt password if provided
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.Password = PasswordManager.EncryptPassword(model.Password);
            }

            _repository.UpdateUser(user);
        }

         public void UpdateUser(UserProfileEditViewModel model)
        {
            var user = new Account();
            
            // Check if email exists for other users (excluding current user)
            if (_repository.UserExists(model.EmailId, model.Id))
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserExists);
            }
            user.Role = 1;
            user.UpdatedTime = DateTime.Now;
            _mapper.Map(model, user);
            
            // Only encrypt password if provided
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.Password = PasswordManager.EncryptPassword(model.Password);
            }

            _repository.UpdateUser(user);
        }

        public void DeleteUser(int userId)
        {
            var user = _repository.GetUserById(userId);
            if (user != null)
            {
                _repository.DeleteUser(user);
            }
        }
    }
}
