using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class ChauffeurProfile : Profile
    {
        public ChauffeurProfile()
        {
            // Entity to DTO mapping
            CreateMap<Chauffeur, ChauffeurDto>()
                .ForMember(d => d.FournisseurNom,
                    opt => opt.MapFrom(s => s.Fournisseur != null ? s.Fournisseur.Societe : null));

            // DTO to Entity mapping - ignore navigation properties to prevent creating new entities
            CreateMap<ChauffeurDto, Chauffeur>()
                .ForMember(d => d.Fournisseur, opt => opt.Ignore())
                .ForMember(d => d.Achats, opt => opt.Ignore());
        }
    }
}
