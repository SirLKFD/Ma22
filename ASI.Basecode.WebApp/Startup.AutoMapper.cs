using AutoMapper;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.Extensions.DependencyInjection;

namespace ASI.Basecode.WebApp
{
    // AutoMapper configuration
    internal partial class StartupConfigurer
    {
        /// <summary>
        /// Configure auto mapper
        /// </summary>
        private void ConfigureAutoMapper()
        {
            var mapperConfiguration = new MapperConfiguration(config =>
            {
                config.AddProfile(new AutoMapperProfileConfiguration());
            });

            this._services.AddSingleton<IMapper>(sp => mapperConfiguration.CreateMapper());
        }

        private class AutoMapperProfileConfiguration : Profile
        {
            public AutoMapperProfileConfiguration()
            {
                CreateMap<UserViewModel, Account>();
                CreateMap<AdminCreateUserViewModel, Account>();
                CreateMap<UserEditViewModel, Account>();
                CreateMap<TrainingCategoryViewModel, TrainingCategory>();
                CreateMap<TrainingViewModel, Training>();
                CreateMap<UserProfileEditViewModel, Account>();
                
                CreateMap<ReviewViewModel, Review>()
                    .ForMember(dest => dest.ReviewId, opt => opt.Ignore())
                    .ForMember(dest => dest.CreatedTime, opt => opt.Ignore())
                    .ForMember(dest => dest.UpdatedTime, opt => opt.Ignore())
                    .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                    .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                    .ForMember(dest => dest.Account, opt => opt.Ignore())
                    .ForMember(dest => dest.Training, opt => opt.Ignore());

                CreateMap<Training, TrainingViewModel>()
                    .ForMember(dest => dest.AccountFirstName, opt => opt.MapFrom(src => src.Account.FirstName))
                    .ForMember(dest => dest.AccountLastName, opt => opt.MapFrom(src => src.Account.LastName));

                CreateMap<TopicViewModel, Topic>();
                CreateMap<TopicMediaViewModel, TopicMedium>();
                CreateMap<AuditLogViewModel, AuditLog>();
            }
        }
    }
}
