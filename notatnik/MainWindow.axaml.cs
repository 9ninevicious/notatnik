using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace notatnik;

public partial class MainWindow : Window
{
    private readonly List<NoteModel> _allNotes = new();
    private readonly ObservableCollection<NoteModel> _filteredNotes = new();
    private NoteModel? _selectedNote;
    private bool _isUpdatingUi;

    public MainWindow()
    {
        InitializeComponent();

        AddHandler(KeyDownEvent, OnWindowKeyDown, RoutingStrategies.Tunnel);

        NotesListBox.ItemsSource = _filteredNotes;

        LoadSavedNotes();
    }

    private void LoadSavedNotes()
    {
        _allNotes.Clear();
        var notes = NoteStorageService.LoadNotes();
        _allNotes.AddRange(notes);

        ApplyFilter();

        if (_filteredNotes.Count > 0)
        {
            NotesListBox.SelectedItem = _filteredNotes[0];
        }
        else
        {
            PrepareNewNote();
        }

        StatusTextBlock.Text = $"Baza danych: {Path.GetFileName(NoteStorageService.FilePath)}";
    }

    private void ApplyFilter(string? query = null)
    {
        query = query?.Trim();

        _filteredNotes.Clear();

        var filtered = string.IsNullOrWhiteSpace(query)
            ? _allNotes
            : _allNotes.Where(n =>
                (n.Title != null && n.Title.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (n.Content != null && n.Content.Contains(query, StringComparison.OrdinalIgnoreCase)));

        foreach (var note in filtered)
        {
            _filteredNotes.Add(note);
        }

        NotesCountTextBlock.Text = $"Notatki: {_allNotes.Count}";
    }

    private void OnNoteSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingUi) return;

        if (NotesListBox.SelectedItem is NoteModel note)
        {
            _selectedNote = note;

            _isUpdatingUi = true;
            TitleTextBox.Text = note.Title;
            ContentTextBox.Text = note.Content;
            _isUpdatingUi = false;

            UnsavedBadge.IsVisible = false;
            EditorHeaderTextBlock.Text = "Edycja notatki";
            DeleteButton.IsEnabled = true;

            StatusTextBlock.Text = $"Wczytano: {note.DisplayTitle}";
            UpdateStats();
        }
    }

    private void OnNewNoteClicked(object? sender, RoutedEventArgs e)
    {
        PrepareNewNote();
    }

    private void PrepareNewNote()
    {
        _selectedNote = null;
        NotesListBox.SelectedItem = null;

        _isUpdatingUi = true;
        TitleTextBox.Text = string.Empty;
        ContentTextBox.Text = string.Empty;
        _isUpdatingUi = false;

        UnsavedBadge.IsVisible = false;
        EditorHeaderTextBlock.Text = "Nowa notatka";
        DeleteButton.IsEnabled = false;

        TitleTextBox.Focus();
        StatusTextBlock.Text = "Wpisz tytuł oraz treść, a następnie kliknij 'Zapisz notatkę'.";
        UpdateStats();
    }

    private void OnSaveClicked(object? sender, RoutedEventArgs e)
    {
        string title = TitleTextBox.Text?.Trim() ?? string.Empty;
        string content = ContentTextBox.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(content))
        {
            StatusTextBlock.Text = "⚠️ Wpisz tytuł lub treść przed zapisaniem.";
            return;
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            title = "Bez tytułu";
            TitleTextBox.Text = title;
        }

        try
        {
            if (_selectedNote != null)
            {
                // Aktualizacja istniejącej notatki
                _selectedNote.Title = title;
                _selectedNote.Content = content;
                _selectedNote.UpdatedAt = DateTime.Now;
            }
            else
            {
                // Utworzenie nowej notatki
                var newNote = new NoteModel
                {
                    Title = title,
                    Content = content,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _allNotes.Insert(0, newNote);
                _selectedNote = newNote;
            }

            // Trwały zapis do pliku
            NoteStorageService.SaveNotes(_allNotes);

            ApplyFilter(SearchTextBox.Text);

            _isUpdatingUi = true;
            NotesListBox.SelectedItem = _selectedNote;
            _isUpdatingUi = false;

            UnsavedBadge.IsVisible = false;
            DeleteButton.IsEnabled = true;
            EditorHeaderTextBlock.Text = "Edycja notatki";

            StatusTextBlock.Text = $"💾 Pomyślnie zapisano do pliku {Path.GetFileName(NoteStorageService.FilePath)} ({DateTime.Now:HH:mm:ss})";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"❌ Błąd zapisu do pliku: {ex.Message}";
        }
    }

    private void OnDeleteClicked(object? sender, RoutedEventArgs e)
    {
        if (_selectedNote == null) return;

        var noteToRemove = _selectedNote;
        _allNotes.Remove(noteToRemove);

        try
        {
            NoteStorageService.SaveNotes(_allNotes);
            ApplyFilter(SearchTextBox.Text);

            if (_filteredNotes.Count > 0)
            {
                NotesListBox.SelectedItem = _filteredNotes[0];
            }
            else
            {
                PrepareNewNote();
            }

            StatusTextBlock.Text = $"🗑️ Notatka '{noteToRemove.DisplayTitle}' została usunięta z pliku.";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"❌ Błąd podczas usuwania: {ex.Message}";
        }
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        ApplyFilter(SearchTextBox.Text);
    }

    private void OnEditorTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isUpdatingUi) return;

        UnsavedBadge.IsVisible = true;
        UpdateStats();
    }

    private void UpdateStats()
    {
        string text = ContentTextBox.Text ?? string.Empty;
        int chars = text.Length;
        int words = string.IsNullOrWhiteSpace(text)
            ? 0
            : text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

        StatsTextBlock.Text = $"Znaki: {chars} | Słowa: {words}";
    }

    private void OnOpenFileLocationClicked(object? sender, RoutedEventArgs e)
    {
        try
        {
            string filePath = NoteStorageService.FilePath;
            if (File.Exists(filePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{filePath}\"",
                    UseShellExecute = true
                });
            }
            else
            {
                string? dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = dir,
                        UseShellExecute = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Nie można otworzyć folderu: {ex.Message}";
        }
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            if (e.Key == Key.S)
            {
                OnSaveClicked(sender, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.N)
            {
                OnNewNoteClicked(sender, new RoutedEventArgs());
                e.Handled = true;
            }
        }
    }
}