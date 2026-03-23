using BusinessObjects.Entities;

namespace MealPrep.DAL.Data;

public static class MealSeedData
{
    /// <summary>
    /// Danh sách món ăn đa dạng (healthy + comfort). Embedding luôn = null.
    /// </summary>
    public static List<Meal> GetAllMeals(DateTime createdAt)
    {
        var meals = new List<Meal>
        {
            // High-protein / muscle gain
            new Meal
            {
                Name        = "Ức gà nướng mật ong rau củ",
                Ingredients = new[] { "Ức gà", "Mật ong", "Tỏi", "Khoai lang", "Bông cải xanh", "Dầu oliu" },
                Images      = new[] { "https://images.unsplash.com/photo-1604908176997-1251884b08a3?auto=format&fit=crop&w=800&q=80" },
                Description = "Ức gà nướng sốt mật ong, ăn kèm khoai lang và bông cải xanh hấp – giàu protein, ít béo.",
                Calories    = 420,
                Protein     = 42m,
                Carbs       = 38m,
                Fat         = 10m,
                IsActive    = true,
                CreatedAt   = createdAt,
                Embedding   = null
            },
            new Meal
            {
                Name        = "Cá hồi áp chảo sốt chanh",
                Ingredients = new[] { "Cá hồi", "Chanh", "Bơ", "Măng tây", "Khoai tây baby", "Tiêu đen" },
                Images      = new[] { "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=800&q=80" },
                Description = "Cá hồi da giòn áp chảo, sốt bơ chanh, giàu Omega‑3 tốt cho tim mạch.",
                Calories    = 520,
                Protein     = 40m,
                Carbs       = 28m,
                Fat         = 24m,
                IsActive    = true,
                CreatedAt   = createdAt,
                Embedding   = null
            },
            new Meal
            {
                Name        = "Bò xào rau củ ngũ sắc",
                Ingredients = new[] { "Thịt bò thăn", "Ớt chuông", "Cà rốt", "Hành tây", "Đậu que", "Tỏi" },
                Images      = new[] { "https://images.unsplash.com/photo-1558030006-450675393462?auto=format&fit=crop&w=800&q=80" },
                Description = "Thịt bò mềm xào nhanh với rau củ ngũ sắc – giàu sắt và protein.",
                Calories    = 460,
                Protein     = 38m,
                Carbs       = 30m,
                Fat         = 18m,
                IsActive    = true,
                CreatedAt   = createdAt,
                Embedding   = null
            },
            new Meal
            {
                Name        = "Gà quay Địa Trung Hải",
                Ingredients = new[] { "Đùi gà", "Chanh vàng", "Hương thảo", "Khoai tây bi", "Cà rốt", "Dầu oliu" },
                Images      = new[] { "https://images.unsplash.com/photo-1604908176997-1251884b08a3?auto=format&fit=crop&w=800&q=80" },
                Description = "Đùi gà quay da giòn tẩm gia vị Địa Trung Hải thơm mùi chanh và thảo mộc.",
                Calories    = 560,
                Protein     = 38m,
                Carbs       = 32m,
                Fat         = 24m,
                IsActive    = true,
                CreatedAt   = createdAt,
                Embedding   = null
            },
            new Meal
            {
                Name        = "Mì Ý gà xé sốt kem nấm",
                Ingredients = new[] { "Mì spaghetti", "Ức gà", "Nấm mỡ", "Kem tươi", "Phô mai", "Tỏi" },
                Images      = new[] { "https://images.unsplash.com/photo-1543353071-873f17a7a088?auto=format&fit=crop&w=800&q=80" },
                Description = "Mì Ý sốt kem nấm béo nhẹ, thêm gà xé cho bữa ăn nhiều đạm.",
                Calories    = 640,
                Protein     = 32m,
                Carbs       = 70m,
                Fat         = 24m,
                IsActive    = true,
                CreatedAt   = createdAt,
                Embedding   = null
            },

            // Low-calorie / fat loss
            new Meal
            {
                Name        = "Salad gà nướng sốt yogurt",
                Ingredients = new[] { "Ức gà nướng", "Xà lách", "Cà chua bi", "Dưa leo", "Sốt yogurt", "Hạnh nhân" },
                Images      = new[] { "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=800&q=80" },
                Description = "Salad ức gà nướng với sốt yogurt ít béo, nhiều rau xanh, hạt hạnh nhân.",
                Calories    = 310,
                Protein     = 28m,
                Carbs       = 18m,
                Fat         = 12m,
                IsActive    = true,
                CreatedAt   = createdAt,
                Embedding   = null
            },
            new Meal
            {
                Name        = "Tôm áp chảo salad cam",
                Ingredients = new[] { "Tôm tươi", "Cam", "Xà lách", "Hành tây đỏ", "Dầu oliu", "Mật ong" },
                Images      = new[] { "https://images.unsplash.com/photo-1559339352-11d035aa65de?auto=format&fit=crop&w=800&q=80" },
                Description = "Tôm áp chảo sốt cam mật ong nhẹ, ăn kèm salad giòn mát.",
                Calories    = 270,
                Protein     = 24m,
                Carbs       = 19m,
                Fat         = 9m,
                IsActive    = true,
                CreatedAt   = createdAt,
                Embedding   = null
            },
            new Meal
            {
                Name        = "Canh gà rau củ ít béo",
                Ingredients = new[] { "Ức gà", "Cà rốt", "Khoai tây", "Hành tây", "Cần tây", "Nước dùng gà" },
                Images      = new[] { "https://images.unsplash.com/photo-1551183053-bf91a1d81141?auto=format&fit=crop&w=800&q=80" },
                Description = "Canh gà hầm rau củ thanh nhẹ, ít chất béo, phù hợp bữa tối.",
                Calories    = 240,
                Protein     = 22m,
                Carbs       = 20m,
                Fat         = 6m,
                IsActive    = true,
                CreatedAt   = createdAt,
                Embedding   = null
            },
            new Meal
            {
                Name        = "Phở gà ít béo",
                Ingredients = new[] { "Bánh phở", "Ức gà", "Nước lèo", "Hành lá", "Ngò gai", "Chanh" },
                Images      = new[] { "https://images.unsplash.com/photo-1546069901-5ec6a79120b0?auto=format&fit=crop&w=800&q=80" },
                Description = "Tô phở gà ít mỡ với nước lèo trong, nhiều rau thơm.",
                Calories    = 320,
                Protein     = 24m,
                Carbs       = 42m,
                Fat         = 7m,
                IsActive    = true,
                CreatedAt   = createdAt,
                Embedding   = null
            },

            // Balanced / daily
            new Meal
            {
                Name        = "Cơm gạo lứt gà xé trứng luộc",
                Ingredients = new[] { "Gạo lứt", "Ức gà xé", "Trứng luộc", "Dưa leo", "Cà rốt", "Nước mắm chanh" },
                Images      = new[] { "https://images.unsplash.com/photo-1604908177525-4025a14fb3cd?auto=format&fit=crop&w=800&q=80" },
                Description = "Tô cơm gạo lứt với ức gà xé, trứng luộc và rau củ – cân bằng dưỡng chất.",
                Calories    = 480,
                Protein     = 32m,
                Carbs       = 55m,
                Fat         = 14m,
                IsActive    = true,
                CreatedAt   = createdAt,
                Embedding   = null
            },
            new Meal
            {
                Name        = "Bowl quinoa cá hồi & rau củ",
                Ingredients = new[] { "Quinoa", "Cá hồi", "Bông cải xanh", "Cà rốt", "Bắp non", "Sốt chanh mật ong" },
                Images      = new[] { "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?auto=format&fit=crop&w=800&q=80" },
                Description = "Bowl quinoa với cá hồi áp chảo và rau củ – cung cấp đủ đạm, tinh bột tốt và chất xơ.",
                Calories    = 520,
                Protein     = 35m,
                Carbs       = 48m,
                Fat         = 18m,
                IsActive    = true,
                CreatedAt   = createdAt,
                Embedding   = null
            },

            // Vegan / vegetarian
            new Meal
            {
                Name        = "Bowl đậu gà & rau củ nướng (Vegan)",
                Ingredients = new[] { "Đậu gà", "Khoai lang", "Bông cải xanh", "Ớt chuông", "Dầu oliu", "Paprika" },
                Images      = new[] { "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?auto=format&fit=crop&w=800&q=80" },
                Description = "Đậu gà và rau củ nướng tẩm gia vị smoky, hoàn toàn thuần chay nhưng đủ đạm.",
                Calories    = 430,
                Protein     = 17m,
                Carbs       = 60m,
                Fat         = 11m,
                IsActive    = true,
                CreatedAt   = createdAt,
                Embedding   = null
            },
            new Meal
            {
                Name        = "Pasta nguyên cám sốt pesto rau củ (Veggie)",
                Ingredients = new[] { "Pasta nguyên cám", "Sốt pesto", "Cà chua bi", "Măng tây", "Phô mai parmesan" },
                Images      = new[] { "https://images.unsplash.com/photo-1546069901-eacef0df6022?auto=format&fit=crop&w=800&q=80" },
                Description = "Mì pasta nguyên cám sốt pesto, nhiều rau củ và một ít phô mai.",
                Calories    = 510,
                Protein     = 20m,
                Carbs       = 62m,
                Fat         = 16m,
                IsActive    = true,
                CreatedAt   = createdAt,
                Embedding   = null
            },

            // Low‑carb / keto
            new Meal
            {
                Name        = "Salad cá ngừ & bơ (Low‑carb)",
                Ingredients = new[] { "Cá ngừ", "Quả bơ", "Xà lách", "Cà chua", "Dưa leo", "Dầu oliu" },
                Images      = new[] { "https://images.unsplash.com/photo-1546069901-5ec6a79120b0?auto=format&fit=crop&w=800&q=80" },
                Description = "Salad cá ngừ và bơ béo ngậy, ít tinh bột, phù hợp chế độ low‑carb/keto.",
                Calories    = 360,
                Protein     = 30m,
                Carbs       = 10m,
                Fat         = 22m,
                IsActive    = true,
                CreatedAt   = createdAt,
                Embedding   = null
            },
            new Meal
            {
                Name        = "Gà xào nấm & măng tây (Keto)",
                Ingredients = new[] { "Ức gà", "Nấm", "Măng tây", "Kem tươi", "Bơ", "Tỏi" },
                Images      = new[] { "https://images.unsplash.com/photo-1604908176997-1251884b08a3?auto=format&fit=crop&w=800&q=80" },
                Description = "Ức gà xào nấm kem với măng tây – giàu đạm, gần như không tinh bột.",
                Calories    = 430,
                Protein     = 38m,
                Carbs       = 8m,
                Fat         = 24m,
                IsActive    = true,
                CreatedAt   = createdAt,
                Embedding   = null
            },

            // Comfort / cheat meals (ví dụ)
            new Meal
            {
                Name        = "Pizza hải sản phô mai",
                Ingredients = new[] { "Đế pizza", "Tôm", "Mực", "Phô mai mozzarella", "Sốt cà chua", "Ô liu" },
                Images      = new[] { "https://images.unsplash.com/photo-1548365328-9daaf8bdea48?auto=format&fit=crop&w=800&q=80" },
                Description = "Pizza đế mỏng phủ hải sản tươi và nhiều phô mai – món “cheat” cuối tuần.",
                Calories    = 780,
                Protein     = 34m,
                Carbs       = 80m,
                Fat         = 32m,
                IsActive    = true,
                CreatedAt   = createdAt,
                Embedding   = null
            },
            new Meal
            {
                Name        = "Burger bò phô mai",
                Ingredients = new[] { "Bánh burger", "Bò xay", "Phô mai cheddar", "Xà lách", "Cà chua", "Sốt mayo" },
                Images      = new[] { "https://images.unsplash.com/photo-1550547660-d9450f859349?auto=format&fit=crop&w=800&q=80" },
                Description = "Burger bò phô mai béo ngậy, ăn kèm rau tươi.",
                Calories    = 820,
                Protein     = 36m,
                Carbs       = 60m,
                Fat         = 45m,
                IsActive    = true,
                CreatedAt   = createdAt,
                Embedding   = null
            },
            new Meal
            {
                Name        = "Cơm tấm sườn bì chả",
                Ingredients = new[] { "Gạo tấm", "Sườn nướng", "Bì heo", "Chả trứng", "Mỡ hành", "Dưa leo" },
                Images      = new[] { "https://images.unsplash.com/photo-1533777324565-a040eb52fac1?auto=format&fit=crop&w=800&q=80" },
                Description = "Cơm tấm sườn bì chả kiểu Sài Gòn, đậm đà và no lâu.",
                Calories    = 900,
                Protein     = 40m,
                Carbs       = 95m,
                Fat         = 38m,
                IsActive    = true,
                CreatedAt   = createdAt,
                Embedding   = null
            },
            new Meal
            {
                Name        = "Bún bò Huế đặc biệt",
                Ingredients = new[] { "Bún", "Bò bắp", "Chả cua", "Huyết heo", "Sả", "Ớt sa tế" },
                Images      = new[] { "https://images.unsplash.com/photo-1604908176997-1251884b08a3?auto=format&fit=crop&w=800&q=80" },
                Description = "Bún bò Huế cay thơm, nước dùng đậm đà, nhiều topping.",
                Calories    = 780,
                Protein     = 38m,
                Carbs       = 82m,
                Fat         = 26m,
                IsActive    = true,
                CreatedAt   = createdAt,
                Embedding   = null
            },
            new Meal
            {
                Name        = "Bánh mì thịt nướng",
                Ingredients = new[] { "Bánh mì", "Thịt nướng", "Dưa leo", "Đồ chua", "Ngò", "Nước mắm pha" },
                Images      = new[] { "https://images.unsplash.com/photo-1604908176991-24057c2b3f49?auto=format&fit=crop&w=800&q=80" },
                Description = "Ổ bánh mì thịt nướng giòn rụm, đầy ắp topping kiểu Việt.",
                Calories    = 650,
                Protein     = 28m,
                Carbs       = 70m,
                Fat         = 22m,
                IsActive    = true,
                CreatedAt   = createdAt,
                Embedding   = null
            }
        };

        // Bổ sung thêm các món đa dạng bằng cách kết hợp protein + phong cách món ăn
        // để đạt khoảng 100 món khác nhau (không chỉ healthy).
        var proteinOptions = new[]
        {
            "Ức gà",
            "Cá hồi",
            "Thịt bò thăn",
            "Tôm",
            "Đậu hũ",
            "Cá ngừ"
        };

        var styleOptions = new[]
        {
            new
            {
                Suffix = "salad ngũ sắc",
                Ingredients = new[] { "Xà lách", "Cà chua bi", "Dưa leo", "Hành tây đỏ", "Dầu oliu" },
                Description = "Salad tươi với nhiều rau củ ngũ sắc, sốt dầu giấm nhẹ, phù hợp bữa trưa nhẹ bụng.",
                Calories = 320, Protein = 24m, Carbs = 22m, Fat = 14m
            },
            new
            {
                Suffix = "cơm gạo lứt rau củ",
                Ingredients = new[] { "Gạo lứt", "Cà rốt", "Bông cải xanh", "Bắp non", "Nước mắm chanh" },
                Description = "Đĩa cơm gạo lứt với rau củ xào nhẹ, cân bằng tinh bột chậm và chất xơ.",
                Calories = 480, Protein = 26m, Carbs = 68m, Fat = 10m
            },
            new
            {
                Suffix = "mì Ý sốt cà chua",
                Ingredients = new[] { "Mì spaghetti", "Sốt cà chua", "Húng quế", "Phô mai bào" },
                Description = "Mì Ý sốt cà chua kiểu nhà hàng, thêm protein để đủ chất cho cả ngày.",
                Calories = 620, Protein = 30m, Carbs = 78m, Fat = 18m
            },
            new
            {
                Suffix = "bowl quinoa Á-Âu",
                Ingredients = new[] { "Quinoa", "Bắp non", "Đậu que", "Sốt chanh mật ong" },
                Description = "Bowl quinoa kết hợp vị Á‑Âu, nhiều chất xơ và vi chất thiết yếu.",
                Calories = 540, Protein = 28m, Carbs = 60m, Fat = 16m
            },
            new
            {
                Suffix = "bún gạo nước dùng thanh",
                Ingredients = new[] { "Bún gạo", "Hành lá", "Rau thơm", "Chanh", "Ớt" },
                Description = "Tô bún nước trong, thanh vị, thêm đạm để no bụng mà không quá nặng.",
                Calories = 430, Protein = 25m, Carbs = 55m, Fat = 9m
            },
            new
            {
                Suffix = "wrap rau củ sốt chua ngọt",
                Ingredients = new[] { "Bánh tortilla", "Xà lách", "Bắp cải tím", "Cà rốt bào", "Sốt chua ngọt" },
                Description = "Wrap tiện lợi với rau củ giòn và protein, thích hợp mang đi làm.",
                Calories = 510, Protein = 27m, Carbs = 58m, Fat = 17m
            },
            new
            {
                Suffix = "cháo dinh dưỡng buổi sáng",
                Ingredients = new[] { "Gạo tẻ", "Gạo lứt", "Cà rốt", "Hành lá" },
                Description = "Cháo mềm ấm bụng cho buổi sáng, bổ sung thêm đạm để đủ năng lượng.",
                Calories = 380, Protein = 22m, Carbs = 54m, Fat = 7m
            },
            new
            {
                Suffix = "mì ramen nước tương",
                Ingredients = new[] { "Mì ramen", "Rong biển", "Mè rang", "Hành lá" },
                Description = "Bát mì ramen đậm đà nước tương, thêm topping protein cho bữa tối thoả mãn.",
                Calories = 720, Protein = 34m, Carbs = 80m, Fat = 24m
            }
        };

        var extraImages = new[]
        {
            "https://images.unsplash.com/photo-1504674900247-0877df9cc836?auto=format&fit=crop&w=800&q=80",
            "https://images.unsplash.com/photo-1504674900247-083f1dc1d7c1?auto=format&fit=crop&w=800&q=80",
            "https://images.unsplash.com/photo-1467003909585-2f8a72700288?auto=format&fit=crop&w=800&q=80",
            "https://images.unsplash.com/photo-1490645935967-10de6ba17061?auto=format&fit=crop&w=800&q=80"
        };

        foreach (var protein in proteinOptions)
        {
            foreach (var style in styleOptions)
            {
                if (meals.Count >= 100) break;

                var ingredients = new List<string> { protein };
                ingredients.AddRange(style.Ingredients);

                var img = extraImages[meals.Count % extraImages.Length];

                meals.Add(new Meal
                {
                    Name = $"{protein} {style.Suffix}",
                    Ingredients = ingredients.ToArray(),
                    Images = new[] { img },
                    Description = style.Description,
                    Calories = style.Calories,
                    Protein = style.Protein,
                    Carbs = style.Carbs,
                    Fat = style.Fat,
                    IsActive = true,
                    CreatedAt = createdAt,
                    Embedding = null
                });
            }

            if (meals.Count >= 100) break;
        }

        // Đảm bảo mỗi món có ít nhất 3 ảnh (Images.Length >= 3)
        var fallbackImages = new[]
        {
            "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=800&q=80",
            "https://images.unsplash.com/photo-1551218808-94e220e084d2?auto=format&fit=crop&w=800&q=80",
            "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?auto=format&fit=crop&w=800&q=80"
        };

        foreach (var meal in meals)
        {
            if (meal.Images == null || meal.Images.Length == 0)
            {
                meal.Images = fallbackImages;
            }
            else if (meal.Images.Length == 1)
            {
                meal.Images = new[]
                {
                    meal.Images[0],
                    fallbackImages[0],
                    fallbackImages[1]
                };
            }
            else if (meal.Images.Length == 2)
            {
                meal.Images = new[]
                {
                    meal.Images[0],
                    meal.Images[1],
                    fallbackImages[0]
                };
            }
        }

        return meals;
    }
}
//using BusinessObjects.Entities;

//namespace MealPrep.DAL.Data
//{
//    public static class MealSeedData
//    {
//        public static List<Meal> GetAllMeals(DateTime createdAt)
//        {
//            var meals = new List<Meal>();

//            // PROTEIN-RICH MEALS (Muscle Gain) - 20 meals
//            meals.AddRange(GetProteinRichMeals(createdAt));

//            // LOW-CALORIE MEALS (Fat Loss) - 20 meals
//            meals.AddRange(GetLowCalorieMeals(createdAt));

//            // BALANCED MEALS (Maintain) - 20 meals
//            meals.AddRange(GetBalancedMeals(createdAt));

//            // VEGAN MEALS - 10 meals
//            meals.AddRange(GetVeganMeals(createdAt));

//            // VEGETARIAN MEALS - 10 meals
//            meals.AddRange(GetVegetarianMeals(createdAt));

//            // LOW-CARB/KETO MEALS - 10 meals
//            meals.AddRange(GetLowCarbMeals(createdAt));

//            // HALAL MEALS - 5 meals
//            meals.AddRange(GetHalalMeals(createdAt));

//            // GLUTEN-FREE MEALS - 5 meals
//            meals.AddRange(GetGlutenFreeMeals(createdAt));

//            return meals;
//        }

//        private static List<Meal> GetProteinRichMeals(DateTime createdAt)
//        {
//            return new List<Meal>
//            {
//                new Meal
//                {
//                    Id = 1,
//                    Name = "Ức Gà Nướng Mật Ong",
//                    Ingredients = "[\"Ức gà\",\"Mật ong\",\"Tỏi\",\"Gừng\",\"Hành tây\",\"Dầu oliu\",\"Muối\",\"Tiêu\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
//                    Description = "Ức gà nướng thơm lừng với sốt mật ong đậm đà, kèm rau củ tươi ngon. Món ăn giàu protein, ít calo, phù hợp cho chế độ ăn kiêng và tập luyện.",
//                    Calories = 320,
//                    Protein = 45.5m,
//                    Carbs = 18.2m,
//                    Fat = 8.8m,
//                    BasePrice = 85000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 2,
//                    Name = "Cá Hồi Áp Chảo",
//                    Ingredients = "[\"Cá hồi\",\"Bơ\",\"Chanh\",\"Thì là\",\"Khoai tây\",\"Bông cải xanh\",\"Muối\",\"Tiêu\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
//                    Description = "Cá hồi tươi ngon áp chảo với lớp da giòn tan, kèm rau củ hấp. Nguồn Omega-3 dồi dào, tốt cho tim mạch và não bộ.",
//                    Calories = 480,
//                    Protein = 42.0m,
//                    Carbs = 25.0m,
//                    Fat = 22.5m,
//                    BasePrice = 120000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 3,
//                    Name = "Bò Xào Rau Củ",
//                    Ingredients = "[\"Thịt bò\",\"Cà rốt\",\"Ớt chuông\",\"Hành tây\",\"Nấm\",\"Tỏi\",\"Gừng\",\"Nước tương\",\"Dầu mè\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]",
//                    Description = "Thịt bò mềm xào với rau củ tươi, sốt đậm đà. Món ăn giàu sắt và protein, phù hợp cho người tập gym.",
//                    Calories = 420,
//                    Protein = 38.0m,
//                    Carbs = 22.0m,
//                    Fat = 18.5m,
//                    BasePrice = 95000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 4,
//                    Name = "Ức Gà Sốt Teriyaki",
//                    Ingredients = "[\"Ức gà\",\"Nước tương\",\"Mật ong\",\"Gừng\",\"Tỏi\",\"Hành tây\",\"Ớt chuông\",\"Dầu mè\",\"Hạt mè\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
//                    Description = "Ức gà sốt teriyaki đậm đà kiểu Nhật, kèm rau củ xào. Món ăn giàu protein, ít béo, phù hợp cho người tập luyện.",
//                    Calories = 380,
//                    Protein = 42.0m,
//                    Carbs = 28.0m,
//                    Fat = 10.5m,
//                    BasePrice = 90000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 5,
//                    Name = "Cá Ngừ Nướng",
//                    Ingredients = "[\"Cá ngừ\",\"Chanh\",\"Tỏi\",\"Thì là\",\"Khoai lang\",\"Bông cải xanh\",\"Dầu oliu\",\"Muối\",\"Tiêu\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
//                    Description = "Cá ngừ tươi nướng vừa chín tới, kèm khoai lang và rau củ. Nguồn protein và Omega-3 dồi dào.",
//                    Calories = 400,
//                    Protein = 48.0m,
//                    Carbs = 30.0m,
//                    Fat = 12.0m,
//                    BasePrice = 105000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 6,
//                    Name = "Gà Tây Nướng",
//                    Ingredients = "[\"Gà tây\",\"Gạo lứt\",\"Đậu xanh\",\"Cà rốt\",\"Hành tây\",\"Tỏi\",\"Gia vị nướng\",\"Dầu oliu\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
//                    Description = "Gà tây nướng thơm lừng với cơm gạo lứt và đậu xanh. Món ăn cân bằng dinh dưỡng, giàu protein và chất xơ.",
//                    Calories = 450,
//                    Protein = 44.0m,
//                    Carbs = 45.0m,
//                    Fat = 10.0m,
//                    BasePrice = 88000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 7,
//                    Name = "Thịt Bò Bít Tết",
//                    Ingredients = "[\"Thịt bò\",\"Khoai tây nghiền\",\"Bông cải xanh\",\"Cà rốt\",\"Tỏi\",\"Bơ\",\"Muối\",\"Tiêu\",\"Hương thảo\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]",
//                    Description = "Thịt bò bít tết mềm ngon, kèm khoai tây nghiền và rau củ. Nguồn protein và sắt cao.",
//                    Calories = 520,
//                    Protein = 46.0m,
//                    Carbs = 35.0m,
//                    Fat = 22.0m,
//                    BasePrice = 150000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 8,
//                    Name = "Ức Gà Nướng Thảo Mộc",
//                    Ingredients = "[\"Ức gà\",\"Hương thảo\",\"Thì là\",\"Tỏi\",\"Chanh\",\"Dầu oliu\",\"Khoai lang\",\"Đậu xanh\",\"Muối\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
//                    Description = "Ức gà nướng với thảo mộc thơm lừng, kèm khoai lang và đậu xanh. Protein cao, ít béo.",
//                    Calories = 350,
//                    Protein = 43.0m,
//                    Carbs = 32.0m,
//                    Fat = 7.5m,
//                    BasePrice = 82000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 9,
//                    Name = "Cá Thu Nướng",
//                    Ingredients = "[\"Cá thu\",\"Chanh\",\"Ớt\",\"Tỏi\",\"Gừng\",\"Rau thơm\",\"Khoai tây\",\"Cà chua\",\"Dầu oliu\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
//                    Description = "Cá thu nướng thơm ngon, giàu Omega-3 và protein. Kèm khoai tây và rau củ.",
//                    Calories = 380,
//                    Protein = 40.0m,
//                    Carbs = 28.0m,
//                    Fat = 14.0m,
//                    BasePrice = 98000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 10,
//                    Name = "Thịt Heo Nướng BBQ",
//                    Ingredients = "[\"Thịt heo\",\"Sốt BBQ\",\"Hành tây\",\"Ớt chuông\",\"Bắp\",\"Khoai tây\",\"Tỏi\",\"Gia vị nướng\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]",
//                    Description = "Thịt heo nướng sốt BBQ đậm đà, kèm rau củ nướng. Protein cao, hương vị đậm đà.",
//                    Calories = 480,
//                    Protein = 41.0m,
//                    Carbs = 38.0m,
//                    Fat = 19.0m,
//                    BasePrice = 92000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 11,
//                    Name = "Gà Nướng Muối Ớt",
//                    Ingredients = "[\"Gà\",\"Muối ớt\",\"Tỏi\",\"Chanh\",\"Rau thơm\",\"Gạo lứt\",\"Rau củ\",\"Dầu ăn\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
//                    Description = "Gà nướng muối ớt cay nồng, kèm cơm gạo lứt và rau củ. Protein cao, hương vị đậm đà.",
//                    Calories = 420,
//                    Protein = 39.0m,
//                    Carbs = 40.0m,
//                    Fat = 12.0m,
//                    BasePrice = 88000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 12,
//                    Name = "Cá Basa Chiên Giòn",
//                    Ingredients = "[\"Cá basa\",\"Bột chiên\",\"Trứng\",\"Bánh mì\",\"Rau sống\",\"Sốt tartar\",\"Chanh\",\"Dầu ăn\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
//                    Description = "Cá basa chiên giòn vàng, kèm rau sống và sốt tartar. Protein cao, giòn ngon.",
//                    Calories = 450,
//                    Protein = 36.0m,
//                    Carbs = 42.0m,
//                    Fat = 15.0m,
//                    BasePrice = 75000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 13,
//                    Name = "Bò Kho",
//                    Ingredients = "[\"Thịt bò\",\"Cà rốt\",\"Hành tây\",\"Gừng\",\"Sả\",\"Nước dừa\",\"Gia vị\",\"Bánh mì\",\"Rau thơm\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]",
//                    Description = "Bò kho đậm đà, thịt bò mềm ngon với cà rốt và hành tây. Protein và sắt cao.",
//                    Calories = 480,
//                    Protein = 44.0m,
//                    Carbs = 45.0m,
//                    Fat = 16.0m,
//                    BasePrice = 110000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 14,
//                    Name = "Gà Nướng Lá Chanh",
//                    Ingredients = "[\"Gà\",\"Lá chanh\",\"Sả\",\"Tỏi\",\"Ớt\",\"Gừng\",\"Nước mắm\",\"Gạo\",\"Rau củ\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
//                    Description = "Gà nướng lá chanh thơm lừng, kèm cơm và rau củ. Protein cao, hương vị đặc trưng.",
//                    Calories = 440,
//                    Protein = 41.0m,
//                    Carbs = 38.0m,
//                    Fat = 14.0m,
//                    BasePrice = 95000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 15,
//                    Name = "Cá Chép Hấp Xì Dầu",
//                    Ingredients = "[\"Cá chép\",\"Xì dầu\",\"Gừng\",\"Hành lá\",\"Ớt\",\"Dầu mè\",\"Gạo\",\"Rau củ\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
//                    Description = "Cá chép hấp xì dầu thơm ngon, kèm cơm và rau củ. Protein cao, ít béo.",
//                    Calories = 360,
//                    Protein = 38.0m,
//                    Carbs = 32.0m,
//                    Fat = 10.0m,
//                    BasePrice = 85000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 16,
//                    Name = "Thịt Bò Xào Lăn",
//                    Ingredients = "[\"Thịt bò\",\"Hành tây\",\"Ớt chuông\",\"Cà chua\",\"Tỏi\",\"Gừng\",\"Nước tương\",\"Dầu mè\",\"Gạo\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]",
//                    Description = "Thịt bò xào lăn đậm đà, kèm cơm. Protein cao, hương vị đậm đà.",
//                    Calories = 420,
//                    Protein = 40.0m,
//                    Carbs = 35.0m,
//                    Fat = 16.0m,
//                    BasePrice = 98000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 17,
//                    Name = "Gà Rang Muối",
//                    Ingredients = "[\"Gà\",\"Muối\",\"Tỏi\",\"Ớt\",\"Hành lá\",\"Gạo\",\"Rau củ\",\"Dầu ăn\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
//                    Description = "Gà rang muối giòn ngon, kèm cơm và rau củ. Protein cao, đơn giản nhưng ngon.",
//                    Calories = 400,
//                    Protein = 38.0m,
//                    Carbs = 36.0m,
//                    Fat = 13.0m,
//                    BasePrice = 90000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 18,
//                    Name = "Cá Hồi Sashimi Bowl",
//                    Ingredients = "[\"Cá hồi\",\"Gạo sushi\",\"Rong biển\",\"Dưa chuột\",\"Cà rốt\",\"Sốt teriyaki\",\"Wasabi\",\"Gừng ngâm\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
//                    Description = "Bowl cá hồi sashimi tươi ngon, kèm gạo sushi và rau củ. Protein và Omega-3 cao.",
//                    Calories = 450,
//                    Protein = 44.0m,
//                    Carbs = 42.0m,
//                    Fat = 15.0m,
//                    BasePrice = 130000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 19,
//                    Name = "Thịt Bò Nướng Lụi",
//                    Ingredients = "[\"Thịt bò\",\"Sả\",\"Tỏi\",\"Ớt\",\"Gia vị nướng\",\"Bánh tráng\",\"Rau sống\",\"Nước chấm\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]",
//                    Description = "Thịt bò nướng lụi thơm lừng, kèm bánh tráng và rau sống. Protein cao, hương vị đậm đà.",
//                    Calories = 380,
//                    Protein = 39.0m,
//                    Carbs = 28.0m,
//                    Fat = 16.0m,
//                    BasePrice = 105000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 20,
//                    Name = "Gà Nướng Mật Ong Tỏi",
//                    Ingredients = "[\"Gà\",\"Mật ong\",\"Tỏi\",\"Gừng\",\"Hành tây\",\"Khoai tây\",\"Rau củ\",\"Dầu oliu\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
//                    Description = "Gà nướng mật ong tỏi ngọt ngào, kèm khoai tây và rau củ. Protein cao, hương vị đặc biệt.",
//                    Calories = 410,
//                    Protein = 40.0m,
//                    Carbs = 38.0m,
//                    Fat = 14.0m,
//                    BasePrice = 92000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                }
//            };
//        }

//        private static List<Meal> GetLowCalorieMeals(DateTime createdAt)
//        {
//            return new List<Meal>
//            {
//                new Meal
//                {
//                    Id = 21,
//                    Name = "Salad Gà Nướng",
//                    Ingredients = "[\"Ức gà\",\"Xà lách\",\"Cà chua bi\",\"Dưa chuột\",\"Ớt chuông\",\"Dầu giấm\",\"Phô mai feta\",\"Hạt óc chó\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]",
//                    Description = "Salad tươi ngon với ức gà nướng, rau xanh giòn, phô mai feta và hạt óc chó. Món ăn nhẹ, giàu chất xơ và vitamin.",
//                    Calories = 280,
//                    Protein = 28.0m,
//                    Carbs = 15.0m,
//                    Fat = 14.0m,
//                    BasePrice = 75000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 22,
//                    Name = "Tôm Sốt Cam",
//                    Ingredients = "[\"Tôm tươi\",\"Cam\",\"Mật ong\",\"Gừng\",\"Tỏi\",\"Ớt\",\"Hành lá\",\"Dầu oliu\",\"Muối\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]",
//                    Description = "Tôm tươi sốt cam chua ngọt, kèm rau củ. Món ăn giàu protein, ít calo, phù hợp cho chế độ ăn kiêng.",
//                    Calories = 250,
//                    Protein = 30.0m,
//                    Carbs = 20.0m,
//                    Fat = 8.0m,
//                    BasePrice = 110000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 23,
//                    Name = "Salad Cá Ngừ",
//                    Ingredients = "[\"Cá ngừ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây đỏ\",\"Dầu giấm\",\"Quả bơ\",\"Hạt hướng dương\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]",
//                    Description = "Salad cá ngừ tươi ngon, kèm rau xanh và quả bơ. Protein cao, ít calo, giàu Omega-3.",
//                    Calories = 290,
//                    Protein = 32.0m,
//                    Carbs = 18.0m,
//                    Fat = 12.0m,
//                    BasePrice = 95000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 24,
//                    Name = "Gà Luộc Rau Củ",
//                    Ingredients = "[\"Gà\",\"Cà rốt\",\"Bông cải xanh\",\"Đậu que\",\"Khoai tây\",\"Hành tây\",\"Gia vị\",\"Nước dùng\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
//                    Description = "Gà luộc mềm ngon, kèm rau củ hấp. Ít calo, giàu protein, phù hợp cho chế độ giảm cân.",
//                    Calories = 320,
//                    Protein = 35.0m,
//                    Carbs = 22.0m,
//                    Fat = 9.0m,
//                    BasePrice = 78000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 25,
//                    Name = "Cá Hấp Gừng",
//                    Ingredients = "[\"Cá\",\"Gừng\",\"Hành lá\",\"Ớt\",\"Nước tương\",\"Dầu mè\",\"Rau củ\",\"Gạo lứt\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
//                    Description = "Cá hấp gừng thơm ngon, kèm cơm gạo lứt và rau củ. Ít calo, giàu protein.",
//                    Calories = 300,
//                    Protein = 34.0m,
//                    Carbs = 28.0m,
//                    Fat = 8.5m,
//                    BasePrice = 88000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 26,
//                    Name = "Salad Quả Bơ Gà",
//                    Ingredients = "[\"Ức gà\",\"Quả bơ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu oliu\",\"Chanh\",\"Muối\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]",
//                    Description = "Salad quả bơ và gà tươi ngon, giàu chất béo tốt và protein. Ít calo, bổ dưỡng.",
//                    Calories = 270,
//                    Protein = 26.0m,
//                    Carbs = 16.0m,
//                    Fat = 13.0m,
//                    BasePrice = 82000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 27,
//                    Name = "Tôm Hấp Bia",
//                    Ingredients = "[\"Tôm\",\"Bia\",\"Sả\",\"Ớt\",\"Chanh\",\"Muối\",\"Rau củ\",\"Gạo lứt\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]",
//                    Description = "Tôm hấp bia thơm ngon, kèm cơm gạo lứt và rau củ. Ít calo, giàu protein.",
//                    Calories = 240,
//                    Protein = 28.0m,
//                    Carbs = 22.0m,
//                    Fat = 6.0m,
//                    BasePrice = 105000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 28,
//                    Name = "Gà Nướng Không Da",
//                    Ingredients = "[\"Ức gà không da\",\"Gia vị\",\"Chanh\",\"Rau củ hấp\",\"Khoai lang\",\"Dầu oliu\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
//                    Description = "Gà nướng không da ít béo, kèm rau củ và khoai lang. Protein cao, calo thấp.",
//                    Calories = 260,
//                    Protein = 38.0m,
//                    Carbs = 20.0m,
//                    Fat = 5.0m,
//                    BasePrice = 75000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 29,
//                    Name = "Cá Nướng Giấy Bạc",
//                    Ingredients = "[\"Cá\",\"Chanh\",\"Gừng\",\"Hành lá\",\"Ớt\",\"Rau củ\",\"Gạo lứt\",\"Dầu oliu\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
//                    Description = "Cá nướng giấy bạc giữ nguyên hương vị, kèm cơm gạo lứt. Ít calo, giàu protein.",
//                    Calories = 280,
//                    Protein = 32.0m,
//                    Carbs = 26.0m,
//                    Fat = 7.0m,
//                    BasePrice = 90000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 30,
//                    Name = "Salad Trứng Luộc",
//                    Ingredients = "[\"Trứng\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu giấm\",\"Hạt chia\",\"Rau thơm\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]",
//                    Description = "Salad trứng luộc tươi ngon, giàu protein. Ít calo, bổ dưỡng.",
//                    Calories = 220,
//                    Protein = 18.0m,
//                    Carbs = 12.0m,
//                    Fat = 11.0m,
//                    BasePrice = 65000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 31,
//                    Name = "Gà Xào Rau Củ",
//                    Ingredients = "[\"Ức gà\",\"Cà rốt\",\"Ớt chuông\",\"Bông cải xanh\",\"Nấm\",\"Tỏi\",\"Dầu oliu\",\"Gia vị\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
//                    Description = "Gà xào rau củ tươi ngon, ít calo. Protein cao, giàu chất xơ.",
//                    Calories = 290,
//                    Protein = 32.0m,
//                    Carbs = 24.0m,
//                    Fat = 8.0m,
//                    BasePrice = 80000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 32,
//                    Name = "Cá Hồi Nướng Rau Củ",
//                    Ingredients = "[\"Cá hồi\",\"Bông cải xanh\",\"Cà rốt\",\"Khoai lang\",\"Dầu oliu\",\"Chanh\",\"Gia vị\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
//                    Description = "Cá hồi nướng kèm rau củ, giàu Omega-3. Ít calo, bổ dưỡng.",
//                    Calories = 310,
//                    Protein = 36.0m,
//                    Carbs = 28.0m,
//                    Fat = 10.0m,
//                    BasePrice = 115000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 33,
//                    Name = "Salad Tôm Bơ",
//                    Ingredients = "[\"Tôm\",\"Quả bơ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu giấm\",\"Chanh\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]",
//                    Description = "Salad tôm và bơ tươi ngon, giàu protein và chất béo tốt. Ít calo.",
//                    Calories = 260,
//                    Protein = 24.0m,
//                    Carbs = 14.0m,
//                    Fat = 12.0m,
//                    BasePrice = 98000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 34,
//                    Name = "Gà Luộc Chấm Muối Tiêu",
//                    Ingredients = "[\"Gà\",\"Muối\",\"Tiêu\",\"Chanh\",\"Rau củ hấp\",\"Gạo lứt\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
//                    Description = "Gà luộc mềm ngon chấm muối tiêu, kèm rau củ. Ít calo, giàu protein.",
//                    Calories = 300,
//                    Protein = 34.0m,
//                    Carbs = 26.0m,
//                    Fat = 8.5m,
//                    BasePrice = 78000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 35,
//                    Name = "Cá Kho Tộ",
//                    Ingredients = "[\"Cá\",\"Nước mắm\",\"Đường\",\"Ớt\",\"Gừng\",\"Hành tây\",\"Gạo\",\"Rau củ\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
//                    Description = "Cá kho tộ đậm đà, kèm cơm và rau củ. Ít calo, giàu protein.",
//                    Calories = 280,
//                    Protein = 30.0m,
//                    Carbs = 30.0m,
//                    Fat = 6.0m,
//                    BasePrice = 85000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 36,
//                    Name = "Salad Ức Gà Nướng",
//                    Ingredients = "[\"Ức gà\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Ớt chuông\",\"Dầu giấm\",\"Hạt hướng dương\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]",
//                    Description = "Salad ức gà nướng tươi ngon, giàu protein. Ít calo, bổ dưỡng.",
//                    Calories = 250,
//                    Protein = 30.0m,
//                    Carbs = 16.0m,
//                    Fat = 9.0m,
//                    BasePrice = 72000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 37,
//                    Name = "Tôm Rang Me",
//                    Ingredients = "[\"Tôm\",\"Me\",\"Đường\",\"Tỏi\",\"Ớt\",\"Hành lá\",\"Gạo\",\"Rau củ\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]",
//                    Description = "Tôm rang me chua ngọt, kèm cơm. Ít calo, giàu protein.",
//                    Calories = 270,
//                    Protein = 26.0m,
//                    Carbs = 32.0m,
//                    Fat = 7.0m,
//                    BasePrice = 102000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 38,
//                    Name = "Gà Hấp Muối",
//                    Ingredients = "[\"Gà\",\"Muối\",\"Gừng\",\"Hành lá\",\"Rau củ\",\"Gạo lứt\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
//                    Description = "Gà hấp muối mềm ngon, kèm rau củ. Ít calo, giàu protein.",
//                    Calories = 290,
//                    Protein = 33.0m,
//                    Carbs = 24.0m,
//                    Fat = 8.0m,
//                    BasePrice = 76000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 39,
//                    Name = "Cá Nướng Muối Ớt",
//                    Ingredients = "[\"Cá\",\"Muối ớt\",\"Chanh\",\"Rau củ\",\"Gạo lứt\",\"Dầu oliu\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
//                    Description = "Cá nướng muối ớt cay nồng, kèm rau củ. Ít calo, giàu protein.",
//                    Calories = 260,
//                    Protein = 28.0m,
//                    Carbs = 22.0m,
//                    Fat = 7.5m,
//                    BasePrice = 88000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                },
//                new Meal
//                {
//                    Id = 40,
//                    Name = "Salad Cá Ngừ Đóng Hộp",
//                    Ingredients = "[\"Cá ngừ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu giấm\",\"Trứng luộc\"]",
//                    Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]",
//                    Description = "Salad cá ngừ đóng hộp tiện lợi, giàu protein. Ít calo, bổ dưỡng.",
//                    Calories = 240,
//                    Protein = 22.0m,
//                    Carbs = 14.0m,
//                    Fat = 10.0m,
//                    BasePrice = 68000,
//                    IsActive = true,
//                    CreatedAt = createdAt
//                }
//            };
//        }

//        private static List<Meal> GetBalancedMeals(DateTime createdAt)
//        {
//            return new List<Meal>
//            {
//                new Meal { Id = 41, Name = "Cơm Gà Tây Nướng", Ingredients = "[\"Gà tây\",\"Gạo lứt\",\"Đậu xanh\",\"Cà rốt\",\"Hành tây\",\"Tỏi\",\"Gia vị nướng\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Gà tây nướng thơm lừng với cơm gạo lứt và đậu xanh. Món ăn cân bằng dinh dưỡng, giàu protein và chất xơ.", Calories = 420, Protein = 40.0m, Carbs = 45.0m, Fat = 10.0m, BasePrice = 88000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 42, Name = "Bowl Quinoa Gà", Ingredients = "[\"Quinoa\",\"Ức gà\",\"Bơ\",\"Cà chua\",\"Dưa chuột\",\"Hành tây đỏ\",\"Rau mầm\",\"Sốt tahini\",\"Chanh\"]", Images = "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", Description = "Bowl quinoa đầy đủ dinh dưỡng với ức gà, rau củ tươi và sốt tahini. Món ăn healthy, giàu protein và chất xơ.", Calories = 400, Protein = 35.0m, Carbs = 42.0m, Fat = 12.5m, BasePrice = 92000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 43, Name = "Cơm Thịt Kho Tàu", Ingredients = "[\"Thịt ba chỉ\",\"Trứng\",\"Nước dừa\",\"Nước mắm\",\"Đường\",\"Hành tây\",\"Gạo\",\"Dưa chua\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Thịt kho tàu đậm đà, kèm trứng và cơm. Món ăn cân bằng dinh dưỡng, hương vị đậm đà.", Calories = 480, Protein = 32.0m, Carbs = 50.0m, Fat = 18.0m, BasePrice = 85000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 44, Name = "Cơm Sườn Nướng", Ingredients = "[\"Sườn heo\",\"Mật ong\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Sườn heo nướng mật ong thơm ngon, kèm cơm và rau củ. Cân bằng dinh dưỡng.", Calories = 520, Protein = 35.0m, Carbs = 48.0m, Fat = 22.0m, BasePrice = 98000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 45, Name = "Cơm Gà Nướng Muối Ớt", Ingredients = "[\"Gà\",\"Muối ớt\",\"Tỏi\",\"Chanh\",\"Gạo\",\"Rau củ\",\"Dầu ăn\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Gà nướng muối ớt cay nồng, kèm cơm và rau củ. Cân bằng dinh dưỡng, hương vị đậm đà.", Calories = 450, Protein = 38.0m, Carbs = 42.0m, Fat = 16.0m, BasePrice = 90000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 46, Name = "Cơm Cá Kho Tộ", Ingredients = "[\"Cá\",\"Nước mắm\",\"Đường\",\"Ớt\",\"Gừng\",\"Hành tây\",\"Gạo\",\"Rau củ\"]", Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", Description = "Cá kho tộ đậm đà, kèm cơm và rau củ. Cân bằng dinh dưỡng, hương vị truyền thống.", Calories = 380, Protein = 32.0m, Carbs = 40.0m, Fat = 12.0m, BasePrice = 88000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 47, Name = "Cơm Thịt Bò Xào", Ingredients = "[\"Thịt bò\",\"Hành tây\",\"Ớt chuông\",\"Cà chua\",\"Tỏi\",\"Gừng\",\"Nước tương\",\"Gạo\",\"Dầu mè\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Thịt bò xào đậm đà, kèm cơm. Cân bằng dinh dưỡng, giàu protein.", Calories = 440, Protein = 36.0m, Carbs = 38.0m, Fat = 18.0m, BasePrice = 102000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 48, Name = "Cơm Gà Luộc", Ingredients = "[\"Gà\",\"Muối\",\"Gừng\",\"Hành lá\",\"Gạo\",\"Rau củ\",\"Nước dùng\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Gà luộc mềm ngon, kèm cơm và rau củ. Cân bằng dinh dưỡng, đơn giản nhưng ngon.", Calories = 400, Protein = 35.0m, Carbs = 40.0m, Fat = 12.0m, BasePrice = 80000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 49, Name = "Cơm Tôm Rang Me", Ingredients = "[\"Tôm\",\"Me\",\"Đường\",\"Tỏi\",\"Ớt\",\"Hành lá\",\"Gạo\",\"Rau củ\"]", Images = "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]", Description = "Tôm rang me chua ngọt, kèm cơm. Cân bằng dinh dưỡng, hương vị đặc biệt.", Calories = 420, Protein = 28.0m, Carbs = 45.0m, Fat = 14.0m, BasePrice = 110000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 50, Name = "Cơm Cá Chiên", Ingredients = "[\"Cá\",\"Bột chiên\",\"Trứng\",\"Gạo\",\"Rau sống\",\"Nước mắm\",\"Chanh\",\"Dầu ăn\"]", Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", Description = "Cá chiên giòn vàng, kèm cơm và rau sống. Cân bằng dinh dưỡng, giòn ngon.", Calories = 460, Protein = 30.0m, Carbs = 48.0m, Fat = 18.0m, BasePrice = 85000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 51, Name = "Cơm Thịt Heo Quay", Ingredients = "[\"Thịt heo quay\",\"Gạo\",\"Dưa chua\",\"Rau củ\",\"Nước mắm\",\"Tỏi\",\"Ớt\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Thịt heo quay giòn tan, kèm cơm và dưa chua. Cân bằng dinh dưỡng, hương vị đậm đà.", Calories = 500, Protein = 34.0m, Carbs = 46.0m, Fat = 22.0m, BasePrice = 95000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 52, Name = "Cơm Gà Xối Mỡ", Ingredients = "[\"Gà\",\"Mỡ\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Nước mắm\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Gà xối mỡ thơm lừng, kèm cơm và rau củ. Cân bằng dinh dưỡng, hương vị đặc trưng.", Calories = 480, Protein = 36.0m, Carbs = 44.0m, Fat = 20.0m, BasePrice = 92000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 53, Name = "Cơm Cá Lóc Kho Tộ", Ingredients = "[\"Cá lóc\",\"Nước mắm\",\"Đường\",\"Ớt\",\"Gừng\",\"Hành tây\",\"Gạo\",\"Rau củ\"]", Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", Description = "Cá lóc kho tộ đậm đà, kèm cơm. Cân bằng dinh dưỡng, hương vị truyền thống.", Calories = 390, Protein = 34.0m, Carbs = 38.0m, Fat = 13.0m, BasePrice = 90000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 54, Name = "Cơm Thịt Bò Nướng", Ingredients = "[\"Thịt bò\",\"Gia vị nướng\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Thịt bò nướng thơm lừng, kèm cơm và rau củ. Cân bằng dinh dưỡng, giàu protein.", Calories = 460, Protein = 40.0m, Carbs = 40.0m, Fat = 18.0m, BasePrice = 115000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 55, Name = "Cơm Gà Rán", Ingredients = "[\"Gà\",\"Bột chiên\",\"Gia vị\",\"Gạo\",\"Rau củ\",\"Dầu ăn\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Gà rán giòn vàng, kèm cơm và rau củ. Cân bằng dinh dưỡng, giòn ngon.", Calories = 520, Protein = 32.0m, Carbs = 50.0m, Fat = 22.0m, BasePrice = 88000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 56, Name = "Cơm Cá Basa Chiên", Ingredients = "[\"Cá basa\",\"Bột chiên\",\"Trứng\",\"Gạo\",\"Rau sống\",\"Nước mắm\",\"Dầu ăn\"]", Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", Description = "Cá basa chiên giòn, kèm cơm và rau sống. Cân bằng dinh dưỡng, giòn ngon.", Calories = 470, Protein = 28.0m, Carbs = 46.0m, Fat = 20.0m, BasePrice = 78000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 57, Name = "Cơm Thịt Heo Nướng", Ingredients = "[\"Thịt heo\",\"Gia vị nướng\",\"Mật ong\",\"Tỏi\",\"Gạo\",\"Rau củ\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Thịt heo nướng thơm lừng, kèm cơm và rau củ. Cân bằng dinh dưỡng, hương vị đậm đà.", Calories = 490, Protein = 33.0m, Carbs = 44.0m, Fat = 21.0m, BasePrice = 90000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 58, Name = "Cơm Gà Nấu Nấm", Ingredients = "[\"Gà\",\"Nấm\",\"Hành tây\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Nước dùng\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Gà nấu nấm thơm ngon, kèm cơm và rau củ. Cân bằng dinh dưỡng, bổ dưỡng.", Calories = 410, Protein = 34.0m, Carbs = 42.0m, Fat = 14.0m, BasePrice = 85000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 59, Name = "Cơm Cá Hấp Xì Dầu", Ingredients = "[\"Cá\",\"Xì dầu\",\"Gừng\",\"Hành lá\",\"Ớt\",\"Gạo\",\"Rau củ\",\"Dầu mè\"]", Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", Description = "Cá hấp xì dầu thơm ngon, kèm cơm và rau củ. Cân bằng dinh dưỡng, ít béo.", Calories = 370, Protein = 32.0m, Carbs = 36.0m, Fat = 11.0m, BasePrice = 88000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 60, Name = "Cơm Thịt Bò Kho", Ingredients = "[\"Thịt bò\",\"Cà rốt\",\"Hành tây\",\"Gừng\",\"Sả\",\"Nước dừa\",\"Gạo\",\"Rau thơm\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Thịt bò kho đậm đà, kèm cơm. Cân bằng dinh dưỡng, hương vị đặc trưng.", Calories = 480, Protein = 38.0m, Carbs = 46.0m, Fat = 17.0m, BasePrice = 110000, IsActive = true, CreatedAt = createdAt }
//            };
//        }

//        private static List<Meal> GetVeganMeals(DateTime createdAt)
//        {
//            return new List<Meal>
//            {
//                new Meal { Id = 61, Name = "Bowl Quinoa Rau Củ", Ingredients = "[\"Quinoa\",\"Bông cải xanh\",\"Cà rốt\",\"Ớt chuông\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Sốt tahini\",\"Chanh\"]", Images = "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", Description = "Bowl quinoa với rau củ tươi, sốt tahini. Món ăn vegan healthy, giàu protein thực vật và chất xơ.", Calories = 350, Protein = 12.0m, Carbs = 55.0m, Fat = 10.0m, BasePrice = 75000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 62, Name = "Salad Đậu Hũ Nướng", Ingredients = "[\"Đậu hũ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Ớt chuông\",\"Hành tây\",\"Dầu giấm\",\"Hạt hướng dương\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Salad đậu hũ nướng tươi ngon, giàu protein thực vật. Món ăn vegan, ít calo.", Calories = 280, Protein = 18.0m, Carbs = 20.0m, Fat = 14.0m, BasePrice = 68000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 63, Name = "Cơm Đậu Hũ Chiên", Ingredients = "[\"Đậu hũ\",\"Bột chiên\",\"Gạo\",\"Rau củ\",\"Nước tương\",\"Dầu ăn\",\"Hành lá\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Đậu hũ chiên giòn, kèm cơm và rau củ. Món ăn vegan, giàu protein thực vật.", Calories = 380, Protein = 16.0m, Carbs = 52.0m, Fat = 12.0m, BasePrice = 65000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 64, Name = "Bowl Đậu Lăng Rau Củ", Ingredients = "[\"Đậu lăng\",\"Cà rốt\",\"Cần tây\",\"Hành tây\",\"Cà chua\",\"Gia vị\",\"Rau thơm\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", Description = "Bowl đậu lăng với rau củ, giàu protein và chất xơ. Món ăn vegan bổ dưỡng.", Calories = 320, Protein = 20.0m, Carbs = 48.0m, Fat = 8.0m, BasePrice = 70000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 65, Name = "Salad Tempeh", Ingredients = "[\"Tempeh\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu giấm\",\"Hạt chia\",\"Rau thơm\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Salad tempeh tươi ngon, giàu protein thực vật. Món ăn vegan, ít calo.", Calories = 260, Protein = 22.0m, Carbs = 18.0m, Fat = 12.0m, BasePrice = 72000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 66, Name = "Cơm Đậu Hũ Sốt Cà Chua", Ingredients = "[\"Đậu hũ\",\"Cà chua\",\"Hành tây\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Đậu hũ sốt cà chua đậm đà, kèm cơm. Món ăn vegan, giàu protein thực vật.", Calories = 360, Protein = 15.0m, Carbs = 50.0m, Fat = 10.0m, BasePrice = 68000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 67, Name = "Bowl Đậu Gà Rau Củ", Ingredients = "[\"Đậu gà\",\"Bông cải xanh\",\"Cà rốt\",\"Ớt chuông\",\"Hành tây\",\"Sốt tahini\",\"Chanh\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", Description = "Bowl đậu gà với rau củ, giàu protein và chất xơ. Món ăn vegan bổ dưỡng.", Calories = 340, Protein = 18.0m, Carbs = 46.0m, Fat = 11.0m, BasePrice = 73000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 68, Name = "Salad Quả Bơ Đậu Hũ", Ingredients = "[\"Đậu hũ\",\"Quả bơ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu giấm\",\"Hạt hướng dương\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Salad quả bơ và đậu hũ, giàu chất béo tốt và protein thực vật. Món ăn vegan.", Calories = 300, Protein = 14.0m, Carbs = 22.0m, Fat = 16.0m, BasePrice = 75000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 69, Name = "Cơm Đậu Hũ Nướng", Ingredients = "[\"Đậu hũ\",\"Gia vị nướng\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Đậu hũ nướng thơm lừng, kèm cơm và rau củ. Món ăn vegan, giàu protein thực vật.", Calories = 370, Protein = 17.0m, Carbs = 48.0m, Fat = 13.0m, BasePrice = 70000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 70, Name = "Bowl Seitan Rau Củ", Ingredients = "[\"Seitan\",\"Bông cải xanh\",\"Cà rốt\",\"Ớt chuông\",\"Hành tây\",\"Sốt teriyaki\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", Description = "Bowl seitan với rau củ, giàu protein thực vật. Món ăn vegan, hương vị đậm đà.", Calories = 330, Protein = 25.0m, Carbs = 42.0m, Fat = 9.0m, BasePrice = 78000, IsActive = true, CreatedAt = createdAt }
//            };
//        }

//        private static List<Meal> GetVegetarianMeals(DateTime createdAt)
//        {
//            return new List<Meal>
//            {
//                new Meal { Id = 71, Name = "Cơm Trứng Chiên", Ingredients = "[\"Trứng\",\"Hành lá\",\"Gạo\",\"Rau củ\",\"Dầu ăn\",\"Nước mắm\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Trứng chiên thơm ngon, kèm cơm và rau củ. Món ăn vegetarian, giàu protein.", Calories = 380, Protein = 20.0m, Carbs = 45.0m, Fat = 14.0m, BasePrice = 60000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 72, Name = "Cơm Đậu Hũ Sốt Nấm", Ingredients = "[\"Đậu hũ\",\"Nấm\",\"Hành tây\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Nước tương\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Đậu hũ sốt nấm thơm ngon, kèm cơm. Món ăn vegetarian, giàu protein thực vật.", Calories = 350, Protein = 16.0m, Carbs = 48.0m, Fat = 11.0m, BasePrice = 68000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 73, Name = "Salad Trứng Quả Bơ", Ingredients = "[\"Trứng\",\"Quả bơ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu giấm\",\"Hạt chia\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Salad trứng và quả bơ, giàu protein và chất béo tốt. Món ăn vegetarian.", Calories = 320, Protein = 16.0m, Carbs = 20.0m, Fat = 18.0m, BasePrice = 72000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 74, Name = "Cơm Phô Mai Nướng", Ingredients = "[\"Phô mai\",\"Gạo\",\"Rau củ\",\"Bơ\",\"Tỏi\",\"Gia vị\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Cơm phô mai nướng thơm lừng, kèm rau củ. Món ăn vegetarian, giàu protein và canxi.", Calories = 420, Protein = 18.0m, Carbs = 50.0m, Fat = 16.0m, BasePrice = 75000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 75, Name = "Bowl Trứng Quinoa", Ingredients = "[\"Trứng\",\"Quinoa\",\"Bông cải xanh\",\"Cà rốt\",\"Ớt chuông\",\"Sốt tahini\",\"Chanh\"]", Images = "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", Description = "Bowl trứng và quinoa với rau củ, giàu protein. Món ăn vegetarian bổ dưỡng.", Calories = 360, Protein = 19.0m, Carbs = 44.0m, Fat = 13.0m, BasePrice = 78000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 76, Name = "Cơm Đậu Hũ Xào Rau Củ", Ingredients = "[\"Đậu hũ\",\"Cà rốt\",\"Ớt chuông\",\"Bông cải xanh\",\"Nấm\",\"Tỏi\",\"Gạo\",\"Nước tương\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Đậu hũ xào rau củ tươi ngon, kèm cơm. Món ăn vegetarian, giàu protein thực vật.", Calories = 340, Protein = 15.0m, Carbs = 46.0m, Fat = 10.0m, BasePrice = 70000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 77, Name = "Salad Trứng Đậu Hũ", Ingredients = "[\"Trứng\",\"Đậu hũ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Dầu giấm\",\"Hạt hướng dương\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Salad trứng và đậu hũ, giàu protein. Món ăn vegetarian, ít calo.", Calories = 290, Protein = 20.0m, Carbs = 18.0m, Fat = 15.0m, BasePrice = 72000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 78, Name = "Cơm Trứng Ốp La", Ingredients = "[\"Trứng\",\"Hành lá\",\"Gạo\",\"Rau củ\",\"Dầu ăn\",\"Nước mắm\",\"Tiêu\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Trứng ốp la thơm ngon, kèm cơm và rau củ. Món ăn vegetarian, giàu protein.", Calories = 370, Protein = 19.0m, Carbs = 44.0m, Fat = 15.0m, BasePrice = 62000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 79, Name = "Bowl Đậu Hũ Quinoa", Ingredients = "[\"Đậu hũ\",\"Quinoa\",\"Bông cải xanh\",\"Cà rốt\",\"Ớt chuông\",\"Sốt tahini\",\"Chanh\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", Description = "Bowl đậu hũ và quinoa với rau củ, giàu protein thực vật. Món ăn vegetarian.", Calories = 350, Protein = 17.0m, Carbs = 48.0m, Fat = 12.0m, BasePrice = 75000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 80, Name = "Cơm Đậu Hũ Chiên Xù", Ingredients = "[\"Đậu hũ\",\"Bột chiên xù\",\"Gạo\",\"Rau củ\",\"Nước tương\",\"Dầu ăn\",\"Hành lá\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Đậu hũ chiên xù giòn tan, kèm cơm và rau củ. Món ăn vegetarian, giòn ngon.", Calories = 390, Protein = 16.0m, Carbs = 50.0m, Fat = 14.0m, BasePrice = 70000, IsActive = true, CreatedAt = createdAt }
//            };
//        }

//        private static List<Meal> GetLowCarbMeals(DateTime createdAt)
//        {
//            return new List<Meal>
//            {
//                new Meal { Id = 81, Name = "Salad Gà Keto", Ingredients = "[\"Ức gà\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Quả bơ\",\"Dầu oliu\",\"Chanh\",\"Hạt chia\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Salad gà keto, ít carb, giàu protein và chất béo tốt. Phù hợp cho chế độ low-carb/keto.", Calories = 320, Protein = 35.0m, Carbs = 8.0m, Fat = 18.0m, BasePrice = 85000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 82, Name = "Cá Hồi Rau Củ Keto", Ingredients = "[\"Cá hồi\",\"Bông cải xanh\",\"Cà rốt\",\"Bơ\",\"Dầu oliu\",\"Chanh\",\"Gia vị\"]", Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", Description = "Cá hồi với rau củ keto, ít carb, giàu Omega-3. Phù hợp cho chế độ low-carb/keto.", Calories = 380, Protein = 36.0m, Carbs = 10.0m, Fat = 24.0m, BasePrice = 120000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 83, Name = "Thịt Bò Xào Rau Củ Keto", Ingredients = "[\"Thịt bò\",\"Bông cải xanh\",\"Ớt chuông\",\"Nấm\",\"Tỏi\",\"Dầu oliu\",\"Gia vị\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Thịt bò xào rau củ keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", Calories = 350, Protein = 38.0m, Carbs = 12.0m, Fat = 18.0m, BasePrice = 98000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 84, Name = "Gà Nướng Rau Củ Keto", Ingredients = "[\"Ức gà\",\"Bông cải xanh\",\"Cà rốt\",\"Bơ\",\"Dầu oliu\",\"Gia vị nướng\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Gà nướng với rau củ keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", Calories = 330, Protein = 40.0m, Carbs = 9.0m, Fat = 16.0m, BasePrice = 88000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 85, Name = "Salad Cá Ngừ Keto", Ingredients = "[\"Cá ngừ\",\"Xà lách\",\"Quả bơ\",\"Dưa chuột\",\"Dầu oliu\",\"Chanh\",\"Hạt chia\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Salad cá ngừ keto, ít carb, giàu protein và Omega-3. Phù hợp cho chế độ low-carb/keto.", Calories = 300, Protein = 32.0m, Carbs = 7.0m, Fat = 16.0m, BasePrice = 95000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 86, Name = "Thịt Bò Nướng Rau Củ Keto", Ingredients = "[\"Thịt bò\",\"Bông cải xanh\",\"Cà rốt\",\"Bơ\",\"Dầu oliu\",\"Gia vị nướng\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Thịt bò nướng với rau củ keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", Calories = 360, Protein = 39.0m, Carbs = 11.0m, Fat = 19.0m, BasePrice = 105000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 87, Name = "Cá Hấp Rau Củ Keto", Ingredients = "[\"Cá\",\"Bông cải xanh\",\"Cà rốt\",\"Bơ\",\"Dầu oliu\",\"Gừng\",\"Chanh\"]", Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", Description = "Cá hấp với rau củ keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", Calories = 310, Protein = 34.0m, Carbs = 8.0m, Fat = 15.0m, BasePrice = 90000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 88, Name = "Gà Xào Rau Củ Keto", Ingredients = "[\"Ức gà\",\"Bông cải xanh\",\"Ớt chuông\",\"Nấm\",\"Tỏi\",\"Dầu oliu\",\"Gia vị\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Gà xào rau củ keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", Calories = 320, Protein = 36.0m, Carbs = 10.0m, Fat = 14.0m, BasePrice = 82000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 89, Name = "Salad Tôm Keto", Ingredients = "[\"Tôm\",\"Xà lách\",\"Quả bơ\",\"Dưa chuột\",\"Dầu oliu\",\"Chanh\",\"Hạt chia\"]", Images = "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]", Description = "Salad tôm keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", Calories = 280, Protein = 28.0m, Carbs = 6.0m, Fat = 14.0m, BasePrice = 110000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 90, Name = "Thịt Bò Xào Nấm Keto", Ingredients = "[\"Thịt bò\",\"Nấm\",\"Bông cải xanh\",\"Tỏi\",\"Dầu oliu\",\"Gia vị\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Thịt bò xào nấm keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", Calories = 340, Protein = 37.0m, Carbs = 9.0m, Fat = 17.0m, BasePrice = 100000, IsActive = true, CreatedAt = createdAt }
//            };
//        }

//        private static List<Meal> GetHalalMeals(DateTime createdAt)
//        {
//            return new List<Meal>
//            {
//                new Meal { Id = 91, Name = "Cơm Gà Halal", Ingredients = "[\"Gà halal\",\"Gạo\",\"Rau củ\",\"Gia vị halal\",\"Dầu oliu\",\"Hành tây\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Cơm gà halal thơm ngon, kèm rau củ. Món ăn halal, giàu protein.", Calories = 420, Protein = 38.0m, Carbs = 44.0m, Fat = 14.0m, BasePrice = 88000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 92, Name = "Cơm Thịt Bò Halal", Ingredients = "[\"Thịt bò halal\",\"Gạo\",\"Rau củ\",\"Gia vị halal\",\"Dầu oliu\",\"Tỏi\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Cơm thịt bò halal đậm đà, kèm rau củ. Món ăn halal, giàu protein và sắt.", Calories = 460, Protein = 40.0m, Carbs = 42.0m, Fat = 18.0m, BasePrice = 115000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 93, Name = "Cơm Gà Nướng Halal", Ingredients = "[\"Gà halal\",\"Gia vị nướng halal\",\"Gạo\",\"Rau củ\",\"Dầu oliu\",\"Chanh\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Gà nướng halal thơm lừng, kèm cơm và rau củ. Món ăn halal, giàu protein.", Calories = 440, Protein = 39.0m, Carbs = 40.0m, Fat = 16.0m, BasePrice = 92000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 94, Name = "Cơm Thịt Cừu Halal", Ingredients = "[\"Thịt cừu halal\",\"Gạo\",\"Rau củ\",\"Gia vị halal\",\"Dầu oliu\",\"Hành tây\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Cơm thịt cừu halal đậm đà, kèm rau củ. Món ăn halal, giàu protein.", Calories = 480, Protein = 42.0m, Carbs = 38.0m, Fat = 22.0m, BasePrice = 125000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 95, Name = "Cơm Gà Xào Halal", Ingredients = "[\"Gà halal\",\"Hành tây\",\"Ớt chuông\",\"Nấm\",\"Gạo\",\"Gia vị halal\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Gà xào halal thơm ngon, kèm cơm. Món ăn halal, giàu protein.", Calories = 400, Protein = 36.0m, Carbs = 38.0m, Fat = 15.0m, BasePrice = 85000, IsActive = true, CreatedAt = createdAt }
//            };
//        }

//        private static List<Meal> GetGlutenFreeMeals(DateTime createdAt)
//        {
//            return new List<Meal>
//            {
//                new Meal { Id = 96, Name = "Cơm Gạo Lứt Gà Nướng", Ingredients = "[\"Gà\",\"Gạo lứt\",\"Rau củ\",\"Dầu oliu\",\"Gia vị\",\"Chanh\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Cơm gạo lứt với gà nướng, không chứa gluten. Món ăn gluten-free, giàu chất xơ.", Calories = 410, Protein = 37.0m, Carbs = 46.0m, Fat = 13.0m, BasePrice = 88000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 97, Name = "Bowl Quinoa Cá Hồi", Ingredients = "[\"Cá hồi\",\"Quinoa\",\"Bông cải xanh\",\"Cà rốt\",\"Dầu oliu\",\"Chanh\",\"Gia vị\"]", Images = "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", Description = "Bowl quinoa với cá hồi, không chứa gluten. Món ăn gluten-free, giàu Omega-3.", Calories = 390, Protein = 34.0m, Carbs = 40.0m, Fat = 16.0m, BasePrice = 118000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 98, Name = "Cơm Gạo Lứt Thịt Bò", Ingredients = "[\"Thịt bò\",\"Gạo lứt\",\"Rau củ\",\"Dầu oliu\",\"Gia vị\",\"Tỏi\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Cơm gạo lứt với thịt bò, không chứa gluten. Món ăn gluten-free, giàu protein.", Calories = 450, Protein = 38.0m, Carbs = 44.0m, Fat = 17.0m, BasePrice = 108000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 99, Name = "Bowl Quinoa Gà", Ingredients = "[\"Gà\",\"Quinoa\",\"Bông cải xanh\",\"Cà rốt\",\"Ớt chuông\",\"Dầu oliu\",\"Gia vị\"]", Images = "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", Description = "Bowl quinoa với gà, không chứa gluten. Món ăn gluten-free, giàu protein và chất xơ.", Calories = 400, Protein = 36.0m, Carbs = 42.0m, Fat = 14.0m, BasePrice = 92000, IsActive = true, CreatedAt = createdAt },
//                new Meal { Id = 100, Name = "Cơm Gạo Lứt Cá Nướng", Ingredients = "[\"Cá\",\"Gạo lứt\",\"Rau củ\",\"Dầu oliu\",\"Chanh\",\"Gia vị\"]", Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", Description = "Cơm gạo lứt với cá nướng, không chứa gluten. Món ăn gluten-free, giàu protein.", Calories = 380, Protein = 32.0m, Carbs = 40.0m, Fat = 12.0m, BasePrice = 90000, IsActive = true, CreatedAt = createdAt }
//            };
//        }
//    }
//}
