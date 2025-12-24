using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class CamionProfile : Profile
    {
        public CamionProfile()
        {
            CreateMap<Camion, CamionDto>()
                .ForMember(d => d.FournisseurNom,
                    opt => opt.MapFrom(s => s.Fournisseur != null ? s.Fournisseur.Nom : null))
                .ForMember(d => d.CiterneNumero,
                    opt => opt.MapFrom(s => s.Citerne != null ? s.Citerne.MatriculeCiterne : null))
                .ReverseMap();
        }
    }
}
