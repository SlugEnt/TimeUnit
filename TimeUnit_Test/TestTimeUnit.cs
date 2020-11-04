/*
 * Copyright 2018 Scott Herrmann

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

	https://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/


using System;
using NUnit.Framework;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SlugEnt;


	[Parallelizable]
	public class TestTimeUnit
	{
		/// <summary>
		/// Validates that providing a seconds value, that it gets correctly stored.
		/// </summary>
		[Test]
		public void CanSetSecondsCorrectly() {
			TimeUnit tu = new TimeUnit(360);

			Assert.AreEqual("360s", tu.Value);
			Assert.AreEqual("360 Seconds", tu.ToString());
		}



		/// <summary>
		/// Validates that providing a second TimeUnit Time value gets set correctly.
		/// </summary>
		[Test]
		public void CanSetSecondsCorrectlyasTimeUnitString() {
			string secs = "720s";
			TimeUnit tu = new TimeUnit(secs);

			Assert.AreEqual("720s", tu.Value);
			Assert.AreEqual("720 Seconds", tu.ToString());
		}



		/// <summary>
		/// Validates that providing a minute value, that it gets correctly stored.
		/// </summary>
		[Test]
		public void CanSetMinutesCorrectly() {
			string mins = "15m";

			TimeUnit tu = new TimeUnit(mins);

			Assert.AreEqual("15m", tu.Value);
			Assert.AreEqual("15 Minutes", tu.ToString());
		}



		/// <summary>
		/// Validates that providing an  hour value, that it gets correctly stored.
		/// </summary>
		[Test]
		public void CanSetHoursCorrectly() {
			string hours = "48h";

			TimeUnit tu = new TimeUnit(hours);

			Assert.AreEqual("48h", tu.Value);
			Assert.AreEqual("48 Hours", tu.ToString());
		}



		/// <summary>
		/// Validates that providing a Day value, that it gets correctly stored.
		/// </summary>
		[Test]
		public void CanSetDaysCorrectly() {
			string days = "5d";

			TimeUnit tu = new TimeUnit(days);

			Assert.AreEqual("5d", tu.Value);
			Assert.AreEqual("5 Days", tu.ToString());
		}



		/// <summary>
		/// Validates that providing a weeks value, that it gets correctly stored.
		/// </summary>
		[Test]
		public void CanSetWeeksCorrectly() {
			string weeks = "19w";

			TimeUnit tu = new TimeUnit(weeks);

			Assert.AreEqual("19w", tu.Value);
			Assert.AreEqual("19 Weeks", tu.ToString());
		}



		/// <summary>
		/// Validates that supplying an empty constructor argument, results in an object with 0 seconds.
		/// </summary>
		[Test]
		public void EmptyConstructorSetsZeroSeconds() {
			TimeUnit tu = new TimeUnit();
			Assert.AreEqual("0s", tu.Value);
		}



		/// <summary>
		/// Validates that supplying a string constructor argument without the Unit Type character throws an error
		/// </summary>
		[Test]
		public void Missing_TimeUnitType_ResultsInError() {
			Assert.Throws<ArgumentException>(() =>
				new TimeUnit("5"));
		}



		/// <summary>
		/// Validates that supplying a string constructor argument with a Unity Type character of more than 1 letter throws an error
		/// </summary>
		[Test]
		public void MultipleCharacter_UnitType_ResultsInError() {
			Assert.Throws<ArgumentException>(() =>
				new TimeUnit("5mm"));
		}



		/// <summary>
		/// Validates that supplying an invalid Time Unit Type character results in error.
		/// </summary>
		[Test]

		public void InvalidUnitTypes_ThrowsError([Values('a', 'b', 'c', 'e', 'f', 'g', 'i', 'j', 'k', 'l', 'n', 'o', 'p', 'q', 'r', 't', 'u', 'v', 'x', 'y', 'z')] char val) {
			string tu = "6" + val;

			Assert.Throws<ArgumentException>(() =>
				new TimeUnit(tu));
		}



		/// <summary>
		/// Validates that a non second time unit returns the correct number of seconds.
		/// </summary>
		[Test]
		public void CanReturnATimeUnitNumberOfSeconds() {
			TimeUnit t = new TimeUnit("5m");

			double secs = t.InSecondsAsDouble;
			Assert.AreEqual(300, secs);
		}



		/// <summary>
		/// Validates tht we can return the TimeUnit Value in a TimeUnit string form different from the UnitTYpe of the actual TimeUnit Object.
		/// Ie.  Return a TimeUnit with a value of 360m into 6h.
		/// </summary>
		/// <param name="ts"></param>
		/// <param name="tuType"></param>
		/// <returns></returns>
		[Test, TestCaseSource(typeof(TimeUnitDataClass), "TestCaseInFXString")]
		public string GetInTimeUnits(string ts, TimeUnitTypes tuType) {
			TimeUnit t = new TimeUnit(ts);
			switch (tuType) {
				case TimeUnitTypes.Seconds:
					return t.InSecondsAsString;
				case TimeUnitTypes.Minutes:
					return t.InMinutesAsString;
				case TimeUnitTypes.Hours:
					return t.InHoursAsString;
				case TimeUnitTypes.Days:
					return t.InDaysAsString;
				case TimeUnitTypes.Weeks:
					return t.InWeeksAsString;
				default:
					return ("Invalid Test Case Parameter of " + ts);
			}
		}



		/// <summary>
		/// Validates that we can return the TimeUnit value in a numeric form different from the Unit Type of the actual TimeUnit object.
		/// IE. Return a TimeUnit object stored as 4m into 240s!
		/// </summary>
		/// <param name="ts"></param>
		/// <param name="tuType"></param>
		/// <returns></returns>
		[Test, TestCaseSource(typeof(TimeUnitDataClass), "TestCaseInFXNumeric")]
		public double GetInTimeUnitsNumeric(string ts, TimeUnitTypes tuType) {
			TimeUnit t = new TimeUnit(ts);
			switch (tuType) {
				case TimeUnitTypes.Seconds:
					return t.InSecondsAsDouble;
				case TimeUnitTypes.Minutes:
					return t.InMinutesAsDouble;
				case TimeUnitTypes.Hours:
					return t.InHoursAsDouble;
				case TimeUnitTypes.Days:
					return t.InDaysAsDouble;
				case TimeUnitTypes.Weeks:
					return t.InWeeksAsDouble;
				default:
					throw new ArgumentException("Invalid Test Case Data supplied: " + ts);
			}
		}



		/// <summary>
		/// Test the equality Equals method.  Should be true
		/// </summary>
		[Test]
		public void TestEquals() {
			TimeUnit t1 = new TimeUnit(45);
			TimeUnit t2 = new TimeUnit(45);

			Assert.True(t1.Equals(t2));
		}


		/// <summary>
		/// Test the equality Equals method should return false.
		/// </summary>
		[Test]
		public void TestNotEqual() {
			TimeUnit t1 = new TimeUnit(45);
			TimeUnit t2 = new TimeUnit(44);

			Assert.False(t1.Equals(t2));
		}



		[Test]
		public void ValueAsNumeric_ReturnsnumericValue() {
			int val = 326;
			string timeStr = val.ToString() + "m";
			TimeUnit t1 = new TimeUnit(timeStr);
			Assert.AreEqual(val, t1.ValueAsNumeric);
		}




		/// <summary>
		/// Validates that the correct numeric suffix is returned for the given TimeUnitTypes value.
		/// </summary>
		/// <param name="val"></param>
		/// <param name="result"></param>
		[Test]
		[TestCase(TimeUnitTypes.Days, "d")]
		[TestCase(TimeUnitTypes.Hours, "h")]
		[TestCase(TimeUnitTypes.Minutes, "m")]
		[TestCase(TimeUnitTypes.Seconds, "s")]
		[TestCase(TimeUnitTypes.Weeks, "w")]
		public void TimeUnitStringValues_AreCorrect(TimeUnitTypes val, string result) {
			Assert.AreEqual(TimeUnit.GetTimeUnitTypeAsString(val), result);
		}



		/// <summary>
		/// Validates that the ValueAsWholeNumber function works correctly:
		///  - Returns the largest whole number time suffix for a given value 
		///  - Or returns the number of seconds.
		/// </summary>
		/// <param name="seconds"></param>
		/// <param name="result"></param>

		[Test]
		[TestCase(60, "1m")]
		[TestCase(180, "3m")]
        [TestCase(600, "10m")]
        [TestCase(3600, "1h")]
        [TestCase(5399, "5399s")]
        [TestCase(5400, "90m")]
		[TestCase(7200, "2h")]
		[TestCase(86400, "1d")]
		[TestCase(108000, "30h")]
		[TestCase(259200, "3d")]
		[TestCase(604800, "1w")]
		[TestCase(90, "90s")]
		[TestCase(4000, "4000s")]
		[TestCase(265000, "265000s")]

		public void ValueAsWholeNumber_WorksCorrectly(long seconds, string result) {
			TimeUnit a = new TimeUnit(seconds);
			Assert.AreEqual(result, a.ValueAsWholeNumber);
		}



		// Validate DateMath Addition
		[Test]
		[TestCase("60m", 60, "m")]
		[TestCase("2d", 2, "d")]
		[TestCase("205h", 205, "h")]

		public void AddToDate_Works(string timeUnitValue, long number, string unit) {
			TimeUnit a = new TimeUnit(timeUnitValue);
			long seconds = a.InSecondsLong;

			// Create Test DateTime Object
			DateTime d = DateTime.Now;
			DateTime e;
			switch (unit) {
				case "m":
					e = d.AddMinutes(number);
					break;
				case "d":
					e = d.AddDays(number);
					break;
				case "h":
					e = d.AddHours(number);
					break;
				default:
					e = DateTime.Now;
					break;
			}

			// Now use TimeUnit date math.
			DateTime f = a.AddToDate(d);
			Assert.AreEqual(e, f, "DateTime {0} did not equal expected value of {1}", f, e);

		}



		// Validate that subtracting a TimeUnit from a Datetime yields correct results.
		[Test]
		[TestCase("60m", 60, "m")]
		[TestCase("2d", 2, "d")]
		[TestCase("205h", 205, "h")]
		public void SubtractFromDate_Works(string timeUnitValue, long number, string unit) {
			TimeUnit a = new TimeUnit(timeUnitValue);
			long seconds = a.InSecondsLong;

			// Create Test DateTime Object
			DateTime d = DateTime.Now;
			DateTime e;
			switch (unit) {
				case "m":
					e = d.AddMinutes(-number);
					break;
				case "d":
					e = d.AddDays(-number);
					break;
				case "h":
					e = d.AddHours(-number);
					break;
				default:
					e = DateTime.Now;
					break;
			}

			// Now use TimeUnit date math.
			DateTime f = a.SubtractFromDate(d);
			Assert.AreEqual(e, f, "DateTime {0} did not equal expected value of {1}", f, e);
		}



		/// <summary>
		/// Validate that Subtracting time from the TimeUnit value that results in a negative number returns zero for the TimeUnit value.
		/// </summary>
		/// <param name="timeUnitValue"></param>
		/// <param name="unit"></param>
		/// <param name="value"></param>
		[Test]
		[TestCase("60m", "m", 90)]
		[TestCase("450s", "s", 9000)]
		[TestCase("35h", "h", 72)]
		[TestCase("2h", "s", 7201)]
		[TestCase("45s", "s", 47)]
		[TestCase("10m", "s", 601)]
		public void SubtractionFunctionsWithNegativeValues(string timeUnitValue, string unit, long value) {
			TimeUnit a = new TimeUnit(timeUnitValue);
			TimeUnit b;

			switch (unit) {
				case "m":
					b = a.SubtractMinutes(value);
					break;
				case "d":
					b = a.SubtractDays(value);
					break;
				case "h":
					b = a.SubtractHours(value);
					break;
				default:
					b = a.SubtractSeconds(value);
					break;

			}
			Assert.AreEqual(0, b.ValueAsNumeric);
		}



		[Test]
		[TestCase("60m", "m", 1, "61m")]
		[TestCase("12d", "d", 3, "15d")]
		[TestCase("7d", "d", 1, "8d")]
		[TestCase("10d", "d", 4, "2w")]
		[TestCase("60m", "m", 60, "2h")]
		[TestCase("60m", "m", 90, "150m")]
		[TestCase("60m", "m", -90,"0s")]
		[TestCase("50s", "s", 10, "1m")]
		[TestCase("50s", "s", 11, "61s")]
		[TestCase("450s", "s", -9000,"0s")]
		[TestCase("35h", "h", -72,"0s")]
		[TestCase("2h", "s", -7201,"0s")]
		[TestCase("45s", "s", -47,"0s")]
		[TestCase("10m", "s", -601,"0s")]

		public void ValidateAdditionFunctions(string timeUnitValue, string unit, long value,string result) {
			TimeUnit a = new TimeUnit(timeUnitValue);
			TimeUnit b;
			switch (unit) {
				case "m":
					b = a.AddMinutes(value);
					break;
				case "d":
					b = a.AddDays(value);
					break;
				case "h":
					b = a.AddHours(value);
					break;
				default:
					b = a.AddSeconds(value);
					break;

			}
			Assert.AreEqual(result, b.Value);
		}




		[TestCase("10m", "m", 6, "4m")]
		[TestCase("1h", "m", 16, "44m")]
		[TestCase("3d", "d", 2, "1d")]
		[TestCase("2w", "d", 2, "12d")]
		[TestCase("140s", "s", 20, "2m")]
		[TestCase("14h", "h", 3, "11h")]
	public void SubtractionFunctionsWork(string timeUnitValue, string unit, long value, string result) {
			TimeUnit a = new TimeUnit(timeUnitValue);
			TimeUnit b;
			switch (unit) {
				case "m":
					b = a.SubtractMinutes(value);
					break;
				case "d":
					b = a.SubtractDays(value);
					break;
				case "h":
					b = a.SubtractHours(value);
					break;
				default:
					b = a.SubtractSeconds(value);
					break;

			}
			Assert.AreEqual(result, b.Value);
		}



	// Adding 2 TimeUnits
	[Test]
		[TestCase("60s","59m","1h")]
		[TestCase("60s", "60s", "2m")]
		[TestCase("12s", "14s", "26s")]
		[TestCase("23h", "1h", "1d")]
		[TestCase("29h", "2h", "31h")]
		[TestCase("4d", "1d", "5d")]
		[TestCase("4d", "3d", "1w")]
		[TestCase("2w", "6w", "8w")]
		public void Adding2TimeUnits_Success (string unitA, string unitB, string result) {
			TimeUnit a = new TimeUnit(unitA);
			TimeUnit b = new TimeUnit(unitB);
			TimeUnit c = a + b;
			Assert.AreEqual(result, c.Value);
		}



		// Subtracting TimeUnit A from TimeUnit B
		[Test]
		[TestCase("60s", "59m", "0s")]
		[TestCase("60s", "60s", "0s")]
		[TestCase("60s", "12s", "48s")]
		[TestCase("12m", "10m", "2m")]
		[TestCase("23h", "1h", "22h")]
		[TestCase("29h", "5h", "1d")]
		[TestCase("4d", "5d", "0s")]
		[TestCase("24w", "5w", "19w")]
		[TestCase("1w", "2d", "5d")]
		public void Subtracting2TimeUnits_Success(string unitA, string unitB, string result) {
			TimeUnit a = new TimeUnit(unitA);
			TimeUnit b = new TimeUnit(unitB);
			TimeUnit c = a - b;
			Assert.AreEqual(result, c.Value);
		}



        // Tests that we can set a TimeUnit = string value (as long as value is valid)
        [Test]
        public void ImplicitStringSet_Success () {
            string timeS1 = "9m";
            TimeUnit t1 = timeS1;

            Assert.AreEqual(timeS1,t1.Value, "A10:  Expected the two values to be the same.  They are not.");
        }


        // Test that we can implicitly set a string to a TimeUnit.  I.E.  string s = timeUnitVar;
        [Test]
        public void ImplicitTimeUnitSetViaString_Success () {
            TimeUnit t1 = new TimeUnit("2h");
            string s1 = t1;

            Assert.AreEqual(t1.Value,s1,"A10:  Expected the two values to be the same.  They are not.");
        }


        // Test that we can set a TimeUnit = int value.
        [Test]
        public void ImplicitIntSet_Success () {
            int time1 = 60;
            TimeUnit t1 = time1;

            Assert.AreEqual(time1,t1.InSecondsLong,"A10:  Expected the number of seconds to be set to the value passed in.");
        }


        // Test that we can set an int value = TimeUnit value.
        [Test]
        public void ImplicitTimeUnitSetViaInt_Success () {
            TimeUnit t1 = new TimeUnit("15m");
            int int1 = t1;

            Assert.AreEqual(t1.InSecondsLong,int1,"A10:  Expected the 2 values to be the same.");
        }


        // Validate that we get ArugmentException errors if passing an invalid string or integer value.
        [Test]
        public void ImplicitSet_Errors () {
            string time1 = "9g";
            TimeUnit t1;

            Assert.Throws<ArgumentException> (() => t1 = time1,"A10:  Expected to see an ArgumentException error.  Did not receive it.");

            int time2 = -4;
            Assert.Throws<ArgumentException>(() => t1 = time2, "A20:  Expected to see an ArgumentException error.  Did not receive it.");
        }




        // Validate to Json only writes Value property
		[Test]
        public void TestJSON () {
			TimeUnit t = new TimeUnit("7d");
			string json = JsonConvert.SerializeObject(t);

			JObject j  = (JObject)JsonConvert.DeserializeObject(json);
			int count = j.Count;
			Assert.AreEqual(1,count);
        }


public class TimeUnitDataClass
		{
			public static IEnumerable TestCaseInFXString
			{
				get {
					yield return new TestCaseData("60s", TimeUnitTypes.Seconds).Returns("60s");
					yield return new TestCaseData("720s", TimeUnitTypes.Minutes).Returns("12m");
					yield return new TestCaseData("3600s", TimeUnitTypes.Hours).Returns("1h");
					yield return new TestCaseData("24h", TimeUnitTypes.Days).Returns("1d");
					yield return new TestCaseData("168h", TimeUnitTypes.Weeks).Returns("1w");
				}
			}
			public static IEnumerable TestCaseInFXNumeric
			{
				get {
					yield return new TestCaseData("60s", TimeUnitTypes.Seconds).Returns(60);
					yield return new TestCaseData("720s", TimeUnitTypes.Minutes).Returns(12);
					yield return new TestCaseData("14400s", TimeUnitTypes.Hours).Returns(4);
					yield return new TestCaseData("48h", TimeUnitTypes.Days).Returns(2);
					yield return new TestCaseData("168h", TimeUnitTypes.Weeks).Returns(1);
				}
			}


		}

	}

