using AutoMapper;
using Rota.Application.DTOs;
using Rota.Domain.Entities;
using System.Text.Json;

namespace Rota.Application.Mappings
{
    public class DomainToDTOMappingProfile : Profile
    {
        public DomainToDTOMappingProfile()
        {
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.AgencyId, opt => opt.MapFrom(src => src.AgencyId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.AgencyName, opt => opt.MapFrom(src => src.Agency.CorporateName));

            CreateMap<UserDTO, User>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());

            CreateMap<UserRegisterDTO, User>()
                .ConstructUsing(src => new User(
                    0,
                    src.Name,
                    src.Email.ToLowerInvariant(),
                    null,
                    src.AgencyId,
                    src.Role));


            CreateMap<Agency, AgencyDTO>().ReverseMap();

            CreateMap<AgencyRegisterDTO, Agency>()
                .ConstructUsing(dto => new Agency(
                    dto.ExternalId,
                    dto.CorporateName,
                    dto.CNPJ,
                    dto.Address,
                    dto.City,
                    dto.State,
                    dto.AddressNumber,
                    dto.AddressComment,
                    dto.PhoneNumberOne,
                    dto.PhoneNumberTwo,
                    dto.Email
                ));
            CreateMap<AgencyJsonServerDTO, Agency>()
                .ConstructUsing(dto => new Agency(
                    dto.ExternalId,
                    dto.CorporateName,
                    dto.CNPJ,
                    dto.Address,
                    dto.City,
                    dto.State,
                    dto.AddressNumber,
                    dto.AddressComment,
                    dto.PhoneNumberOne,
                    dto.PhoneNumberTwo,
                    dto.Email
                ));
            CreateMap<InvoiceFile, InvoiceFileDTO>();
            CreateMap<Invoice, InvoiceDTO>()
                .ForMember(d => d.IsAnnual, o => o.MapFrom(s => s.IsAnnual))
                
                .ForMember(dest => dest.AgencyName, opt => opt.MapFrom(src => src.Agency.CorporateName))
                
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name));
            CreateMap<Transaction, TransactionDTO>()
                
                .ForMember(dest => dest.AgencyName, opt => opt.MapFrom(src => src.Agency.CorporateName))
                
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Name : "Sistema"))
                .ReverseMap();


            CreateMap<TransactionRegisterDTO, Transaction>()
                .ConstructUsing(dto => Transaction.CreateAutomatic(
                    dto.Type,
                    dto.Value,
                    dto.Date,
                    dto.TransactionIdApi,
                    dto.AgencyId
                ));


            CreateMap<Transaction, TransactionDTO>().ReverseMap();
            CreateMap<MonthlyCommission, MonthlyCommissionDTO>()
                .ForMember(dest => dest.AgencyName, opt => opt.MapFrom(src => src.Agency.CorporateName));

            CreateMap<CommissionRule, CommissionRuleDTO>().ReverseMap();
            CreateMap<CommissionRule, RuleCreateDTO>().ReverseMap();
            CreateMap<AuditLog, AuditLogDTO>().ReverseMap();
            CreateMap<AuditLog, AuditLogDTO>()
                .ForMember(dest => dest.OldValues, opt => opt.MapFrom(src => 
                    string.IsNullOrEmpty(src.OldValues) ? null : JsonSerializer.Deserialize<object>(src.OldValues, new JsonSerializerOptions())))
                
                .ForMember(dest => dest.NewValues, opt => opt.MapFrom(src => 
                    string.IsNullOrEmpty(src.NewValues) ? null : JsonSerializer.Deserialize<object>(src.NewValues, new JsonSerializerOptions())))
                
                .ForMember(dest => dest.AffectedColumns, opt => opt.MapFrom(src => 
                    string.IsNullOrEmpty(src.AffectedColumns) ? null : JsonSerializer.Deserialize<object>(src.AffectedColumns, new JsonSerializerOptions())));

        }
    }
}