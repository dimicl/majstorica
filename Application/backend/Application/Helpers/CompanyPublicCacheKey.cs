namespace backend.Application.Helpers;

public static class CompanyPublicCacheKey
{
    public static string ForCompany(Guid companyId) => $"cache:company:public:{companyId:D}";
}
