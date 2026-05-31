using Shopiva.Contracts.Dashboard;

namespace Shopiva.Interfaces
{
    public interface IDashboardService
    {
        // Store Overview
        Task<StoreOverviewResponse> GetStoreOverviewAsync();

        // Recent Users
        Task<RecentUsersResponse> GetRecentUsersAsync(int count = 10);

        // Promo Codes
        Task<List<PromoCodeResponse>> GetAllPromoCodesAsync();
        Task<PromoCodeResponse> GetPromoCodeByIdAsync(int id);
        Task<PromoCodeResponse> CreatePromoCodeAsync(CreatePromoCodeRequest request);
        Task<PromoCodeResponse> UpdatePromoCodeAsync(int id, UpdatePromoCodeRequest request);
        Task DeletePromoCodeAsync(int id);

        // Banner
        Task<List<BannerResponse>> GetAllBannersAsync();
        Task<BannerResponse> GetLiveBannerAsync();
        Task<BannerResponse> CreateBannerAsync(CreateBannerRequest request);
        Task<BannerResponse> UpdateBannerAsync(int id, UpdateBannerRequest request);
        Task<BannerResponse> SetLiveBannerAsync(int id);
        Task DeleteBannerAsync(int id);
    }
}
