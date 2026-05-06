using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Touir.ExpensesManager.Expenses.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Parent categories (IDs 1–17) ───────────────────────────────────
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "IsArchived", "ParentCategoryId" },
                values: new object[,]
                {
                    {  1, "Housing",                   "Expenses related to home, rent, utilities, property, and household maintenance.", false, null },
                    {  2, "Food and Drinks",            "All expenses related to food, beverages, groceries, and dining.",               false, null },
                    {  3, "Transportation",             "Expenses related to commuting, vehicles, and transport services.",              false, null },
                    {  4, "Health and Medical",         "Healthcare, pharmacy purchases, medical services, and wellness expenses.",      false, null },
                    {  5, "Shopping",                   "Personal purchases, consumer goods, and retail shopping.",                     false, null },
                    {  6, "Personal Care",              "Beauty, hygiene, grooming, and self-care expenses.",                           false, null },
                    {  7, "Bills and Subscriptions",    "Recurring bills, memberships, and subscription-based services.",               false, null },
                    {  8, "Education",                  "Education and learning expenses for adults and children.",                     false, null },
                    {  9, "Kids and Family",            "Expenses related to children, childcare, and family activities.",              false, null },
                    { 10, "Entertainment and Leisure",  "Leisure activities, hobbies, and fun expenses.",                              false, null },
                    { 11, "Travel and Vacation",        "Trips, holidays, and vacation-related expenses.",                             false, null },
                    { 12, "Pets",                       "Expenses related to pets and animal care.",                                   false, null },
                    { 13, "Gifts and Donations",        "Gifts for others and charitable donations.",                                  false, null },
                    { 14, "Financial and Banking",      "Financial transactions, banking fees, savings, investments, and taxes.",      false, null },
                    { 15, "Work and Business",          "Expenses related to work, freelancing, and business activities.",             false, null },
                    { 16, "Government and Legal",       "Legal costs, government fees, and administrative expenses.",                  false, null },
                    { 17, "Miscellaneous",              "Uncategorized or unexpected expenses.",                                       false, null },
                });

            // ── Housing subcategories (IDs 18–29, ParentCategoryId = 1) ────────
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "IsArchived", "ParentCategoryId" },
                values: new object[,]
                {
                    { 18, "Rent",               "Monthly rent payments.",                                                 false, 1 },
                    { 19, "Mortgage",           "Mortgage payments for home ownership.",                                  false, 1 },
                    { 20, "Property Tax",       "Government property taxes for housing.",                                 false, 1 },
                    { 21, "Home Insurance",     "Homeowner or renter insurance payments.",                                false, 1 },
                    { 22, "Utilities",          "Electricity, gas, water, heating and other utility bills.",              false, 1 },
                    { 23, "Internet",           "Home internet subscription.",                                            false, 1 },
                    { 24, "Cable TV",           "Cable TV or TV subscription services.",                                  false, 1 },
                    { 25, "Home Repairs",       "Repairs, maintenance, plumbing, electrician, renovation costs.",         false, 1 },
                    { 26, "Furniture",          "Furniture purchases for the home.",                                      false, 1 },
                    { 27, "Appliances",         "Household appliances such as fridge, washing machine, etc.",             false, 1 },
                    { 28, "Home Services",      "Cleaning service, gardener, security, pest control.",                    false, 1 },
                    { 29, "Household Supplies", "Cleaning products, tools, and small household items.",                   false, 1 },
                });

            // ── Food and Drinks subcategories (IDs 30–36, ParentCategoryId = 2) ─
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "IsArchived", "ParentCategoryId" },
                values: new object[,]
                {
                    { 30, "Groceries",     "Supermarket purchases, fresh food, household food supplies.", false, 2 },
                    { 31, "Restaurants",   "Meals eaten at restaurants.",                                 false, 2 },
                    { 32, "Fast Food",     "Fast food and quick-service meals.",                          false, 2 },
                    { 33, "Cafes",         "Coffee shops, pastries, and drinks.",                         false, 2 },
                    { 34, "Snacks",        "Snacks, sweets, and small food purchases.",                   false, 2 },
                    { 35, "Food Delivery", "Food ordered via delivery services.",                         false, 2 },
                    { 36, "Work Meals",    "Meals purchased during work hours.",                          false, 2 },
                });

            // ── Transportation subcategories (IDs 37–48, ParentCategoryId = 3) ──
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "IsArchived", "ParentCategoryId" },
                values: new object[,]
                {
                    { 37, "Public Transport",     "Bus, metro, tram, train subscriptions and tickets.",        false, 3 },
                    { 38, "Taxi",                 "Taxi rides and fares.",                                     false, 3 },
                    { 39, "Ride Sharing",          "Uber, Bolt, and similar ride-sharing services.",           false, 3 },
                    { 40, "Fuel",                 "Gasoline, diesel, or electric charging costs.",             false, 3 },
                    { 41, "Car Loan",             "Car loan or leasing payments.",                             false, 3 },
                    { 42, "Car Insurance",        "Insurance for car.",                                        false, 3 },
                    { 43, "Motorcycle Insurance", "Insurance for motorcycle or scooter.",                      false, 3 },
                    { 44, "Parking",              "Parking fees and subscriptions.",                           false, 3 },
                    { 45, "Tolls",                "Highway and road toll charges.",                            false, 3 },
                    { 46, "Vehicle Maintenance",  "Repairs, servicing, tires, spare parts, inspections.",     false, 3 },
                    { 47, "Car Wash",             "Car cleaning and detailing.",                               false, 3 },
                    { 48, "Vehicle Rental",       "Car, bike, scooter rentals.",                               false, 3 },
                });

            // ── Health and Medical subcategories (IDs 49–58, ParentCategoryId = 4)
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "IsArchived", "ParentCategoryId" },
                values: new object[,]
                {
                    { 49, "Health Insurance", "Health insurance and mutual insurance payments.",    false, 4 },
                    { 50, "Doctor",           "General doctor appointments.",                       false, 4 },
                    { 51, "Specialist",       "Specialist doctor consultations.",                   false, 4 },
                    { 52, "Dentist",          "Dental care, orthodontics, treatments.",             false, 4 },
                    { 53, "Optician",         "Glasses, contact lenses, vision products.",          false, 4 },
                    { 54, "Hospital",         "Hospital bills, emergency services, surgery costs.", false, 4 },
                    { 55, "Pharmacy",         "Medicines and medical products.",                    false, 4 },
                    { 56, "Therapy",          "Psychologist, counseling, therapy sessions.",        false, 4 },
                    { 57, "Fitness",          "Gym membership, yoga, sports training.",             false, 4 },
                    { 58, "Wellness",         "Massage, spa, wellness programs.",                   false, 4 },
                });

            // ── Shopping subcategories (IDs 59–64, ParentCategoryId = 5) ────────
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "IsArchived", "ParentCategoryId" },
                values: new object[,]
                {
                    { 59, "Clothing",        "Clothes for adults or children.",                  false, 5 },
                    { 60, "Shoes",           "Shoes and footwear purchases.",                    false, 5 },
                    { 61, "Accessories",     "Bags, jewelry, watches, fashion accessories.",     false, 5 },
                    { 62, "Electronics",     "Phones, laptops, tablets, TVs, accessories.",      false, 5 },
                    { 63, "Home Decor",      "Decoration items for the home.",                   false, 5 },
                    { 64, "Household Items", "Kitchen tools, utensils, small household items.",  false, 5 },
                });

            // ── Personal Care subcategories (IDs 65–68, ParentCategoryId = 6) ───
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "IsArchived", "ParentCategoryId" },
                values: new object[,]
                {
                    { 65, "Hairdresser",       "Haircuts, salon visits, hair treatments.",     false, 6 },
                    { 66, "Cosmetics",         "Makeup, skincare, beauty products.",           false, 6 },
                    { 67, "Toiletries",        "Soap, shampoo, toothpaste, hygiene products.", false, 6 },
                    { 68, "Beauty Treatments", "Nails, facial care, spa treatments.",          false, 6 },
                });

            // ── Bills and Subscriptions subcategories (IDs 69–73, ParentCategoryId = 7)
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "IsArchived", "ParentCategoryId" },
                values: new object[,]
                {
                    { 69, "Mobile Plan",           "Mobile phone subscription bills.",                                    false, 7 },
                    { 70, "Internet Subscription", "Internet service provider subscription.",                             false, 7 },
                    { 71, "Streaming",             "Netflix, Disney Plus, Spotify, YouTube Premium subscriptions.",       false, 7 },
                    { 72, "Software Subscription", "Cloud services, apps, antivirus, productivity tools.",                false, 7 },
                    { 73, "Memberships",           "Gym memberships, clubs, associations.",                               false, 7 },
                });

            // ── Education subcategories (IDs 74–79, ParentCategoryId = 8) ───────
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "IsArchived", "ParentCategoryId" },
                values: new object[,]
                {
                    { 74, "School Fees",     "School tuition and registration fees.",    false, 8 },
                    { 75, "Books",           "Books, magazines, educational materials.", false, 8 },
                    { 76, "School Supplies", "Notebooks, pens, and school materials.",   false, 8 },
                    { 77, "Online Courses",  "Online courses and training programs.",    false, 8 },
                    { 78, "Certifications",  "Professional certifications and exams.",   false, 8 },
                    { 79, "Tutoring",        "Private lessons and tutoring sessions.",   false, 8 },
                });

            // ── Kids and Family subcategories (IDs 80–85, ParentCategoryId = 9) ─
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "IsArchived", "ParentCategoryId" },
                values: new object[,]
                {
                    { 80, "Childcare",         "Daycare, babysitter, nanny services.",                  false, 9 },
                    { 81, "Kids Clothing",     "Children clothing and shoes.",                          false, 9 },
                    { 82, "Toys",              "Toys and games for children.",                          false, 9 },
                    { 83, "School Activities", "School trips and extracurricular activities.",          false, 9 },
                    { 84, "Kids Activities",   "Sports, music, and other activities for children.",    false, 9 },
                    { 85, "Pocket Money",      "Allowance given to children.",                         false, 9 },
                });

            // ── Entertainment and Leisure subcategories (IDs 86–91, ParentCategoryId = 10)
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "IsArchived", "ParentCategoryId" },
                values: new object[,]
                {
                    { 86, "Cinema",            "Cinema tickets and movie rentals.",                false, 10 },
                    { 87, "Concerts",          "Concerts, festivals, live shows.",                 false, 10 },
                    { 88, "Events",            "Paid events and activities.",                      false, 10 },
                    { 89, "Games",             "Video games, board games, subscriptions.",         false, 10 },
                    { 90, "Hobbies",           "Sports equipment, music, art, crafts.",            false, 10 },
                    { 91, "Sports Activities", "Sports clubs, training sessions, classes.",        false, 10 },
                });

            // ── Travel and Vacation subcategories (IDs 92–98, ParentCategoryId = 11)
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "IsArchived", "ParentCategoryId" },
                values: new object[,]
                {
                    { 92, "Flights",          "Airplane tickets and airline expenses.",           false, 11 },
                    { 93, "Train Tickets",    "Train transport expenses.",                        false, 11 },
                    { 94, "Hotels",           "Hotels, Airbnb, and accommodation bookings.",      false, 11 },
                    { 95, "Travel Food",      "Meals and restaurants during travel.",             false, 11 },
                    { 96, "Tourism",          "Museums, attractions, tours, tickets.",            false, 11 },
                    { 97, "Car Rental",       "Car rental services during travel.",               false, 11 },
                    { 98, "Travel Insurance", "Insurance for travel and vacation.",               false, 11 },
                });

            // ── Pets subcategories (IDs 99–103, ParentCategoryId = 12) ──────────
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "IsArchived", "ParentCategoryId" },
                values: new object[,]
                {
                    {  99, "Pet Food",     "Pet food and treats.",                    false, 12 },
                    { 100, "Veterinarian", "Vet visits, vaccines, medicine.",         false, 12 },
                    { 101, "Pet Supplies", "Toys, cages, accessories.",               false, 12 },
                    { 102, "Pet Grooming", "Grooming services for pets.",             false, 12 },
                    { 103, "Pet Sitting",  "Pet sitting services during travel.",     false, 12 },
                });

            // ── Gifts and Donations subcategories (IDs 104–106, ParentCategoryId = 13)
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "IsArchived", "ParentCategoryId" },
                values: new object[,]
                {
                    { 104, "Gifts",        "Birthday gifts, wedding gifts, holiday gifts.",  false, 13 },
                    { 105, "Donations",    "Charity donations and contributions.",            false, 13 },
                    { 106, "Celebrations", "Party supplies, decorations, event expenses.",   false, 13 },
                });

            // ── Financial and Banking subcategories (IDs 107–113, ParentCategoryId = 14)
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "IsArchived", "ParentCategoryId" },
                values: new object[,]
                {
                    { 107, "Bank Fees",      "Monthly bank fees, transfer fees, card fees.", false, 14 },
                    { 108, "Loan Repayment", "Loan payments and credit repayment.",          false, 14 },
                    { 109, "Savings",        "Money deposited into savings accounts.",       false, 14 },
                    { 110, "Investments",    "Stocks, ETFs, crypto, investment deposits.",   false, 14 },
                    { 111, "Taxes",          "Income tax, local taxes, government taxes.",   false, 14 },
                    { 112, "Life Insurance", "Life insurance payments.",                     false, 14 },
                    { 113, "Car Insurance",  "Insurance payments for car.",                  false, 14 },
                });

            // ── Work and Business subcategories (IDs 114–119, ParentCategoryId = 15)
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "IsArchived", "ParentCategoryId" },
                values: new object[,]
                {
                    { 114, "Office Supplies",       "Stationery, printing, office consumables.",        false, 15 },
                    { 115, "Work Equipment",        "Laptop, phone, accessories used for work.",        false, 15 },
                    { 116, "Professional Services", "Accountant, lawyer, business tools.",              false, 15 },
                    { 117, "Coworking",             "Coworking spaces and office rent.",                false, 15 },
                    { 118, "Business Travel",       "Transport and hotels for business purposes.",      false, 15 },
                    { 119, "Business Meals",        "Meals with clients or business lunches.",          false, 15 },
                });

            // ── Government and Legal subcategories (IDs 120–122, ParentCategoryId = 16)
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "IsArchived", "ParentCategoryId" },
                values: new object[,]
                {
                    { 120, "Administrative Fees", "Documents, permits, official paperwork fees.", false, 16 },
                    { 121, "Legal Fees",          "Lawyer, notary, legal consultations.",         false, 16 },
                    { 122, "Fines",               "Parking fines, speeding tickets, penalties.",  false, 16 },
                });

            // ── Miscellaneous subcategories (IDs 123–125, ParentCategoryId = 17)
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "IsArchived", "ParentCategoryId" },
                values: new object[,]
                {
                    { 123, "Emergency",       "Unexpected urgent expenses.",                       false, 17 },
                    { 124, "Cash Withdrawal", "Cash withdrawn from ATM for offline spending.",     false, 17 },
                    { 125, "Other",           "Expenses not fitting any category.",                false, 17 },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Subcategories first (FK), then parents
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValues: new object[]
                {
                     18,  19,  20,  21,  22,  23,  24,  25,  26,  27,  28,  29,
                     30,  31,  32,  33,  34,  35,  36,
                     37,  38,  39,  40,  41,  42,  43,  44,  45,  46,  47,  48,
                     49,  50,  51,  52,  53,  54,  55,  56,  57,  58,
                     59,  60,  61,  62,  63,  64,
                     65,  66,  67,  68,
                     69,  70,  71,  72,  73,
                     74,  75,  76,  77,  78,  79,
                     80,  81,  82,  83,  84,  85,
                     86,  87,  88,  89,  90,  91,
                     92,  93,  94,  95,  96,  97,  98,
                     99, 100, 101, 102, 103,
                    104, 105, 106,
                    107, 108, 109, 110, 111, 112, 113,
                    114, 115, 116, 117, 118, 119,
                    120, 121, 122,
                    123, 124, 125,
                });

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17
                });
        }
    }
}
