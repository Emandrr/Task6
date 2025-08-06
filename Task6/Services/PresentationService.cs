using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Task6.Contracts;
using Task6.Hubs;
using Task6.Models;

namespace Task6.Services
{
    public class PresentationService
    {
        private ApplicationDbContext _db;
        private IHubContext<PresentationHub> _hubContext;
        public PresentationService(ApplicationDbContext db, IHubContext<PresentationHub> hubContext)
        {
            _db = db;
            _hubContext = hubContext;
        }

        public async Task Create(PresentationDTO presentation)
        {
            Presentation pres = new Presentation();
            pres.CreatedAt = DateTime.UtcNow;
            pres.CreatorName = presentation.Author;
            pres.Name = presentation.Name;
            await SaveToDatabase(pres);
            await Inform(pres);
        }
        public async Task SaveToDatabase(Presentation presentation)
        {
            if (presentation.Id == 0)
            {
                await _db.Presentations.AddAsync(presentation);
            }
            else
            {
                _db.Presentations.Update(presentation);
            }

            try
            {
                var ans = await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task Inform(Presentation presentation)
        {
            await _hubContext.Clients.Group("PresentationsGroup").SendAsync("NewPresentationAdded", new
            {
                id = presentation.Id,
                name = presentation.Name,
                author = presentation.CreatorName ?? "Unknown",
                date = presentation.CreatedAt.ToString("dd/MM/yyyy")
            });
        }
        public List<Presentation> ListAll()
        {
            return _db.Presentations.ToList();
        }
        public async Task<Presentation> GetPresentation(int id)
        {
            return await _db.Presentations
                          .Include(p => p.Slides).ThenInclude(s => s.TextElements)
                          .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<bool> ChangePresentatiomUserRole(string presentationId, string userName, string newRole,string Role)
        {
            if (userName == null || newRole == null) return false;
            Presentation presentation = await GetPresentation(Int32.Parse(presentationId));
            if (Role!="Creator") return false;
            return await ChangePresentatiomUserRoleChecked(presentation, newRole,userName);
        }
        public async Task<bool> ChangePresentatiomUserRoleChecked(Presentation presentation,string newRole,string userName)
        {
            if (newRole != "Editor" && newRole != "Viewer") return false;
            else if (newRole == "Editor") return await ChangeToEditor(presentation,newRole,userName);
            else  return await ChangeToViewer(presentation, newRole, userName);
        }
        public async Task<bool> ChangeToViewer(Presentation presentation, string newRole, string userName)
        {
            if (presentation.EditUserNames.Contains(userName))
            {
                presentation.EditUserNames.Remove(userName);
            }
            await UpdatePresentation(presentation);
            return true;
        }
        public async Task<bool> ChangeToEditor(Presentation presentation, string newRole, string userName)
        {
            if (!presentation.EditUserNames.Contains(userName))
            {
                presentation.EditUserNames.Add(userName);
            }
            await UpdatePresentation(presentation);
            return true;
        }
        public async Task UpdatePresentation(Presentation presentation)
        {
            _db.Update(presentation);
            await _db.SaveChangesAsync();
        }
        public int GetAll()
        {
            return _db.Presentations.ToList().Count;
        }
    }
}
