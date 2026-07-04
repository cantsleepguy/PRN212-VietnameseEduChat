using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Interfaces;

namespace PRN212_VietnameseEduChat.Pages.Documents
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IDocumentService _documentService;

        public DetailsModel(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        public Document? Document { get; set; }

        public List<DocumentChunk> OrderedChunks { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Document = await _documentService.GetByIdWithChunksAsync(id);

            if (Document == null)
            {
                return NotFound();
            }

            OrderedChunks = Document.Chunks
                .OrderBy(x => x.ChunkIndex)
                .ToList();

            return Page();
        }
    }
}