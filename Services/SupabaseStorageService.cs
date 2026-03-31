using Supabase;

namespace GTXZone.Services
{
    public class SupabaseStorageService
    {
        private readonly Supabase.Client _client;
        private readonly string _bucket;
        private bool _initialized;

        public SupabaseStorageService(IConfiguration configuration)
        {
            var url = configuration["Supabase:Url"];
            var key = configuration["Supabase:Key"];
            _bucket = configuration["Supabase:Bucket"] ?? "torrents";

            if (string.IsNullOrWhiteSpace(url))
                throw new Exception("Supabase:Url is missing.");

            if (string.IsNullOrWhiteSpace(key))
                throw new Exception("Supabase:Key is missing.");

            _client = new Supabase.Client(url, key);
        }

        private async Task EnsureInitializedAsync()
        {
            if (_initialized)
                return;

            await _client.InitializeAsync();
            _initialized = true;
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            await EnsureInitializedAsync();

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var tempPath = Path.Combine(Path.GetTempPath(), fileName);

            await using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            try
            {
                await _client.Storage
                    .From(_bucket)
                    .Upload(tempPath, fileName);

                return _client.Storage
                    .From(_bucket)
                    .GetPublicUrl(fileName);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        public async Task DeleteFileAsync(string? fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return;

            // Skip old local paths like /Uploads/abc.torrent
            if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
                return;

            await EnsureInitializedAsync();

            var fileName = Path.GetFileName(uri.AbsolutePath);

            if (string.IsNullOrWhiteSpace(fileName))
                return;

            await _client.Storage
                .From(_bucket)
                .Remove(new List<string> { fileName });
        }
    }
}