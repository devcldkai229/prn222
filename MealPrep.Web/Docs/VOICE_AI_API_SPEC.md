# Voice AI API Specification - /api/analyze-voice

Để **nguyên liệu có số lượng** (200g ức gà, 15ml mật ong) và **Calories/Protein/Carbs/Fat chính xác**, service Voice AI cần trả về JSON theo format sau.

## Endpoint

`POST {BaseUrl}/api/analyze-voice`

- **Content-Type**: `multipart/form-data`
- **Body**: `file` (audio webm/mp3)

## Response Format

### 1. Cấu trúc nguyên liệu (quan trọng - phải có số lượng)

**Cách 1 – Chuỗi đã format (ưu tiên):**
```json
{
  "ingredients_raw": "200g ức gà, 15ml mật ong, 50g bông cải xanh"
}
```

**Cách 2 – Mảng với quantity + unit:**
```json
{
  "ingredients": [
    { "name": "ức gà", "quantity": 200, "unit": "g" },
    { "name": "mật ong", "quantity": 15, "unit": "ml" },
    { "name": "bông cải xanh", "quantity": 50, "unit": "g" }
  ]
}
```

- `quantity`: số (number)
- `unit`: `g`, `gram`, `ml`, `kg`, v.v.

**Không nên dùng** mảng chỉ có `name` (thiếu số lượng):
```json
{ "ingredients": [{ "name": "Ức gà" }, { "name": "Mật ong" }] }
```

### 2. Dinh dưỡng (bắt buộc tính theo số lượng)

```json
{
  "total_nutrition": {
    "calories": 280,
    "protein_g": 42.5,
    "carbs_g": 12.3,
    "fat_g": 8.1
  }
}
```

- **Calories, protein, carbs, fat** phải được tính từ **số lượng thực tế**:
  - Ví dụ: 200g ức gà ≈ 330 kcal, 62g protein
  - 15ml mật ong ≈ 46 kcal
  - 50g bông cải xanh ≈ 17 kcal
  - Tổng ≈ 393 kcal

- Nên dùng bảng dinh dưỡng theo 100g/100ml (USDA hoặc tương đương).

### 3. Ví dụ response đầy đủ

```json
{
  "dish_name": "Ức gà nướng mật ong",
  "description": "Ức gà ướp mật ong nướng thơm, kèm bông cải xanh.",
  "ingredients_raw": "200g ức gà, 15ml mật ong, 50g bông cải xanh",
  "total_nutrition": {
    "calories": 393,
    "protein_g": 44.2,
    "carbs_g": 15.8,
    "fat_g": 8.5
  }
}
```

Hoặc dùng mảng:

```json
{
  "dish_name": "Ức gà nướng mật ong",
  "description": "Ức gà ướp mật ong nướng thơm, kèm bông cải xanh.",
  "ingredients": [
    { "name": "ức gà", "quantity": 200, "unit": "g" },
    { "name": "mật ong", "quantity": 15, "unit": "ml" },
    { "name": "bông cải xanh", "quantity": 50, "unit": "g" }
  ],
  "total_nutrition": {
    "calories": 393,
    "protein_g": 44.2,
    "carbs_g": 15.8,
    "fat_g": 8.5
  }
}
```

## Yêu cầu độ chính xác

1. **Transcribe**: Giữ nguyên số lượng và đơn vị ("200 gram", "15 ml", "50g").
2. **Parse**: Extract `quantity` và `unit` rõ ràng.
3. **Nutrition**: Tính `total_nutrition` từ từng nguyên liệu dựa trên số lượng.
4. **Unit chuẩn hóa**: `g`, `gram`, `ml`, `kg`, `l`, v.v. để MealPrep hiển thị nhất quán.
