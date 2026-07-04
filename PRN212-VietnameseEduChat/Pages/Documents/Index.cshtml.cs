using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Interfaces;

namespace PRN212_VietnameseEduChat.Pages.Documents
{
    public class IndexModel : PageModel
    {
        private readonly IDocumentService _documentService;

        public IndexModel(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        public IList<Document> Documents { get; set; }
            = new List<Document>();

        public async Task OnGetAsync()
        {
            Documents = await _documentService.GetAllAsync();
        }
    }
}
