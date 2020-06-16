using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Text;

using CSS.Connector.FileProcessing.Core;

using Newtonsoft.Json;

namespace CSS.Connector.FileProcessors.Vanguard
{
	public class MovementServiceHelper
	{
		/*
		static ClientTokenGrantHelper _tokenProxy;
		ClientTokenGrantHelper TokenProxy
		{
			get
			{
				if (_tokenProxy == null) _tokenProxy = new ClientTokenGrantHelper(GetIdentityURL(), "TopAnalytics", "TopAdvisor");
				return _tokenProxy;
			}
		}
		*/

		public DateTime GetNextSettlementDate(DateTime currentDate)
		{
			string path = string.Format("/MovementsService/api/v1/Movements/NextSettlementDate?currentDate={0}", currentDate);
			string nextSettlementDateString = null;
			PrepareAndSendMessage(path, string.Empty, false, out nextSettlementDateString);
			nextSettlementDateString = Regex.Match(nextSettlementDateString, @"\d{4}-\d\d-\d\d").Captures[0].ToString();
			return DateTime.ParseExact(nextSettlementDateString, "yyyy-MM-dd", null);
		}

		public void PostExpectedActivity(ExpectedActivity expectedActivity)
		{
			string path = string.Format("/MovementsService/api/v1/Movements/Cash/ForecastActivity");
			string body = Serialize(expectedActivity);
			PrepareAndSendMessage(path, body);
		}

		public void PostExpectedBalance(decimal amount, string trailer, string transactionType, string accountNumber = null, string clearingId = null, DateTime? settlementDate = null)
		{
			string path = string.Format("/MovementsService/api/v1/Movements/Cash/Balance");
			// GroupId of "FOR" is for forecast (as opposed to "ACT" for actual)
			var expectedBalance = new { AccountNumber = accountNumber, Amount = amount, Trailer = trailer, TransactionType = transactionType, ClearingNumber = clearingId, GroupId = "FOR", SettlementDate = settlementDate};
			string body = Serialize(expectedBalance);
			PrepareAndSendMessage(path, body);
		}

		internal void PostBankLoan(BankLoan bankLoan)
		{
			string path = string.Format("/MovementsService/api/v1/Movements/BankLoan");
			string body = Serialize(bankLoan);
			PrepareAndSendMessage(path, body);
		}

		internal void PostBetaReport1Totals(DateTime processDate, decimal longMarketValue, decimal shortMarketValue)
		{
			string path = string.Format("/MovementsService/api/v1/Movements/EndOfDayTotals");
			var endOfDayTotals = new[] {
				new { ProcessDate = processDate, Code = "LMV", Amount = longMarketValue },
				new { ProcessDate = processDate, Code = "SMV", Amount = shortMarketValue } };
			string body = Serialize(endOfDayTotals);
			PrepareAndSendMessage(path, body);
		}

		internal void PostBetaReport2Totals(
			DateTime processDate,
			decimal failsToDeliver,
			decimal stockBorrow,
			decimal failsToReceive,
			decimal stockLoan)
		{
			string path = string.Format("/MovementsService/api/v1/Movements/EndOfDayTotals");
			var endOfDayTotals = new[] {
				new { ProcessDate = processDate, Code = "FTD", Amount = failsToDeliver },
				new { ProcessDate = processDate, Code = "SB" , Amount = stockBorrow },
				new { ProcessDate = processDate, Code = "FTR", Amount = failsToReceive },
				new { ProcessDate = processDate, Code = "SL" , Amount = stockLoan } };
			string body = Serialize(endOfDayTotals);
			PrepareAndSendMessage(path, body);
		}

		internal void PostForecastStockAndCash(BaseMovement movement, bool isCashOnly = false)
		{
			string path = string.Format("/MovementsService/api/v1/Movements/" + (isCashOnly ? "" : "StockAnd") + "Cash/ForecastActivity");
			string body = Serialize(movement);
			PrepareAndSendMessage(path, body);
		}

		internal void PostReinvestedDividend(
			DateTime payableDate,
			DateTime businessDate,
			string cusip,
			decimal  reinvestedDividendAmount,
			bool     isUnknownDate)
		{
			string path = string.Format("/MovementsService/api/v1/Movements/ReinvestedDividend");
			var reinvestedDividend = new { PayableDate = payableDate, BusinessDate = businessDate, Cusip = cusip, ReinvestedDividendAmount = reinvestedDividendAmount, DividendCode = (isUnknownDate ? "UKD" : null) };
			string body = Serialize(reinvestedDividend);
			PrepareAndSendMessage(path, body);
		}

		private string Serialize(object objectToSerialize)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringWriter stringWriter = new StringWriter(stringBuilder);
			JsonWriter writer = new JsonTextWriter(stringWriter);
			(new JsonSerializer()).Serialize(writer, objectToSerialize);
			return stringBuilder.ToString();
		}

		private HttpResponseMessage PrepareAndSendMessage(string path, string body)
		{
			string responseString = null;
			return PrepareAndSendMessage(path, body, true, out responseString);
		}

		private HttpResponseMessage PrepareAndSendMessage(string path, string body, bool usePost, out string responseString)
		{
			HttpResponseMessage responseMessage = null; /* = SendMessage(
				new Uri(GetMovementServiceHostAddress() + path),
				new HttpClient(),
				new StringContent(body, Encoding.UTF8, "application/json"),
				TokenProxy.Token.Replace("Bearer ", ""),
				body,
				usePost,
				out responseString
				);*/
			responseString = string.Empty;

			if (!responseMessage.IsSuccessStatusCode)
			{
				throw new Exception(string.Format("Error occurred while calling Movements Service: {0}. Check the Error table for more details. {1}", path, responseMessage.ToString()));
			}

			return responseMessage;
		}

		private HttpResponseMessage SendMessage(Uri serviceUri, HttpClient client, HttpContent httpContent, string token, string body, bool usePost, out string responseString)
		{
			responseString = null;
			HttpResponseMessage responseMessage = null;
			bool shouldRetry = false;
			int tryCount = 0;
			do
			{
				try
				{
					shouldRetry = false;
					using (var requestMessage = new HttpRequestMessage((usePost ? HttpMethod.Post : HttpMethod.Get), serviceUri))
					{
						requestMessage.Content = httpContent;
						requestMessage.Headers.Add("Authorization", "Bearer " + token);
						//responseMessage = client.SendAsync(requestMessage).Result;
						using (responseMessage = client.SendAsync(requestMessage).Result)
						{
							using (HttpContent content = responseMessage.Content)
							{
								responseString = content.ReadAsStringAsync().Result;
							}
						}
					}
				}
				catch (Exception e)
				{
					(new LoggingHelper()).Log("VG::SendMessage", string.Format("Error calling SendAsync.  Try number: {0}. Message: {1}. Error: {2}", ++tryCount, body, e.ToString()), 1, true);
					if (tryCount < 5) shouldRetry = true;
					else throw;
				}
			} while (shouldRetry == true);
			return responseMessage;
		}

		private static string GetIdentityURL()
		{
#if DEBUG
			return "https://Top-Appdev.Talisystech.com:8885";   // for AppDev testing of Movements service
#else
            return CSS.Cloud.Framework.ServiceFabricManager.GetConfigParameter("Config", "FileProcessingConfig", "AuthorityURL");
#endif
		}

		private static string GetMovementServiceHostAddress()
		{
#if DEBUG

			return "https://Top-Appdev.Talisystech.com";		// call movements service on AppDev
			//return "http://TIMMW-10.csssoftware.com:8143";		// for local testing of Movements service
#else
            return CSS.Cloud.Framework.AddressHelper.GetHostAddress("fabric:/" + ApplicationSettingsManager.GetConfigParameter("Config", "FileProcessingConfig", "MovementsPackage") + "/Movements");
#endif
        }
    }
}
