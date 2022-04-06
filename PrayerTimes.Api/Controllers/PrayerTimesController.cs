using System.Globalization;
using System.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using PrayerTimes.Api.Models;
using PrayerTimes.Library.Calculators;
using PrayerTimes.Library.Enumerations;
using PrayerTimes.Library.Helpers;
using PrayerTimes.Library.Models;

namespace PrayerTimes.Api.Controllers
{
    /// <summary>
    /// Prayer Times Controller - Provides methods to calculate the Prayer Times
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PrayerTimesController : ControllerBase
    {
        /// <summary>
        /// Calculate Prayer Times.
        /// </summary>
        /// <param name="latitude">The latitude (as a double) (e.g. 51.52914341845893)</param>
        /// <param name="longitude">The longitude (as a double) (e.g. -0.18896143561607293)</param>
        /// <param name="altitude">The altitude (as a double) (e.g. 27.23452345), can be 0.0 if unknown or unable to be detected</param>
        /// <param name="gregorianDate">The date for which prayer times need to be calculated in  ISO 8601 format (e.g. 2022-04-06T21:02:28Z (YYYY-MM-DDTHH:MM:SSZZ))</param>
        /// <param name="calculationMethod">The required calculation method see the enum definition for CalculationMethodPreset below</param>
        /// <param name="timeZone">The timezone (as a double) (e.g. 0.0 or 1.5 - if the time difference from UTC is 1 hour 30 minutes, then this would be 1.5 NOT 1.3)</param>
        /// <param name="isDaylightSavings">Boolean value indicating whether day light savings time is in effect for the given date and timezone</param> 
        /// <returns>Returns the prayer times for the given date and parameters</returns>
        /// <remarks>
        /// Sample request:
        ///     GET /api/PrayerTimes/Calculate/51.52914341845893/-0.18896143561607293/27.23452345/2022-04-06T21%3A02%3A28Z/IthnaAshari/0.0/true
        /// </remarks>
        /// <response code="201">Returns the calculated prayer times</response>
        /// <response code="500">Server error returned when calculation of the prayer times is impossible with the provided parameters</response>
        [HttpGet(
            "Calculate/{latitude}/{longitude}/{altitude}/{gregorianDate}/{calculationMethod}/{timeZone}/{isDaylightSavings}")]

        public IActionResult CalculatePrayerTimes(double latitude,
            double longitude,
            double altitude,
            string gregorianDate,
            CalculationMethodPreset calculationMethod,
            double timeZone,
            bool isDaylightSavings)
        {
            try
            {
                var chosenDate = DateTime.Parse(gregorianDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                if (isDaylightSavings)
                {
                    timeZone += 1.0;
                }
                var when = Instant.FromDateTimeUtc(chosenDate.ToUniversalTime());
                var settings = new PrayerCalculationSettings();
                settings.CalculationMethod.SetCalculationMethodPreset(when, calculationMethod);
                var geo = new Geocoordinate(latitude, longitude, altitude);
                var calculatedTimes = Prayers.On(when, settings, geo, timeZone);

                var prayerTimesResult = new PrayerTimesResultModel
                {
                    Location = geo,
                    PrayerTimesForDate = chosenDate,
                    Imsaak = GetPrayerTimeString(calculatedTimes.Imsak, timeZone),
                    Fajr = GetPrayerTimeString(calculatedTimes.Fajr, timeZone),
                    Sunrise = GetPrayerTimeString(calculatedTimes.Sunrise, timeZone),
                    Dhuhr = GetPrayerTimeString(calculatedTimes.Dhuhr, timeZone),
                    Asr = GetPrayerTimeString(calculatedTimes.Asr, timeZone),
                    Sunset = GetPrayerTimeString(calculatedTimes.Sunset, timeZone),
                    Maghreb = GetPrayerTimeString(calculatedTimes.Maghrib, timeZone),
                    Isha = GetPrayerTimeString(calculatedTimes.Isha, timeZone),
                    Midnight = GetPrayerTimeString(calculatedTimes.Midnight, timeZone),
                };

                return new OkObjectResult(prayerTimesResult);
            }
            catch (Exception e)
            {
                throw new HttpRequestException("Fatal error, unable to calculate prayer times with the provided values",
                    e);
            }
        }

        private static string GetPrayerTimeString(Instant instant, double timeZone)
        {
            var zoned = instant.InZone(DateTimeZone.ForOffset(Offset.FromTimeSpan(TimeSpan.FromHours(timeZone))));
            return zoned.ToString("HH:mm", CultureInfo.InvariantCulture);
        }
    }
}
