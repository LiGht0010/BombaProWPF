using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class ChauffeurProfile : Profile
    {
        public ChauffeurProfile()
        {
            CreateMap<Chauffeur, ChauffeurDto>()
                .ForMember(d => d.FournisseurNom,
                    opt => opt.MapFrom(s => s.Fournisseur != null ? s.Fournisseur.Nom : null))
                .ReverseMap();
        }
    }
}
