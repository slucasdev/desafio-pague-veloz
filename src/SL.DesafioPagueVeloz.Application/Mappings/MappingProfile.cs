using AutoMapper;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Domain.Entities;

namespace SL.DesafioPagueVeloz.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Cliente → ClienteDTO
            CreateMap<Cliente, ClienteDTO>()
                .ForMember(dest => dest.Documento, opt => opt.MapFrom(src => src.Documento.Numero))
                .ForMember(dest => dest.TipoDocumento, opt => opt.MapFrom(src => src.Documento.Tipo.ToString()));

            // Conta → ContaDTO
            CreateMap<Conta, ContaDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // Transacao → TransacaoDTO
            CreateMap<Transacao, TransacaoDTO>()
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Tipo.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // Conta → SaldoDTO
            CreateMap<Conta, SaldoDTO>()
                .ForMember(dest => dest.NumeroConta, opt => opt.MapFrom(src => src.Numero))
                .ForMember(dest => dest.ConsultadoEm, opt => opt.Ignore()); // Setado manualmente no handler
        }
    }
}
