using System.Drawing;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Limpeza de arquivos não relacionados a fotos/vídeos e corrompidos ===");

        Console.Write("Informe o diretório raiz: ");
        string rootPath = Console.ReadLine()!;

        if (!Directory.Exists(rootPath))
        {
            Console.WriteLine("❌ Diretório não encontrado!");
            return;
        }

        string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".webp", ".heic", ".heif" };
        string[] videoExtensions = { ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".webm", ".m4v", ".3gp" };

        var allFiles = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories);

        int deleted = 0;
        int skipped = 0;
        int corrompidos = 0;

        foreach (var file in allFiles)
        {
            string ext = Path.GetExtension(file).ToLowerInvariant();
            bool isImage = imageExtensions.Contains(ext);
            bool isVideo = videoExtensions.Contains(ext);

            // Se não for imagem nem vídeo, exclui direto
            if (!isImage && !isVideo)
            {
                TryDelete(file, ref deleted);
                continue;
            }

            // Tratamento para imagens
            if (isImage)
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(file);
                    using (var ms = new MemoryStream(bytes))
                    using (var img = Image.FromStream(ms))
                    {
                        // imagem válida, mantém
                        skipped++;
                    }
                }
                catch
                {
                    corrompidos++;
                    Console.WriteLine($"🚫 Imagem corrompida: {file}");
                    TryDelete(file, ref deleted);
                }
                continue;
            }

            // Tratamento simples para vídeos
            if (isVideo)
            {
                try
                {
                    var info = new FileInfo(file);
                    if (info.Length <= 4096) // arquivos menores que 1KB são suspeitos
                    {
                        corrompidos++;
                        Console.WriteLine($"🚫 Vídeo possivelmente corrompido: {file}");
                        TryDelete(file, ref deleted);
                    }
                    else
                    {
                        skipped++;
                    }
                }
                catch
                {
                    corrompidos++;
                    Console.WriteLine($"🚫 Erro ao verificar vídeo: {file}");
                    TryDelete(file, ref deleted);
                }
            }
        }

        Console.WriteLine($"\n✅ Concluído!");
        Console.WriteLine($"   Total excluídos: {deleted}");
        Console.WriteLine($"   Mantidos: {skipped}");
        Console.WriteLine($"   Corrompidos removidos: {corrompidos}");
    }

    static void TryDelete(string path, ref int deleted)
    {
        try
        {
            File.Delete(path);
            deleted++;
            Console.WriteLine($"🗑️  Excluído: {path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Erro ao excluir {path}: {ex.Message}");
        }
    }
}
