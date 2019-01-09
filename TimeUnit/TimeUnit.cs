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
using System.Linq;

namespace SlugEnt {
	/// <summary>
	/// Used to represent all valid TimeUnitTypes for the TimeUnit class.
	/// </summary>
	public enum TimeUnitTypes : byte { Seconds, Minutes, Hours, Days, Weeks };



	/// <summary>
	/// Represents a unit of time that is sometimes used to provide a more friendly human readable format.  The unit of time is represented as a string in the format
	/// [amount of units][unit value].  Where:
	///   [amount of units] is a whole number representing how many of the units this time represents.
	///   [TimeUnitType] is a single character which represents the unit value.  Valid values are:
	///      s - Seconds
	///      m - Minutes
	///      h - Hours
	///      d - Days
	///      w - Weeks
	///      
	///      Larger Unit Types are not allowed as they become invalid due to calendar variations (not all months are 30 days for instance, or leap year). 
	///      
	/// Two TimeUnits are considered to be equal if their base amount of time (seconds) is the same.  So a TimeUnit of 120s and 2m would be considered equal.  This applies
	/// to both the == and Equals comparison operators.
	/// Common Functions are:  
	///    - Value
	///    - ValueAsNumeric
	///    - ToString
	///    - ValueAsWholeNumber
	///    
	/// </summary>
	/// <example>6m - 6 minutes</example>
	/// <example>14h - 14 hours</example>
	/// <example>104d - 104 days</example>
	public struct TimeUnit : IEquatable<TimeUnit>, IComparable<TimeUnit> {
		/// <summary>
		/// We store the base unit in seconds.  We use a double because the TimeSpan conversion functions all require doubles, so this avoids lots of casting to double.
		/// </summary>
		private readonly long _seconds;

		/// <summary>
		/// The TimeUnitType that this represents.
		/// </summary>
		private readonly TimeUnitTypes _unitType;



		/// <summary>
		/// Takes a number of seconds and turns it into a TimeUnit value stored as seconds.  Seconds will be the preferred UnitType display.
		/// </summary>
		/// <param name="seconds">The number of seconds the TimeUnit represents</param>
		public TimeUnit (long seconds) {
			if ( seconds < 0 ) { throw new ArgumentException("TimeUnits cannot be negative numbers."); }

			_seconds = seconds;
			_unitType = TimeUnitTypes.Seconds;
		}


		/// <summary>
		/// Takes a number of seconds (double) and turns it into a TimeUnit value stored as seconds.  Seconds will be the preferred UnitType display.
		/// </summary>
		/// <param name="seconds">The number of seconds the TimeUnit represents</param>
/*		public TimeUnit(double seconds) {
			if (seconds < 0) { throw new ArgumentException("TimeUnits cannot be negative numbers."); }
			_seconds = seconds;
			_unitType = TimeUnitTypes.Seconds;
		}
		*/


		/// <summary>
		/// Creates a TimeUnit object from the specified TimeUnit string value (ie. 7m or 3h).  Note, the numeric part of the TimeUnit value must be an integer number.  The suffix will define the preferred UnitType display of the object.
		/// </summary>
		/// <param name="timeValue"></param>
		public TimeUnit (string timeValue) {
			// First get last character of string.  Must be a letter.
			int len = timeValue.Length;

			// Length must be > 2.
			if ( len < 2 ) {
				throw new ArgumentException(
					"timeValue", "The value of TimeValue must be in the format of <number><TimeType> Where TimeType is a single character.");
			}

			char timeIncrement = timeValue [len - 1];

			// Now get first part of string which is the numeric part.
			string timeDuration = new string(timeValue.TakeWhile(d => !Char.IsLetter(d)).ToArray());


			// Validate we have a number and ultimately convert into a double for storing.
			long numericValue;
			if ( !long.TryParse(timeDuration, out numericValue) ) {
				throw new ArgumentException(
					"timeValue",
					"Did not contain a valid numeric prefix.  Proper format is <Number><TimeType> where Number is an integer and TimeType is a single character.");
			}


			// To completely validate we have what they sent we build a new string from the 2 components and compare the 2.  Should be equal.
			string snew = numericValue.ToString() + timeIncrement;
			if ( snew != timeValue ) {
				string msg = String.Format(
					"Argument is in an invalid format - [{0}].  Proper format is <Number><TimeType> where Number is an integer and TimeType is a single character.",
					timeValue);
				throw new ArgumentException("timeValue", msg);
			}


			// Now we just need to validate the time unit type is correct and convert to seconds.


			// Validate the unit of time is correct.
			switch ( timeIncrement ) {
				case 'd':
					_unitType = TimeUnitTypes.Days;
					_seconds = 86400 * numericValue;
					break;
				case 'm':
					_unitType = TimeUnitTypes.Minutes;
					_seconds = 60 * numericValue;
					break;
				case 'h':
					_unitType = TimeUnitTypes.Hours;
					_seconds = 3600 * numericValue;
					break;
				case 's':
					_unitType = TimeUnitTypes.Seconds;
					_seconds = numericValue;
					break;
				case 'w':
					_unitType = TimeUnitTypes.Weeks;
					_seconds = 604800 * numericValue;
					break;
				default: throw new ArgumentException("Invalid TimeUnitType specified.  Must be one of s,m,h,d,w.");
			}
		}



		/// <summary>
		/// Prints out the the TimeUnit in long text.  Example: 6 Minutes.
		/// </summary>
		/// <returns>String representing the long textual value.</returns>
		public override string ToString () {
			string rs = String.Format("{0} {1}", GetUnits(_unitType), _unitType.ToString());
			return rs;
		}



		/// <summary>
		/// Returns the TimeUnit "native" value.  Example:  6m
		/// </summary>
		public string Value {
			get {
				string rs = String.Format("{0}{1}", GetUnits(_unitType), GetUnitTypeAbbrev());
				return rs;
			}
		}



		public string ValueAsWholeNumber {
			get => GetHighestWholeNumberUnitType((long) _seconds);
		}



		/// <summary>
		/// Returns the numeric value of this TimeUnit.  If upon creation you specified a value of 9m (9 minutes) this function will return 9.
		/// </summary>
		public long ValueAsNumeric {
			get => GetUnits(_unitType);
		}


/* This was never called and the field value should never be able to be changed.
        ///  Do we want to allow changing of the unit type character or should it stay what it was when initially set.
		/// <summary>
		/// Sets the UnitType field to the appropriate value based upon its string representation.
		/// It does not convert or change the seconds property.
		/// </summary>
		/// <param name="timeIncrement"></param>
		private void SetUnitTypeCharacter (char timeIncrement) {
			// Validate the unit of time is correct.
			switch (timeIncrement) {
				case 'd':
					_unitType = TimeUnitTypes.Days;
					break;
				case 'm':
					_unitType = TimeUnitTypes.Minutes;
					break;
				case 'h':
					_unitType = TimeUnitTypes.Hours;
					break;
				case 's':
					_unitType = TimeUnitTypes.Seconds;
					break;
				case 'w':
					_unitType = TimeUnitTypes.Weeks;
					break;
				default:
					throw new ArgumentException("Invalid TimeUnitType specified.  Must be one of s,m,h,d,w.");
			}

		}
*/



		/// <summary>
		/// Validates that the timeIncrement character passed in is a valid TimeUnit type.
		/// </summary>
		/// <param name="timeIncrement">Char that should be inspected for a valid TimeUnit Type.</param>
		/// <returns>True if valid timeIncrement character code.  False otherwise.</returns>
		public bool ValidateUnitTypeCharacter (char timeIncrement) {
			// Validate the unit of time is correct.
			switch ( timeIncrement ) {
				case 'd':
				case 'm':
				case 'h':
				case 's':
				case 'w':
					return true;
				default: return false;

				//throw new ArgumentException("Invalid TimeUnitType specified.  Must be one of s,m,h,d,w.");
			}
		}



		#region "Object Overrides"


		// Compare if the same.  Considered the same if number of seconds is same, does not matter what the TimeUnit type is.
/*		public static bool operator == (TimeUnit x, TimeUnit y) {
			if (x._seconds == y._seconds) {	return true; }
			else { return false; }
		}



		// Compare if not the same.  Considered the same if number of seconds is same, does not matter what the TimeUnit type is.
		public static bool operator != (TimeUnit x, TimeUnit y) {
			if (x._seconds != y._seconds) { return true; }
			else { return false; }
		}
        */


		public override bool Equals (object obj) {
			if ( !(obj is TimeUnit) ) { return false; }

			TimeUnit tu = (TimeUnit) obj;

			if ( tu._seconds == _seconds ) { return true; }
			else { return false; }
		}


		public override int GetHashCode () { return (int) _seconds; }


		// Math comparison functions
		public static bool operator == (TimeUnit x, TimeUnit y) { return x._seconds == y._seconds; }
		public static bool operator != (TimeUnit x, TimeUnit y) { return x._seconds != y._seconds; }
		public static bool operator > (TimeUnit x, TimeUnit y) { return x.CompareTo(y) > 0; }
		public static bool operator < (TimeUnit x, TimeUnit y) { return x.CompareTo(y) < 0; }
		public static bool operator >= (TimeUnit x, TimeUnit y) { return x.CompareTo(y) >= 0; }
		public static bool operator <= (TimeUnit x, TimeUnit y) { return x.CompareTo(y) <= 0; }


		#endregion



		#region "InFX Functions"     


		/// <summary>
		/// Returns the number of seconds this TimeUnit represents in Double format.
		/// </summary>
		public double InSecondsAsDouble {
			get { return GetUnits(TimeUnitTypes.Seconds); }
		}


		/// <summary>
		/// Returns the TimeUnit value as a double seconds string. IE.  125s
		/// </summary>
		public string InSecondsAsString {
			get { return (InSecondsAsDouble.ToString() + "s"); }
		}


		/// <summary>
		/// Returns the TimeUnit in seconds format, but as a long value.
		/// </summary>
		public long InSecondsLong {
			get { return (long) GetUnits(TimeUnitTypes.Seconds); }
		}


		/// <summary>
		/// Returns the number of seconds this TimeUnit represents.
		/// </summary>
		/// <returns></returns>
		public double InMinutesAsDouble {
			get { return GetUnits(TimeUnitTypes.Minutes); }
		}


		/// <summary>
		///  Returns the TimeUnit in minutes as a string (ie. 6m)
		/// </summary>
		public string InMinutesAsString {
			get { return (InMinutesAsDouble.ToString() + "m"); }
		}


		/// <summary>
		/// Returns the number of seconds this TimeUnit represents.
		/// </summary>
		/// <returns></returns>
		public double InHoursAsDouble {
			get { return GetUnits(TimeUnitTypes.Hours); }
		}


		/// <summary>
		/// Returns the number of hours this timeunit represents as a string.  Ex.  29h
		/// </summary>
		public string InHoursAsString {
			get { return (InHoursAsDouble.ToString() + "h"); }
		}


		/// <summary>
		/// Returns the number of days this TimeUnit represents as a double.
		/// </summary>
		/// <returns></returns>
		public double InDaysAsDouble {
			get { return GetUnits(TimeUnitTypes.Days); }
		}


		/// <summary>
		/// Returns the number of days in string format.  Ex.  16d
		/// </summary>
		public string InDaysAsString {
			get { return (InDaysAsDouble.ToString() + "d"); }
		}


		/// <summary>
		/// Returns the number of weeks this TimeUnit represents in double form.  Ex.  6.44
		/// </summary>
		/// <returns></returns>
		public double InWeeksAsDouble {
			get { return GetUnits(TimeUnitTypes.Weeks); }
		}


		/// <summary>
		/// Returns the number of weeks this TimeUnit represents in string form:  6.4w
		/// </summary>
		/// <returns></returns>
		public string InWeeksAsString {
			get { return (InWeeksAsDouble.ToString() + "w"); }
		}


		#endregion


		#region "UnitType Functions"


		/// <summary>
		/// Returns the proper Unit Type abbreviation (single letter) of the Current TimeUnit value.
		/// </summary>
		/// <returns>String with single character representing the Unit Type.</returns>
		private string GetUnitTypeAbbrev () { return GetTimeUnitTypeAsString(_unitType); }



		/// <summary>
		/// Gets the number of units of the Unit Type.  Basically, just converts the internally stored seconds into proper unit value.
		/// </summary>
		/// <returns>long - The number of units of the given UnitType</returns>
		private long GetUnits (TimeUnitTypes tuType) {
			switch ( tuType ) {
				case TimeUnitTypes.Seconds: return _seconds;
				case TimeUnitTypes.Minutes: return _seconds / 60; //ConvertSecondsToMinutes(_seconds);
				case TimeUnitTypes.Hours: return _seconds / 3600; //ConvertSecondsToHours(_seconds);
				case TimeUnitTypes.Days: return _seconds / 86400; //ConvertSecondsToDays(_seconds);
				case TimeUnitTypes.Weeks: return _seconds / 604800; //(ConvertSecondsToDays(_seconds) / 7);
				default: return _seconds;
			}
		}



		/// <summary>
		/// Returns the appropriate character representation for the TimeUnitTypes Enum value.  For example if TimeUnitTypes.Seconds returns 's'.
		/// </summary>
		/// <param name="timeUnitType">The TimeUnitType enum value to retrieve the character or string representation for.</param>
		/// <returns>string value of the TimeUnitType</returns>
		public static string GetTimeUnitTypeAsString (TimeUnitTypes timeUnitType) {
			switch ( timeUnitType ) {
				case TimeUnitTypes.Seconds: return "s";
				case TimeUnitTypes.Minutes: return "m";
				case TimeUnitTypes.Hours: return "h";
				case TimeUnitTypes.Days: return "d";
				case TimeUnitTypes.Weeks: return "w";
				default: return "s";
			}
		}


		#endregion



		/// <summary>
		/// Returns the TimeUnit in a value that represents the largest unit value that results in a whole number.  For instance - 360 seconds would return 6m.  359 seconds would return 359 seconds.
		/// </summary>
		internal static string GetHighestWholeNumberUnitType (long seconds) {
			long retNumeric = -1;
			string retString = "";
			long remainder = 0;


			// Try to convert to Weeks.
			if ( seconds >= 604800 ) {
				remainder = seconds % 604800;
				if ( remainder == 0 ) {
					retNumeric = seconds / 604800;
					retString = GetTimeUnitTypeAsString(TimeUnitTypes.Weeks);
					return (retNumeric.ToString() + retString);
				}
			}


			// Try to convert to days
			if ( seconds >= 86400 ) {
				remainder = seconds % 86400;
				if ( remainder == 0 ) {
					retNumeric = seconds / 86400;
					retString = GetTimeUnitTypeAsString(TimeUnitTypes.Days);
					return (retNumeric.ToString() + retString);
				}
			}


			// Try to convert to Hours
			if ( seconds >= 3600 ) {
				remainder = seconds % 3600;
				if ( remainder == 0 ) {
					retNumeric = seconds / 3600;
					retString = GetTimeUnitTypeAsString(TimeUnitTypes.Hours);
					return (retNumeric.ToString() + retString);
				}
			}


			// Try to convert to minutes
			if ( seconds >= 60 ) {
				remainder = seconds % 60;
				if ( remainder == 0 ) {
					retNumeric = seconds / 60;
					retString = GetTimeUnitTypeAsString(TimeUnitTypes.Minutes);
					return (retNumeric.ToString() + retString);
				}
			}


//			if (retNumeric != -1) { return (retNumeric.ToString() + retString); }

			return (seconds.ToString() + GetTimeUnitTypeAsString(TimeUnitTypes.Seconds));
		}



		/// <summary>
		/// Adds 2 TimeUnit types together to arrive at a 3rd TimeUnit object.  Will set the TimeUnit Type property to the largest WholeNumber value it can determine.  
		/// So, if a TimeUnit of 60s is added to a TimeUnit of 59m, will result in a TimeUnit of 1h.
		/// </summary>
		/// <param name="a">1st TimeUnit object</param>
		/// <param name="b">2nd TimeUnit object</param>
		/// <returns>Result of adding the 2 TimeUnits together.</returns>
		public static TimeUnit operator + (TimeUnit a, TimeUnit b) {
			long newSeconds = a._seconds + b._seconds;
			string newValue = GetHighestWholeNumberUnitType(newSeconds);
			return (new TimeUnit(newValue));
		}



		/// <summary>
		/// Subtracts 2 TimeUnit types to arrive at a 3rd TimeUnit object.  Will set the TimeUnit Type property to the largest WholeNumber value it can determine.  
		/// So, if a TimeUnit of 60s is added to a TimeUnit of 59m, will result in a TimeUnit of 1h.  
		/// If the 2nd TimeUnit is larger than the first TimeUnit the result will be zero.
		/// </summary>
		/// <param name="a">1st TimeUnit object</param>
		/// <param name="b">2nd TimeUnit object</param>
		/// <returns>Result of subtracting TimeUnit b from TimeUnit a.  Negative values all result in a value of 0s.</returns>
		public static TimeUnit operator - (TimeUnit a, TimeUnit b) {
			long newSeconds = a._seconds - b._seconds;
			if ( newSeconds < 0 ) { newSeconds = 0; }

			string newValue = GetHighestWholeNumberUnitType(newSeconds);
			return (new TimeUnit(newValue));
		}



		#region "DateMath Functions"


		/// <summary>
		/// Adds the Current TimeUnit value to the date provided and returns a new DateTime object.
		/// </summary>
		/// <param name="dateTime">DateTime object to be used as the starting date and time.</param>
		/// <returns>Datetime object with the current TimeUnit value added to the datetime provided.</returns>
		public DateTime AddToDate (DateTime dateTime) { return dateTime.AddSeconds(_seconds); }



		/// <summary>
		/// Subtracts the Current TimeUnit value from the date provided and returns a new DateTime object.
		/// </summary>
		/// <param name="dateTime">DateTime object to be used as the starting date and time.</param>
		/// <returns>Datetime object with the current TimeUnit subtracted from the datetime provided.</returns>
		public DateTime SubtractFromDate (DateTime dateTime) { return dateTime.AddSeconds(-(_seconds)); }


		#endregion


		#region "Math Functions"


		public TimeUnit AddSeconds (long seconds) {
			long calcSeconds;
			if ( seconds < 0 ) { return SubtractSeconds(-seconds); }
			else { calcSeconds = _seconds + seconds; }

			string newValue = GetHighestWholeNumberUnitType(calcSeconds);
			return new TimeUnit(newValue);
		}


		public TimeUnit AddMinutes (long minutes) {
			long calcSeconds;
			if ( minutes < 0 ) { return SubtractMinutes(-minutes); }
			else { calcSeconds = _seconds + (minutes * 60); }

			string newValue = GetHighestWholeNumberUnitType((long) calcSeconds);
			return new TimeUnit(newValue);
		}


		public TimeUnit AddHours (long hours) {
			long calcSeconds;
			if ( hours < 0 ) { return SubtractHours(-hours); }
			else { calcSeconds = _seconds + 3600 * hours; }

			string newValue = GetHighestWholeNumberUnitType(calcSeconds);
			return new TimeUnit(newValue);
		}


		public TimeUnit AddDays (long days) {
			long calcSeconds;
			if ( days < 0 ) { return SubtractDays(-days); }
			else { calcSeconds = _seconds + 86400 * days; }

			string newValue = GetHighestWholeNumberUnitType((long) calcSeconds);
			return new TimeUnit(newValue);
		}


		public TimeUnit SubtractSeconds (long seconds) {
			double calcSeconds;
			if ( seconds > _seconds ) { calcSeconds = 0; }
			else { calcSeconds = _seconds - seconds; }

			string newValue = GetHighestWholeNumberUnitType((long) calcSeconds);
			return new TimeUnit(newValue);
		}


		public TimeUnit SubtractMinutes (long minutes) {
			long val = minutes * 60;
			double calcSeconds;
			if ( val > _seconds ) { calcSeconds = 0; }
			else { calcSeconds = _seconds - val; }

			string newValue = GetHighestWholeNumberUnitType((long) calcSeconds);
			return new TimeUnit(newValue);
		}


		public TimeUnit SubtractHours (long hours) {
			double val = ConvertHoursToSeconds((double) hours);
			double calcSeconds;
			if ( val > _seconds ) { calcSeconds = 0; }
			else { calcSeconds = _seconds - val; }

			string newValue = GetHighestWholeNumberUnitType((long) calcSeconds);
			return new TimeUnit(newValue);
		}


		public TimeUnit SubtractDays (long days) {
			double val = ConvertDaysToSeconds((double) days);
			double calcSeconds;
			if ( val > _seconds ) { calcSeconds = 0; }
			else { calcSeconds = _seconds - val; }

			string newValue = GetHighestWholeNumberUnitType((long) calcSeconds);
			return new TimeUnit(newValue);
		}


		#endregion


		#region To days


		public static double ConvertMillisecondsToDays (double milliseconds) { return TimeSpan.FromMilliseconds(milliseconds).TotalDays; }

		public static double ConvertSecondsToDays (double seconds) { return TimeSpan.FromSeconds(seconds).TotalDays; }

		public static double ConvertMinutesToDays (double minutes) { return TimeSpan.FromMinutes(minutes).TotalDays; }

		public static double ConvertHoursToDays (double hours) { return TimeSpan.FromHours(hours).TotalDays; }


		#endregion


		#region To hours


		public static double ConvertMillisecondsToHours (double milliseconds) { return TimeSpan.FromMilliseconds(milliseconds).TotalHours; }

		public static double ConvertSecondsToHours (double seconds) { return TimeSpan.FromSeconds(seconds).TotalHours; }

		public static double ConvertMinutesToHours (double minutes) { return TimeSpan.FromMinutes(minutes).TotalHours; }

		public static double ConvertDaysToHours (double days) { return TimeSpan.FromHours(days).TotalHours; }


		#endregion


		#region To minutes


		public static double ConvertMillisecondsToMinutes (double milliseconds) { return TimeSpan.FromMilliseconds(milliseconds).TotalMinutes; }

		public static double ConvertSecondsToMinutes (double seconds) { return TimeSpan.FromSeconds(seconds).TotalMinutes; }

		public static double ConvertHoursToMinutes (double hours) { return TimeSpan.FromHours(hours).TotalMinutes; }

		public static double ConvertDaysToMinutes (double days) { return TimeSpan.FromDays(days).TotalMinutes; }


		#endregion


		#region To seconds


		public static double ConvertMillisecondsToSeconds (double milliseconds) { return TimeSpan.FromMilliseconds(milliseconds).TotalSeconds; }

		public static double ConvertMinutesToSeconds (double minutes) { return TimeSpan.FromMinutes(minutes).TotalSeconds; }

		public static double ConvertHoursToSeconds (double hours) { return TimeSpan.FromHours(hours).TotalSeconds; }

		public static double ConvertDaysToSeconds (double days) { return TimeSpan.FromDays(days).TotalSeconds; }


		#endregion


		#region To milliseconds


		public static double ConvertSecondsToMilliseconds (double seconds) { return TimeSpan.FromSeconds(seconds).TotalMilliseconds; }

		public static double ConvertMinutesToMilliseconds (double minutes) { return TimeSpan.FromMinutes(minutes).TotalMilliseconds; }

		public static double ConvertHoursToMilliseconds (double hours) { return TimeSpan.FromHours(hours).TotalMilliseconds; }

		public static double ConvertDaysToMilliseconds (double days) { return TimeSpan.FromDays(days).TotalMilliseconds; }


		#endregion



		/// <summary>
		/// For IEquatable Interface
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals (TimeUnit other) { return (_seconds.Equals(other._seconds)); }


		/// <summary>
		/// Used for IComparable Interface
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo (TimeUnit other) { return _seconds.CompareTo(other); }


		//TODO - This is where I am at.
		// Allow direct setting to/from string
		public static implicit operator string (TimeUnit timeUnit) { return timeUnit.Value; }
		public static implicit operator TimeUnit (string s) { return new TimeUnit(s); }


		// Allow direct setting to/from an integer
		public static implicit operator int (TimeUnit timeUnit) { return (int) timeUnit._seconds; }
		public static implicit operator TimeUnit (int s) { return new TimeUnit(s); }
	}
}