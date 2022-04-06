using PrayerTimes.Library.Calculators;
using PrayerTimes.Library.Models;
#pragma warning disable CS1591

namespace PrayerTimes.Api.Models;

public class PrayerTimesResultModel
{
    public Geocoordinate? Location { get; set; }
    public DateTime? PrayerTimesForDate { get; set; }
    public string? Imsaak { get; set; }
    public string? Fajr { get; set; }
    public string? Sunrise { get; set; }
    public string? Dhuhr { get; set; }
    public string? Asr { get; set; }
    public string? Sunset { get; set; }
    public string? Maghreb { get; set; }
    public string? Isha { get; set; }
    public string? Midnight { get; set; }
}