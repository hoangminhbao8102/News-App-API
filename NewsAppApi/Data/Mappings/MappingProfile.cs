using AutoMapper;
using NewsAppApi.Models.DTOs;
using NewsAppApi.Models.Entities;

namespace NewsAppApi.Data.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User
            CreateMap<User, UserDto>();

            // Category
            CreateMap<Category, CategoryDto>();

            // Tag
            CreateMap<Tag, TagDto>();

            // Article
            CreateMap<Article, ArticleDto>()
                .ForMember(d => d.AuthorName, m => m.MapFrom(s => s.Author != null ? s.Author.FullName : null))
                .ForMember(d => d.CategoryName, m => m.MapFrom(s => s.Category != null ? s.Category.Name : null))
                .ForMember(d => d.Tags, m => m.MapFrom(s =>
                    s.ArticleTags.Select(at => new TagDto(at.TagId, at.Tag.Name)).ToList()
                ));

            // Bookmark
            CreateMap<Bookmark, BookmarkDto>();

            // ReadHistory
            CreateMap<ReadHistory, ReadHistoryDto>();
        }
    }
}
