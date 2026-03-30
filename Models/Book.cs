using System;

namespace prjLibrarySystem.Models
{
    // Mirrors tblBooks + vwBookAvailability:
    // ISBN (PK), Title, Author, CategoryID (FK->tblCategories), CategoryName (from join),
    // TotalCopies, AvailableCopies (computed via vwBookAvailability), Description, DateAdded, UpdatedAt
    public class Book
    {
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }

        // Category is now a FK relationship — store both ID and name
        public int? CategoryID { get; set; }
        public string CategoryName { get; set; }

        public int TotalCopies { get; set; }

        // AvailableCopies is no longer stored in tblBooks.
        // It is computed by vwBookAvailability:
        //   TotalCopies - COUNT(active accepted borrows)
        // Populate this property by querying vwBookAvailability.
        public int AvailableCopies { get; set; }

        public string Description { get; set; }
        public DateTime? DateAdded { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Book()
        {
            DateAdded = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        public Book(string isbn, string title, string author, int? categoryId, string categoryName,
                    int totalCopies, int availableCopies, string description = "")
        {
            ISBN = isbn;
            Title = title;
            Author = author;
            CategoryID = categoryId;
            CategoryName = categoryName;
            TotalCopies = totalCopies;
            AvailableCopies = availableCopies;
            Description = description;
            DateAdded = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        public bool IsAvailable => AvailableCopies > 0;
        public int BorrowedCopies => TotalCopies - AvailableCopies;
    }
}