using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using IdentityApi.ViewModels;
using IdentityCore.Models;

namespace IdentityApi.StartupConfig
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            MapUsers();
            MapUserGroups();
            MapUserConfiguration();
        }


        private void MapUsers()
        {
            CreateMap<PortalUserPostView, IdentityMSUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.UserGroupIdentityMSUserRelations, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
                .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordToken, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordTokenExpiryTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastAccess, opt => opt.Ignore());

            CreateMap<PortalUserConfigurationView, IdentityMSUser>()
                .ForMember(dest => dest.PasswordConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
                .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordToken, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordTokenExpiryTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastAccess, opt => opt.Ignore());

            CreateMap<IdentityMSUser, PortalUserGetView>();

            CreateMap<IdentityMSUser, BasicUserGetView>();
            CreateMap<IdentityMSUser, PortalUserConfigurationView>();

            CreateMap<UserGroupIdentityMSUserRelation, UserGroupIdentityMSUserRelationViewModelDetail>()
                .ForMember(dest=> dest.Email, opt => opt.MapFrom(src => src.IdentityMsUser.Email))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.IdentityMsUser.UserName))
                .ReverseMap();
            CreateMap<UserGroupIdentityMSUserRelation, UserGroupIdentityMSUserRelationViewModel>()
                .ReverseMap();
        }

        private void MapUserGroups()
        {
            CreateMap<UserGroup, UserGroupViewModel>()
                .ReverseMap();
            CreateMap<UserGroup, UserGroupGetViewModelSummary>();
            CreateMap<UserGroup, UserGroupGetViewModel>();
            CreateMap<UserGroup, UserGroupGetViewModelDetail>();

            CreateMap<UserGroupPostViewModel, UserGroup>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserGroupIdentityMSUserRelations, opt => opt.Ignore());

            CreateMap<UserGroupPutViewModel, UserGroup>()
                .ForMember(dest => dest.UserGroupIdentityMSUserRelations, opt => opt.Ignore());

        }

        private void MapUserConfiguration()
        {
            CreateMap<UserConfigurationPostView, UserConfiguration>()
                .ForMember(dest => dest.Errors, opt => opt.Ignore());

            CreateMap<ChangedUserConfigurationPostView, ChangedUserConfiguration>()
                .ForMember(dest => dest.Errors, opt => opt.Ignore());

            CreateMap<UserConfiguration, UserConfigurationGetView>();
            CreateMap<ChangedUserConfiguration, ChangedUserConfigurationGetView>();
        }

    }
}
