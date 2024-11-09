using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MovieCatalogApi.Entities;
using MovieCatalogApi.Services;
using System.ComponentModel.DataAnnotations;

namespace MovieCatalog.Web.Pages
{
    public class TitleModel : PageModel
    {
        private readonly IMovieCatalogDataService _movieCatalogDataService;

        public TitleModel(IMovieCatalogDataService movieCatalogDataService)
        {
            _movieCatalogDataService = movieCatalogDataService;
        }

        public int? Id { get; set; }

        [Required(ErrorMessage = "Primary Title is required.")]
        [StringLength(500, ErrorMessage = "Primary Title cannot exceed 500 characters.")]
        public string PrimaryTitle { get; set; }

        [StringLength(500, ErrorMessage = "Original Title cannot exceed 500 characters.")]
        public string OriginalTitle { get; set; }

        [Required(ErrorMessage = "Title Type is required.")]
        public TitleType TitleType { get; set; }

        [Range(1900, 2100, ErrorMessage = "Please enter a valid year.")]
        public int? StartYear { get; set; }

        [Range(1900, 2100, ErrorMessage = "Please enter a valid year.")]
        public int? EndYear { get; set; }

        [Range(1, 9999, ErrorMessage = "Runtime must be between 1 and 9999 minutes.")]
        public int? RuntimeMinutes { get; set; }

        [Required(ErrorMessage = "At least one genre must be selected.")]
        [MaxLength(3, ErrorMessage = "You can select a maximum of 3 genres.")]
        public List<int> SelectedGenreIds { get; set; } = new List<int>();

        public IReadOnlyCollection<SelectListItem> GenreOptions { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> Genres { get; set; } = new List<SelectListItem>();

        public string SuccessMessage
        {
            get => TempData["SuccessMessage"] as string;
            set => TempData["SuccessMessage"] = value;
        }

        public async Task OnGetAsync(int? id)
        {
            Id = id;

            if (id.HasValue)
            {
                var title = await _movieCatalogDataService.GetTitleByIdAsync(id.Value);
                if (title != null)
                {
                    PrimaryTitle = title.PrimaryTitle;
                    OriginalTitle = title.OriginalTitle;
                    TitleType = title.TitleType;
                    StartYear = title.StartYear;
                    EndYear = title.EndYear;
                    RuntimeMinutes = title.RuntimeMinutes;
                    SelectedGenreIds = title.TitleGenres.Select(g => g.GenreId).ToList();
                }
            }

            GenreOptions = await GetGenreOptionsAsync();
            Genres = GenreOptions.ToList();
        }

        public async Task<IReadOnlyCollection<SelectListItem>> GetGenreOptionsAsync()
        {
            var genres = await _movieCatalogDataService.GetGenresAsync();
            return genres.Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = g.Name
            }).ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                GenreOptions = await GetGenreOptionsAsync();
                Genres = GenreOptions.ToList();
                return Page();
            }

            var title = await _movieCatalogDataService.InsertOrUpdateTitleAsync(
                Id,
                PrimaryTitle,
                OriginalTitle,
                TitleType,
                StartYear,
                EndYear,
                RuntimeMinutes,
                SelectedGenreIds.ToArray()
            );

            SuccessMessage = "Title saved successfully!";
            return RedirectToPage(new { Id = title.Id });
        }
    }
}
