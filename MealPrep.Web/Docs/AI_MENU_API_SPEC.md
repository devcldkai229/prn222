# AI Generate Menu API Specification

API: `POST {AiSettings:ServiceUrl}` (vd: `http://localhost:8000/api/generate-menu`)

## Request Body (JSON)

```json
{
  "user_profile": {
    "height_cm": 170,
    "weight_kg": 70,
    "goal": 1,
    "activity_level": 3,
    "meals_per_day": 2,
    "notes": "...",
    "diet_pref": 0,
    "calories_in_day": 2000,
    "allergies": ["đậu phộng"]
  },
  "disliked_ids": [5, 12],
  "number_of_days": 5,
  "weekly_preference": "Tôi muốn ăn món có rau, nhiều thịt bò"
}
```

## Quan trọng: Sử dụng `weekly_preference`

**`weekly_preference`** là ưu tiên cao nhất khi đề xuất món. Đây là nội dung user nhập trong ô "Bạn muốn ăn thế nào" trên giao diện Chọn món ăn.

### Yêu cầu

1. **BẮT BUỘC** đưa `weekly_preference` vào prompt LLM như instruction chính, ví dụ:
   ```
   YÊU CẦU CỦA USER CHO TUẦN NÀY: "{weekly_preference}"
   Bạn PHẢI ưu tiên chọn các món phù hợp với yêu cầu trên. Không chọn món trái với yêu cầu.
   ```

2. Ví dụ: `weekly_preference = "Tôi muốn ăn món có rau, nhiều thịt bò"`  
   → Chọn món có rau, ưu tiên thịt bò, **tránh** thịt gà (trừ khi user yêu cầu).

3. Nếu `weekly_preference` rỗng, dùng `user_profile.notes` làm fallback.

### Response

```json
[
  { "day": 1, "meal_ids": [3, 7, 12], "reason": "..." },
  { "day": 2, "meal_ids": [5, 9], "reason": "..." }
]
```

- `day`: 1–7 (thứ trong tuần) hoặc index 0-based tùy implementation
- `meal_ids`: danh sách meal ID đề xuất cho ngày đó
