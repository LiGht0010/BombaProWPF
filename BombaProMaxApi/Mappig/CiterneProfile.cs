using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class CiterneProfile : Profile
    {
        public CiterneProfile()
        {
            CreateMap<Citerne, CiterneDto>()
                .ForMember(d => d.FournisseurNom,
                    opt => opt.MapFrom(s => s.Fournisseur != null ? s.Fournisseur.Nom : null))
                .ReverseMap();
        }
    }
}
