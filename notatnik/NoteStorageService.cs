using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace notatnik;

public static class NoteStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string FilePath { get; } = ResolveFilePath();

    private static string ResolveFilePath()
    {
        // 1. Sprawdź bieżący katalog
        string cwd = Directory.GetCurrentDirectory();
        if (File.Exists(Path.Combine(cwd, "notatki.json")))
            return Path.Combine(cwd, "notatki.json");

        if (File.Exists(Path.Combine(cwd, "notatnik.csproj")))
            return Path.Combine(cwd, "notatki.json");

        if (Directory.Exists(Path.Combine(cwd, "notatnik")))
            return Path.Combine(cwd, "notatnik", "notatki.json");

        // 2. Jeśli aplikacja uruchomiona z bin/Debug/..., znajdź katalog projektu
        var dirInfo = new DirectoryInfo(AppContext.BaseDirectory);
        while (dirInfo != null && dirInfo.Parent != null)
        {
            if (File.Exists(Path.Combine(dirInfo.FullName, "notatnik.csproj")))
            {
                return Path.Combine(dirInfo.FullName, "notatki.json");
            }
            if (File.Exists(Path.Combine(dirInfo.FullName, "notatki.json")))
            {
                return Path.Combine(dirInfo.FullName, "notatki.json");
            }
            dirInfo = dirInfo.Parent;
        }

        // 3. Domyślnie w folderze bazowym aplikacji
        return Path.Combine(AppContext.BaseDirectory, "notatki.json");
    }

    public static List<NoteModel> LoadNotes()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                var notes = JsonSerializer.Deserialize<List<NoteModel>>(json, JsonOptions);
                if (notes != null)
                {
                    return notes;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas wczytywania notatek: {ex.Message}");
        }

        // Domyślna pierwsza notatka przy pierwszym uruchomieniu
        var initialNotes = new List<NoteModel>
        {
            new NoteModel
            {
                Title = "Witaj w Notatniku! 📝",
                Content = "To jest Twoja pierwsza notatka.\n\nMożesz edytować jej tytuł i treść, a następnie kliknąć 'Zapisz'.\nNotatki są trwale zapisywane do pliku notatki.json na Twoim dysku.",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }
        };

        SaveNotes(initialNotes);
        return initialNotes;
    }

    public static void SaveNotes(IEnumerable<NoteModel> notes)
    {
        try
        {
            string? dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string json = JsonSerializer.Serialize(notes, JsonOptions);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas zapisywania notatek: {ex.Message}");
            throw;
        }
    }
}
