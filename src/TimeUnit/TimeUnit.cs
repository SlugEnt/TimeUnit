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
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace SlugEnt {
#pragma warning disable CS1591
	/// <summary>
	/// Used to represent all valid TimeUnitTypes for the TimeUnit class.
	/// </summary>
	public enum TimeUnitTypes : byte { Milliseconds, Seconds, Minutes, Hours, Days, Weeks };


	/// <summary>
	/// Represents a unit of time that is sometimes used to provide a more friendly human readable format.  The unit of time is represented as a string in the format
	/// [amount of units][unit value].  Where:
	///   [amount of units] is a whole number representing how many of the units this time represents.
	///   [TimeUnitType] is a single character which represents the unit value.  Valid values are:
	///      S - MilliSeconds
	///      s - Seconds
	///      m - Minutes
	///      h - Hours
	///      d - Days
	///      w - Weeks
	///      
	///      Larger Unit Types are not allowed as they become invalid due to calendar variations (not all months are 30 days for instance, or leap year). 
	///      
	/// Two TimeUnits are considered to be equal if their base amount of time (milliseconds) is the same.  So a TimeUnit of 120s and 2m would be considered equal.  This applies
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
	[Serializable]
	public struct TimeUnit : IEquatable<TimeUnit>, IComparable<TimeUnit>, ISerializable {
		internal const long MILLISECONDS_IN_WEEK = 604800000;
		internal const long MILLISECONDS_IN_DAY = 86400000;
		internal const long MILLISECONDS_IN_HOUR = 3600000;
		internal const long MILLISECONDS_IN_MINUTE = 60000;
		internal const long MILLISECONDS_IN_SECOND = 1000;


		/// <summary>
		/// We store the base unit in milliSeconds.
		/// </summary>
		private readonly long _milliSeconds;


		/// <summary>
		/// The TimeUnitType that this represents.
		/// </summary>
		private readonly TimeUnitTypes _unitType;
		

		/// <summary>
		/// Takes a number of seconds and turns it into a TimeUnit value stored as seconds.  Seconds will be the preferred UnitType display.
		/// </summary>
		/// <param name="milliSeconds">The number of seconds the TimeUnit represents</param>
		public TimeUnit (long milliSeconds) {
			if ( milliSeconds < 0 ) { throw new ArgumentException("TimeUnits cannot be negative numbers."); }

			_milliSeconds = milliSeconds;
			_unitType = TimeUnitTypes.Milliseconds;
		}



		/// <summary>
		/// Creates a TimeUnit object from the specified TimeUnit string value (ie. 7m or 3h).  Note, the numeric part of the TimeUnit value must be an integer number.  The suffix will define the preferred UnitType display of the object.
		/// </summary>
		/// <param name="value"></param>
		[JsonConstructor]
		public TimeUnit (string value) {
			// First get last character of string.  Must be a letter.
			int len = value.Length;

			// Length must be > 2.
			if ( len < 2 ) {
				throw new ArgumentException(
					 "The value of TimeValue must be in the format of <number><TimeType> Where TimeType is a single character.");
			}

			char timeIncrement = value [len - 1];

			// Now get first part of string which is the numeric part.
			string timeDuration = new string(value.TakeWhile(d => !Char.IsLetter(d)).ToArray());


			// Validate we have a number and ultimately convert into a long for storing.
			if ( !long.TryParse(timeDuration, out long numericValue) ) {
				throw new ArgumentException(
					"Did not contain a valid numeric prefix.  Proper format is <Number><TimeType> where Number is an integer and TimeType is a single character.");
			}


			// To completely validate we have what they sent we build a new string from the 2 components and compare the 2.  Should be equal.
			string snew = numericValue.ToString() + timeIncrement;
			if ( snew != value ) {
				string msg = String.Format(
					"Argument is in an invalid format - [{0}].  Proper format is <Number><TimeType> where Number is an integer and TimeType is a single character.",
					value);
				throw new ArgumentException(msg);
			}


			// Now we just need to validate the time unit type is correct and convert to seconds.


			// Validate the unit of time is correct.
			switch ( timeIncrement ) {
				case 'd':
					_unitType = TimeUnitTypes.Days;
					_milliSeconds = MILLISECONDS_IN_DAY * numericValue;
					break;
				case 'm':
					_unitType = TimeUnitTypes.Minutes;
					_milliSeconds = MILLISECONDS_IN_MINUTE * numericValue;
					break;
				case 'h':
					_unitType = TimeUnitTypes.Hours;
					_milliSeconds = MILLISECONDS_IN_HOUR * numericValue;
					break;
				case 's':
					_unitType = TimeUnitTypes.Seconds;
					_milliSeconds = MILLISECONDS_IN_SECOND * numericValue;
					break;
				case 'w':
					_unitType = TimeUnitTypes.Weeks;
					_milliSeconds = MILLISECONDS_IN_WEEK * numericValue;
					break;
				case 'S': 
					_unitType = TimeUnitTypes.Milliseconds;
					_milliSeconds = numericValue;
					break;
				default: throw new ArgumentException("Invalid TimeUnitType specified.  Must be one of S,s,m,h,d,w.");
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


		/// <summary>
		/// Returns the largest TimeUnit Value that is a whole number.  For instance 59m would return 59m.  60m would return 1h.  24h would return 1d.
		/// </summary>
		[JsonIgnore]
		public string ValueAsWholeNumber {
			get => GetHighestWholeNumberUnitType((long) _milliSeconds);
		}



		/// <summary>
		/// Returns the numeric value of this TimeUnit.  If upon creation you specified a value of 9m (9 minutes) this function will return 9.
		/// </summary>
		[JsonIgnore]
		public long ValueAsNumeric {
			get => GetUnits(_unitType);
		}



		/// <summary>
		/// Validates that the timeIncrement character passed in is a valid TimeUnit type.
		/// </summary>
		/// <param name="timeIncrement">Char that should be inspected for a valid TimeUnit Type.</param>
		/// <returns>True if valid timeIncrement character code.  False otherwise.</returns>
		public static bool ValidateUnitTypeCharacter (char timeIncrement) {
			// Validate the unit of time is correct.
			return timeIncrement switch
			{
				'S' or 's' or 'm' or 'h' or 'd' or 'w' => true,
				_ => false
			};
		}



		#region "Object Overrides"

		public override bool Equals (object obj) {
			if ( !(obj is TimeUnit) ) { return false; }

			TimeUnit tu = (TimeUnit) obj;

			if ( tu._milliSeconds == _milliSeconds ) { return true; }
			else { return false; }
		}


		/// <summary>
		/// Returns the Hashcode for the milliseconds value
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode () { return _milliSeconds.GetHashCode(); }


		// Math comparison functions
		public static bool operator == (TimeUnit x, TimeUnit y) { return x._milliSeconds == y._milliSeconds; }
		public static bool operator != (TimeUnit x, TimeUnit y) { return x._milliSeconds != y._milliSeconds; }
		public static bool operator > (TimeUnit x, TimeUnit y) { return x.CompareTo(y) > 0; }
		public static bool operator < (TimeUnit x, TimeUnit y) { return x.CompareTo(y) < 0; }
		public static bool operator >= (TimeUnit x, TimeUnit y) { return x.CompareTo(y) >= 0; }
		public static bool operator <= (TimeUnit x, TimeUnit y) { return x.CompareTo(y) <= 0; }


		#endregion



		#region "InFX Functions"

		/// <summary>
		/// Returns the number of milliseconds this TimeUnit reporesents as a string
		/// </summary>
		[JsonIgnore]
		public string InMilliSecondsAsString {
			get { return (InMilliSecondsLong.ToString() + "S"); }
		}


		/// <summary>
		/// Returns the number of milli-seconds this TimeUnit represents
		/// </summary>
		[JsonIgnore]
		public long InMilliSecondsLong {
			get { return (long) GetUnits(TimeUnitTypes.Milliseconds); }
		}

		/// <summary>
		/// Returns the number of seconds this TimeUnit represents in Double format.
		/// </summary>
		[JsonIgnore]
		public double InSecondsAsDouble {
			get { return GetUnitsAsDouble(TimeUnitTypes.Seconds); }
		}


		/// <summary>
		/// Returns the TimeUnit value as a double seconds string. IE.  125s
		/// </summary>
		[JsonIgnore]
		public string InSecondsAsString {
			get { return (InSecondsAsDouble.ToString() + "s"); }
		}


		/// <summary>
		/// Returns the TimeUnit in seconds format, but as a long value.
		/// </summary>
		[JsonIgnore]
		public long InSecondsLong {
			get { return (long) GetUnits(TimeUnitTypes.Seconds); }
		}


		/// <summary>
		/// Returns the number of Minutes this TimeUnit represents.
		/// </summary>
		/// <returns></returns>
		[JsonIgnore]
		public double InMinutesAsDouble {
			get { return GetUnitsAsDouble(TimeUnitTypes.Minutes); }
		}


		/// <summary>
		///  Returns the TimeUnit in minutes as a string (ie. 6m)
		/// </summary>
		[JsonIgnore]
		public string InMinutesAsString {
			get { return (InMinutesAsDouble.ToString() + "m"); }
		}


		/// <summary>
		/// Returns the number of Hours this TimeUnit represents.
		/// </summary>
		/// <returns></returns>
		[JsonIgnore]
		public double InHoursAsDouble {
			get { return GetUnitsAsDouble(TimeUnitTypes.Hours); }
		}


		/// <summary>
		/// Returns the number of hours this timeunit represents as a string.  Ex.  29h
		/// </summary>
		[JsonIgnore]
		public string InHoursAsString {
			get { return (InHoursAsDouble.ToString() + "h"); }
		}


		/// <summary>
		/// Returns the number of days this TimeUnit represents as a double.
		/// </summary>
		/// <returns></returns>
		[JsonIgnore]
		public double InDaysAsDouble {
			get { return GetUnitsAsDouble(TimeUnitTypes.Days); }
		}


		/// <summary>
		/// Returns the number of days in string format.  Ex.  16d
		/// </summary>
		[JsonIgnore]
		public string InDaysAsString {
			get { return (InDaysAsDouble.ToString() + "d"); }
		}


		/// <summary>
		/// Returns the number of weeks this TimeUnit represents in double form.  Ex.  6.44
		/// </summary>
		/// <returns></returns>
		[JsonIgnore]
		public double InWeeksAsDouble {
			get { return GetUnitsAsDouble(TimeUnitTypes.Weeks); }
		}


		/// <summary>
		/// Returns the number of weeks this TimeUnit represents in string form:  6.4w
		/// </summary>
		/// <returns></returns>
		[JsonIgnore]
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
		/// Gets the number of units of the Unit Type.  Basically, just converts the internally stored millseconds into proper unit value.
		/// </summary>
		/// <returns>long - The number of units of the given UnitType</returns>
		private long GetUnits (TimeUnitTypes tuType) {
			return tuType switch
			{
				TimeUnitTypes.Seconds => _milliSeconds / 1000,
				TimeUnitTypes.Minutes => _milliSeconds / 60000,
				TimeUnitTypes.Hours => _milliSeconds / 3600000,
				TimeUnitTypes.Days => _milliSeconds / 86400000,
				TimeUnitTypes.Weeks => _milliSeconds / 604800000,
				_ => _milliSeconds
			};
		}


		private double GetUnitsAsDouble (TimeUnitTypes timeUnitType) {
			return timeUnitType switch
			{
				TimeUnitTypes.Seconds => _milliSeconds / 1000,
				TimeUnitTypes.Minutes =>_milliSeconds / 60000,
				TimeUnitTypes.Hours => _milliSeconds / 3600000,
				TimeUnitTypes.Days => _milliSeconds / 86400000,
				TimeUnitTypes.Weeks => _milliSeconds / 604800000,
				_ => _milliSeconds
			};
		}


		/// <summary>
		/// Returns the appropriate character representation for the TimeUnitTypes Enum value.  For example if TimeUnitTypes.Seconds returns 's'.
		/// </summary>
		/// <param name="timeUnitType">The TimeUnitType enum value to retrieve the character or string representation for.</param>
		/// <returns>string value of the TimeUnitType</returns>
		public static string GetTimeUnitTypeAsString (TimeUnitTypes timeUnitType) {
			return timeUnitType switch {
				TimeUnitTypes.Seconds =>  "s",
				TimeUnitTypes.Minutes => "m",
				TimeUnitTypes.Hours => "h",
				TimeUnitTypes.Days =>  "d",
				TimeUnitTypes.Weeks =>  "w",
				_ =>  "S"
			};
		}


		#endregion


		/// <summary>
		/// Returns the TimeUnit in a value that represents the largest unit value that results in a whole number.  For instance - 360 seconds would return 6m.  359 seconds would return 359 seconds.
		/// </summary>
		internal static string GetHighestWholeNumberUnitType (long milliSeconds) {
			long retNumeric;
			string retString;
			long remainder;


			// Try to convert to Weeks.
			if ( milliSeconds >= MILLISECONDS_IN_WEEK) {
				remainder = milliSeconds % MILLISECONDS_IN_WEEK;
				if ( remainder == 0 ) {
					retNumeric = milliSeconds / MILLISECONDS_IN_WEEK;
					retString = GetTimeUnitTypeAsString(TimeUnitTypes.Weeks);
					return (retNumeric.ToString() + retString);
				}
			}


			// Try to convert to days
			if ( milliSeconds >= MILLISECONDS_IN_DAY) {
				remainder = milliSeconds % MILLISECONDS_IN_DAY;
				if ( remainder == 0 ) {
					retNumeric = milliSeconds / MILLISECONDS_IN_DAY;
					retString = GetTimeUnitTypeAsString(TimeUnitTypes.Days);
					return (retNumeric.ToString() + retString);
				}
			}


			// Try to convert to Hours
			if ( milliSeconds >= MILLISECONDS_IN_HOUR) {
				remainder = milliSeconds % MILLISECONDS_IN_HOUR;
				if ( remainder == 0 ) {
					retNumeric = milliSeconds / MILLISECONDS_IN_HOUR;
					retString = GetTimeUnitTypeAsString(TimeUnitTypes.Hours);
					return (retNumeric.ToString() + retString);
				}
			}


			// Try to convert to minutes
			if ( milliSeconds >= MILLISECONDS_IN_MINUTE ) {
				remainder = milliSeconds % MILLISECONDS_IN_MINUTE;
				if ( remainder == 0 ) {
					retNumeric = milliSeconds / MILLISECONDS_IN_MINUTE;
					retString = GetTimeUnitTypeAsString(TimeUnitTypes.Minutes);
					return (retNumeric.ToString() + retString);
				}
			}

			// Try to convert to seconds
			if (milliSeconds >= MILLISECONDS_IN_SECOND)
			{
				remainder = milliSeconds % MILLISECONDS_IN_SECOND;
				if (remainder == 0)
				{
					retNumeric = milliSeconds / MILLISECONDS_IN_SECOND;
					retString = GetTimeUnitTypeAsString(TimeUnitTypes.Seconds);
					return (retNumeric.ToString() + retString);
				}
			}

			return (milliSeconds.ToString() + GetTimeUnitTypeAsString(TimeUnitTypes.Milliseconds));
		}



		/// <summary>
		/// Adds 2 TimeUnit types together to arrive at a 3rd TimeUnit object.  Will set the TimeUnit Type property to the largest WholeNumber value it can determine.  
		/// So, if a TimeUnit of 60s is added to a TimeUnit of 59m, will result in a TimeUnit of 1h.
		/// </summary>
		/// <param name="a">1st TimeUnit object</param>
		/// <param name="b">2nd TimeUnit object</param>
		/// <returns>Result of adding the 2 TimeUnits together.</returns>
		public static TimeUnit operator + (TimeUnit a, TimeUnit b) {
			long newMilliSeconds = a._milliSeconds + b._milliSeconds;
			string newValue = GetHighestWholeNumberUnitType(newMilliSeconds);
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
			long newMilliSeconds = a._milliSeconds - b._milliSeconds;
			if ( newMilliSeconds < 0 ) { newMilliSeconds = 0; }

			string newValue = GetHighestWholeNumberUnitType(newMilliSeconds);
			return (new TimeUnit(newValue));
		}



		#region "DateMath Functions"


		/// <summary>
		/// Adds the Current TimeUnit value to the date provided and returns a new DateTime object.
		/// </summary>
		/// <param name="dateTime">DateTime object to be used as the starting date and time.</param>
		/// <returns>Datetime object with the current TimeUnit value added to the datetime provided.</returns>
		public DateTime AddToDate (DateTime dateTime) { return dateTime.AddMilliseconds(_milliSeconds); }



		/// <summary>
		/// Subtracts the Current TimeUnit value from the date provided and returns a new DateTime object.
		/// </summary>
		/// <param name="dateTime">DateTime object to be used as the starting date and time.</param>
		/// <returns>Datetime object with the current TimeUnit subtracted from the datetime provided.</returns>
		public DateTime SubtractFromDate (DateTime dateTime) { return dateTime.AddMilliseconds(-(_milliSeconds)); }


		#endregion


		#region "Math Functions"


		/// <summary>
		/// Adds the given number of milli seconds to current value
		/// </summary>
		/// <param name="milliSeconds">Number of milliseconds to add.  Provide negative value to subtract</param>
		/// <returns></returns>
		public TimeUnit AddMilliseconds (long milliSeconds) {
			long calcSeconds;
			if ( milliSeconds < 0 ) { return SubtractMilliSeconds(-milliSeconds); }
			else { calcSeconds = _milliSeconds + milliSeconds; }

			string newValue = GetHighestWholeNumberUnitType(calcSeconds);
			return new TimeUnit(newValue);
		}



		/// <summary>
		/// Add the given number of seconds to the TimeUnit
		/// </summary>
		/// <param name="seconds">Number of Seconds to add.  Provide negative value to subtract</param>
		/// <returns></returns>
		public TimeUnit AddSeconds (long seconds) {
			long calcSeconds;
			if ( seconds < 0 ) { return SubtractSeconds(-seconds); }
			else { calcSeconds = _milliSeconds + (seconds * MILLISECONDS_IN_SECOND); }

			string newValue = GetHighestWholeNumberUnitType(calcSeconds);
			return new TimeUnit(newValue);
		}



		/// <summary>
		/// Add the given number of minutes to the TimeUnit
		/// </summary>
		/// <param name="minutes">Number of Minutes to add.  Negative values will subtract</param>
		/// <returns></returns>
		public TimeUnit AddMinutes (long minutes) {
			long calcSeconds;
			if ( minutes < 0 ) { return SubtractMinutes(-minutes); }
			else { calcSeconds = _milliSeconds + (minutes * MILLISECONDS_IN_MINUTE); }

			string newValue = GetHighestWholeNumberUnitType((long) calcSeconds);
			return new TimeUnit(newValue);
		}



		/// <summary>
		/// Add the given number of hours to the TimeUnit
		/// </summary>
		/// <param name="hours">Number of Hours to add.  Negative values will subtract</param>
		/// <returns></returns>
		public TimeUnit AddHours (long hours) {
			long calcSeconds;
			if ( hours < 0 ) { return SubtractHours(-hours); }
			else { calcSeconds = _milliSeconds + hours * MILLISECONDS_IN_HOUR; }

			string newValue = GetHighestWholeNumberUnitType(calcSeconds);
			return new TimeUnit(newValue);
		}



		/// <summary>
		/// Add the given number of Days to the TimeUnit
		/// </summary>
		/// <param name="days">Number of Days to add.  Negative values will subtract</param>
		/// <returns></returns>
		public TimeUnit AddDays (long days) {
			long calcSeconds;
			if ( days < 0 ) { return SubtractDays(-days); }
			else { calcSeconds = _milliSeconds + days * MILLISECONDS_IN_DAY; }

			string newValue = GetHighestWholeNumberUnitType((long) calcSeconds);
			return new TimeUnit(newValue);
		}


		/// <summary>
		/// Add the given number of Weeks to the TimeUnit
		/// </summary>
		/// <param name="weeks">Number of Weeks to add.  Negative values will subtract</param>
		/// <returns></returns>
		public TimeUnit AddWeeks(long weeks)
		{
			long calcSeconds;
			if (weeks < 0) { return SubtractWeeks(-weeks); }
			else { calcSeconds = _milliSeconds + weeks * MILLISECONDS_IN_WEEK; }

			string newValue = GetHighestWholeNumberUnitType((long)calcSeconds);
			return new TimeUnit(newValue);
		}



		/// <summary>
		/// Subtract the number of Milliseconds from the TimeUnit.  If the resulting value goes below 0, it is set to OS
		/// </summary>
		/// <param name="milliSeconds">Number of milliseconds to subtract</param>
		/// <returns></returns>
		public TimeUnit SubtractMilliSeconds(long milliSeconds)
		{
			double calcSeconds;
			if ( milliSeconds > _milliSeconds ) { calcSeconds = 0; }
			else { calcSeconds = _milliSeconds - milliSeconds; }

			string newValue = GetHighestWholeNumberUnitType((long)calcSeconds);
			return new TimeUnit(newValue);
		}



		/// <summary>
		/// Subtract the number of seconds from the TimeUnit.    If the resulting value goes below 0, it is set to OS
		/// </summary>
		/// <param name="seconds">Number of Seconds to subtract</param>
		/// <returns></returns>
		public TimeUnit SubtractSeconds (long seconds) {
			long val = seconds * MILLISECONDS_IN_SECOND;
			double calcSeconds;
			if ( val > _milliSeconds ) { calcSeconds = 0; }
			else { calcSeconds = _milliSeconds - val; }

			string newValue = GetHighestWholeNumberUnitType((long) calcSeconds);
			return new TimeUnit(newValue);
		}


		/// <summary>
		/// Subtract the number of minutes to the TimeUnit.  If the resulting value goes below 0, it is set to OS
		/// </summary>
		/// <param name="minutes">Number of Minutes to subtract.</param>
		/// <returns></returns>
		public TimeUnit SubtractMinutes (long minutes) {
			long val = minutes * MILLISECONDS_IN_MINUTE;
			double calcSeconds;
			if ( val > _milliSeconds ) { calcSeconds = 0; }
			else { calcSeconds = _milliSeconds - val; }

			string newValue = GetHighestWholeNumberUnitType((long) calcSeconds);
			return new TimeUnit(newValue);
		}


		/// <summary>
		/// Subtract the number of hours to the TimeUnit.  If the resulting value goes below 0, it is set to OS
		/// </summary>
		/// <param name="hours">Number of Hours to subtract</param>
		/// <returns></returns>
		public TimeUnit SubtractHours (long hours) {
			double val = hours * MILLISECONDS_IN_HOUR;
			double calcSeconds;
			if ( val > _milliSeconds ) { calcSeconds = 0; }
			else { calcSeconds = _milliSeconds - val; }

			string newValue = GetHighestWholeNumberUnitType((long) calcSeconds);
			return new TimeUnit(newValue);
		}


		/// <summary>
		/// Subtract the given number of days from the TimeUnit.  If the resulting value goes below 0, it is set to OS
		/// </summary>
		/// <param name="days">Number of days to subtract</param>
		/// <returns></returns>
		public TimeUnit SubtractDays (long days) {
			double val = days * MILLISECONDS_IN_DAY;
			double calcSeconds;
			if ( val > _milliSeconds ) { calcSeconds = 0; }
			else { calcSeconds = _milliSeconds - val; }

			string newValue = GetHighestWholeNumberUnitType((long) calcSeconds);
			return new TimeUnit(newValue);
		}



		/// <summary>
		/// Subtract the given number of weeks from the TimeUnit.  If the resulting value goes below 0, it is set to OS
		/// </summary>
		/// <param name="weeks">Number of weeks to subtract</param>
		/// <returns></returns>
		public TimeUnit SubtractWeeks(long weeks)
		{
			double val = weeks * MILLISECONDS_IN_WEEK;
			double calcSeconds;
			if (val > _milliSeconds) { calcSeconds = 0; }
			else { calcSeconds = _milliSeconds - val; }

			string newValue = GetHighestWholeNumberUnitType((long)calcSeconds);
			return new TimeUnit(newValue);
		}


		#endregion


		/// <summary>
		/// For IEquatable Interface
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals (TimeUnit other) { return (_milliSeconds.Equals(other._milliSeconds)); }


		/// <summary>
		/// Used for IComparable Interface
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo (TimeUnit other) { return _milliSeconds.CompareTo(other); }


		/// <summary>
		/// Enable Serialization
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("tu", this.Value);
		}


		public TimeUnit (SerializationInfo info, StreamingContext context) {
			string ts = info.GetString("tu");
			this = new TimeUnit(ts);
		}



		// Allow direct setting to/from string
		public static implicit operator string (TimeUnit timeUnit) { return timeUnit.Value; }
		public static implicit operator TimeUnit (string s) { return new TimeUnit(s); }


		// Allow direct setting to/from an integer
		public static implicit operator int (TimeUnit timeUnit) { return (int) timeUnit._milliSeconds; }
		public static implicit operator TimeUnit (int s) { return new TimeUnit(s); }
	}
}