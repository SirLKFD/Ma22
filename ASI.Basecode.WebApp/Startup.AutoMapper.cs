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

                CreateMap<Training, TrainingViewModel>()
                    .ForMember(dest => dest.AccountFirstName, opt => opt.MapFrom(src => src.Account.FirstName))
                    .ForMember(dest => dest.AccountLastName, opt => opt.MapFrom(src => src.Account.LastName));

                CreateMap<TopicViewModel, Topic>();
                CreateMap<TopicMediaViewModel, TopicMedium>();
            }
        }
    }
}
