using Lyke.Core.Enums;

namespace Lyke.Application.Helpers;

public static class BodyProfileHelper
{
    public static string? FormatBodyTypeLabel(Stature? stature, Build? build, string bodyTypeName)
    {
        if (stature == null && build == null)
        {
            return null;
        }

        var parts = new List<string>();
        if (stature.HasValue)
        {
            parts.Add(stature.Value.ToString());
        }
        if (build.HasValue && build.Value != Build.Standard)
        {
            parts.Add(build.Value.ToString());
        }
        parts.Add(bodyTypeName);

        return string.Join(" ", parts);
    }

    public static string GetHeightRange(int heightCm)
    {
        return heightCm switch
        {
            < 155 => "Under 155cm (5'1\")",
            < 160 => "155-159cm (5'1\"-5'2\")",
            < 165 => "160-164cm (5'3\"-5'4\")",
            < 170 => "165-169cm (5'5\"-5'6\")",
            < 175 => "170-174cm (5'7\"-5'8\")",
            < 180 => "175-179cm (5'9\"-5'10\")",
            < 185 => "180-184cm (5'11\"-6'0\")",
            < 190 => "185-189cm (6'1\"-6'2\")",
            _ => "190cm+ (6'3\"+)",
        };
    }

    public static string GetWeightRange(decimal weightKg)
    {
        return weightKg switch
        {
            < 50 => "Under 50kg (110lbs)",
            < 55 => "50-54kg (110-121lbs)",
            < 60 => "55-59kg (121-130lbs)",
            < 65 => "60-64kg (132-143lbs)",
            < 70 => "65-69kg (143-152lbs)",
            < 75 => "70-74kg (154-163lbs)",
            < 80 => "75-79kg (165-174lbs)",
            < 85 => "80-84kg (176-185lbs)",
            < 90 => "85-89kg (187-196lbs)",
            < 95 => "90-94kg (198-207lbs)",
            < 100 => "95-99kg (209-218lbs)",
            _ => "100kg+ (220lbs+)",
        };
    }
}
