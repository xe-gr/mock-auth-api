using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using mockAuth.Dto;

namespace mockAuth.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class PaymentController : ControllerBase
	{
		private ILogger Logger { get; set; }

		public PaymentController (ILogger<PaymentController> logger)
		{
			Logger = logger;
		}

		[HttpPost]
		public IActionResult Post([FromBody] PaymentDto dto)
		{
			if (dto == null)
			{
				Logger.LogInformation("No DTO");
				return BadRequest("No dto specified");
			}

			Logger.LogInformation($"Card {dto.CardNumber}, MM/DD {dto.ExpiryMonth}/{dto.ExpiryYear}");

			if (string.IsNullOrEmpty(dto.CardNumber)
				|| string.IsNullOrEmpty(dto.ExpiryMonth)
				|| string.IsNullOrEmpty(dto.ExpiryYear)
				|| dto.CardNumber.Length != 16
				|| dto.ExpiryMonth.Length != 2
				|| dto.ExpiryYear.Length != 4)
			{
				return BadRequest("Card number, expiry month and expiry year must all have values. Card number should be 16 digits long, expiry month 2 and expiry year 4");
			}

			if (!Luhn(dto.CardNumber))
			{
				return BadRequest("Card is not valid");
			}

			if (!IsNumeric(dto.CardNumber) || !IsNumeric(dto.ExpiryMonth) || !IsNumeric(dto.ExpiryYear))
			{
				return BadRequest("Card number, expiry month and expiry year must be numeric");
			}

			if (Convert(dto.ExpiryMonth) <= 0 || Convert(dto.ExpiryMonth) > 12)
			{
				return BadRequest("Expiry month must be between 01 and 12");
			}

			if (Convert(dto.ExpiryYear) < DateTime.UtcNow.Year)
			{
				return BadRequest("Expiry year has lapsed");
			}

			if (dto.ExpiryYear == "2041")
			{
				var result = new ObjectResult("Card could not be charged")
				{
					StatusCode = 422
				};
				return result;
			}

			return Ok("Transaction completed.");
		}

		private bool Luhn(string digits)
		{
			return digits.All(char.IsDigit) && digits.Reverse()
				.Select(c => c - 48)
				.Select((thisNum, i) => i % 2 == 0
					? thisNum
					: ((thisNum *= 2) > 9 ? thisNum - 9 : thisNum)
				).Sum() % 10 == 0;
		}

		private bool IsNumeric(string s)
		{
			return decimal.TryParse(s, out _);
		}

		private int Convert (string s)
		{
			Int32.TryParse(s, out var output);
			return output;
		}
	}
}
