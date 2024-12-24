namespace CryptoBot.Services
{
    public class DataService
    {
        private readonly AppDbContext _context;

        public DataService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAdminAsync(Admin admin)
        {
            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
        }

        public async Task<long> GetAdminIdAsync()
        {
            return _context.Admins.FirstOrDefault(u => u.Id == 1).Id;
        }
    }
}