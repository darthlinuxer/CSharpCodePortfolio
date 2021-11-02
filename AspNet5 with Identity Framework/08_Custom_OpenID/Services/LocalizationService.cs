using System.Linq;
using OpenIDApp.Context;
using OpenIDApp.Models;

namespace OpenIDApp.Services
{
    public interface ILocalizationService
    {
        StringResource GetStringResource(string resourceKey, int languageId);
    }

    public class LocalizationService : ILocalizationService
    {
        private readonly CulturesDbContext _context;

        public LocalizationService(CulturesDbContext context) => _context = context;

        public StringResource GetStringResource(string resourceKey, int languageId)
        {
            return _context.StringResources.FirstOrDefault(x =>
                    x.Key.Trim().ToLower() == resourceKey.Trim().ToLower()
                    && x.LanguageId == languageId);
        }
    }
}