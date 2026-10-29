Niech model poprawi twoją prośbę:
Stwórz desktopową aplikację notatnika w C# (.NET 10) z interfejsem w Avalonia UI (motyw Fluent).
Wymagania funkcjonalne i interfejs:
1. Układ okna:
   - Dwie kolumny: lewy panel o szerokości ok. 300px na listę notatek, prawy panel na edytor.
   - Lewy panel: przycisk "+ Nowa notatka" na górze oraz lista zapisanych notatek (każdy element pokazuje tytuł, sformatowaną datę modyfikacji i krótki podgląd treści).
   - Prawy panel: jednowierszowe pole na tytuł, wielowierszowe pole na treść, przycisk "Zapisz" (skrót Ctrl+S) oraz przycisk "Usuń".
2. Logika działania:
   - Kliknięcie notatki na liście wczytuje jej dane do edytora.
   - Przycisk "Zapisz":
     - Jeśli edytowana jest istniejąca notatka: aktualizuje jej treść i pole UpdatedAt.
     - Jeśli nie wybrano notatki (lub kliknięto "+ Nowa"): tworzy nowy wpis z unikalnym GUID.
   - Walidacja: Tytuł nie może być pusty – w przypadku braku tytułu wyświetl komunikat ostrzegawczy i nie zapisuj zmian.
   - Przycisk "Usuń": usuwa aktualnie wybraną notatkę z listy i czyści formularz.
3. Przechowywanie danych:
   - Notatki zapisuj w pojedynczym pliku "notatki.json" w katalogu roboczym aplikacji w formacie JSON (kodowanie UTF-8, wcięcie indented).
   - Struktura rekordu: Id (string/guid), Title (string), Content (string), CreatedAt (DateTime), UpdatedAt (DateTime).
   - Lista notatek powinna być posortowana malejąco według daty UpdatedAt (najnowsze na górze).
   - Jeśli plik "notatki.json" nie istnieje przy starcie aplikacji, utwórz go automatycznie z jedną powitalną notatką demonstracyjną.
