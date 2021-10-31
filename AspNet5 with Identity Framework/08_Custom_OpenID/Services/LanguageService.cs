using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Context;
using App.Models;
using Microsoft.EntityFrameworkCore;

namespace App.Services
{
    public interface ILanguageService
    {
        Task<IEnumerable<Language>> GetLanguages();
        Language GetLanguageByCulture(string culture);
    }

    public class LanguageService : ILanguageService
    {
        private readonly CulturesDbContext context;

        public LanguageService(CulturesDbContext context)
        {
            this.context = context;
        }
        public Language GetLanguageByCulture(string culture)
        {
            return context.Languages.FirstOrDefault(x => 
            x.Culture.Trim().ToLower() == culture.Trim().ToLower());
        }

        public async Task<IEnumerable<Language>> GetLanguages() => await context.Languages.ToListAsync();
    }
}