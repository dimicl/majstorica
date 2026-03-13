using backend.Domain.Enums;
using backend.Infrastructure.Persistence.Neo4j.Entities;

namespace backend.Infrastructure.Persistence.Neo4j.Mappers;
public static class SkillGraphMapper
{
    public static SkillNode ToNode(MasterCategory category)
    {
        return new SkillNode
        {
            Id = ((int)category).ToString(),
            Name = category.ToString()
        };
    }

    public static object ToMergeParameters(SkillNode node)
    {
        return new { id = node.Id, name = node.Name };
    }
}
