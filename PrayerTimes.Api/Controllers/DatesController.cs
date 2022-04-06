using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using PrayerTimes.Api.Exceptions;
using prayertimescore.PrayerTimes.Library.Calender;
using Calendar = prayertimescore.PrayerTimes.Library.Calender.Calendar;

namespace PrayerTimes.Api.Controllers
{
    /// <summary>
    /// Dates Controller - Provides methods to convert to and from Gregorian, Solar and Lunar Hijri dates
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DatesController : ControllerBase
    {
        /// <summary>
        /// Converts Gregorian Date to Solar Hijri.
        /// </summary>
        /// <param name="gregorianDateToConvert">Date to convert in  ISO 8601 format (e.g. 2022-04-06T21:02:28Z (YYYY-MM-DDTHH:MM:SSZZ))</param>
        /// <param name="shortForm">Set to true if you want the short form of the solar hijri date (dd/mm/yyyy).  Default is false</param> 
        /// <returns>Returns the Solar Hijri date for the provided Gregorian Date as a string</returns>
        /// <remarks>
        /// Sample request:
        ///     GET /api/Dates/FromGregorianToSolarHijri/2022-04-06T21:02:28Z/false
        /// </remarks>
        /// <response code="201">Returns the converted date in the format specified</response>
        /// <response code="400">If an error occours converting the gregorian date to solar hijri</response>
        [HttpGet("FromGregorianToSolarHijri/{gregorianDateToConvert}/{shortForm}")]

        public IActionResult GetSolarHijriDate(string gregorianDateToConvert, bool shortForm = false)
        {
            try
            {
                var chosenDate = DateTime.Parse(gregorianDateToConvert, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                var when = Instant.FromDateTimeUtc(chosenDate);
                var solarDate = Calendar.ConvertToPersian(when.ToDateTimeUtc());
                return shortForm ?
                    new OkObjectResult($"{solarDate.ArrayType[2]}/{solarDate.ArrayType[1]}/{solarDate.ArrayType[0]}") :
                    new OkObjectResult($"{solarDate.ToString("english_day")} {solarDate.ToString("english_month")} {solarDate.ToString("english_year")}");
            }
            catch (Exception)
            {
                return BadRequest($"Unable to convert the Gregorian date {gregorianDateToConvert} to Solar Hijri format, the provided Gregorian date is invalid.");
            }
        }

        /// <summary>
        /// Converts Gregorian Date to Lunar Hijri.
        /// </summary>
        /// <param name="gregorianDateToConvert">Date to convert in  ISO 8601 format (e.g. 2022-04-06T21:02:28Z (YYYY-MM-DDTHH:MM:SSZZ))</param>
        /// <param name="lunarHijriOffset">This is the lunar hijri offset, it can be either positive or negative between -5 and 5</param>
        /// <param name="shortForm">Set to true if you want the short form of the solar hijri date (dd/mm/yyyy).  Default is false</param> 
        /// <returns>Returns the Lunar Hijri date for the provided Gregorian Date as a string</returns>
        /// <remarks>
        /// Sample request:
        ///     GET /api/Dates/FromGregorianToLunarHijri/2022-04-06T21:02:28Z/-1/false
        /// </remarks>
        /// <response code="201">Returns the converted date in the format specified</response>
        /// <response code="400">If an error occours converting the gregorian date to lunar hijri</response>
        [HttpGet("FromGregorianToLunarHijri/{gregorianDateToConvert}/{lunarHijriOffset}/{shortForm}")]
        public IActionResult GetLunarHijriDate(string gregorianDateToConvert, int lunarHijriOffset, bool shortForm = false)
        {
            try
            {
                if (lunarHijriOffset is > 5 or < -5)
                {
                    throw new DateTimeConversionException();
                }
                var chosenDate = DateTime.Parse(gregorianDateToConvert, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                var when = Instant.FromDateTimeUtc(chosenDate);
                var lunarDate = Calendar.ConvertToIslamic(when.ToDateTimeUtc().AddDays(lunarHijriOffset));

                return shortForm
                    ? new OkObjectResult($"{lunarDate.ArrayType[2]}/{lunarDate.ArrayType[1]}/{lunarDate.ArrayType[0]}")
                    : new OkObjectResult(
                        $"{lunarDate.ToString("english_day")} {lunarDate.ToString("english_month")} {lunarDate.ToString("english_year")}");
            }
            catch (DateTimeConversionException)
            {
                return BadRequest($"Unable to convert the Gregorian date {gregorianDateToConvert} to Lunar Hijri format, the provided offset value {lunarHijriOffset} is out of bounds.");
            }
            catch (Exception)
            {
                return BadRequest($"Unable to convert the Gregorian date {gregorianDateToConvert} to Lunar Hijri format, the provided Gregorian date is invalid.");
            }
        }
    }
}
